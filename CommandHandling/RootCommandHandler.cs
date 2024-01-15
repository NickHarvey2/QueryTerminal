using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using QueryTerminal.Data;
using QueryTerminal.OutputFormatting;
using QueryTerminal.Prompting;
using Spectre.Console;

namespace QueryTerminal.CommandHandling;

public class RootCommandHandler
{
    private IServiceProvider _serviceProvider;
    private string? _connectionString;
    private IOutputFormatter _outputFormatter;
    private bool _terminate = false;

    public RootCommandHandler(string? connectionString, IServiceProvider serviceProvider)
    {
        _connectionString = connectionString;
        _serviceProvider = serviceProvider;
        SetOutputFormatByName("csv");
    }

    public void SetOutputFormatByName(string outputFormatName)
    {
        var newOutputFormatter = OutputFormat.Get(outputFormatName);
        if (newOutputFormatter is null)
        {
            throw new InvalidOperationException($"No output formatter found for key '{outputFormatName}'");
        }
        _outputFormatter = newOutputFormatter;
    }

    public IOutputFormatter OutputFormatter { get => _outputFormatter; }

    public void Terminate()
    {
        _terminate = true;
    }

    public async Task Run<TConnection>(string? commandText, CancellationToken cancellationToken) where TConnection : DbConnection, new()
    {
        // using var connection = _serviceProvider.GetRequiredService<IDbConnectionProvider<TConnection>>().Connect(_connectionString);
        // await connection.OpenAsync(cancellationToken);
        await using var connection = _serviceProvider.GetRequiredService<QueryTerminalDbConnection<TConnection>>();
        await connection.ConnectAsync(_connectionString, cancellationToken);

        if (string.IsNullOrWhiteSpace(commandText))
        {
            var dotCommandHandler = _serviceProvider.GetRequiredService<DotCommandHandlerFactory<TConnection>>().Create(this, connection);
            await using var prompt = _serviceProvider.GetRequiredService<QueryTerminalPrompt>();
            while (!_terminate)
            {
                var result = await prompt.ReadLineAsync();
                if (!result.IsSuccess)
                {
                    continue;
                }
                commandText = result.Text;
                if (string.IsNullOrWhiteSpace(commandText))
                {
                    continue;
                }
                try
                {
                    if (commandText.StartsWith("."))
                    {
                        await dotCommandHandler.Handle(commandText, cancellationToken);
                    }
                    else
                    {
                        // await HandleSqlCommand(connection, commandText, cancellationToken);
                        await using var reader = await connection.ExecuteQueryAsync(commandText, cancellationToken);
                        _outputFormatter.WriteOutput(reader);
                    }
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteException(e);
                }
            }
        }
        else
        {
            // await HandleSqlCommand(connection, commandText, cancellationToken);
            await using var reader = await connection.ExecuteQueryAsync(commandText, cancellationToken);
            _outputFormatter.WriteOutput(reader);
        }
    }

    private async Task HandleSqlCommand(DbConnection connection, string commandText, CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText = commandText;
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        _outputFormatter.WriteOutput(reader);
    }
}
