using System.Collections.Immutable;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using QueryTerminal.Data;
using QueryTerminal.OutputFormatting;
using Spectre.Console;

namespace QueryTerminal.CommandHandling;

public class RootCommandHandler
{
    private IServiceProvider _serviceProvider;
    private string? _connectionString;
    private IOutputFormatter _outputFormatter;
    private bool _terminate = false;
    private IDictionary<string, Func<DbConnection, string[], CancellationToken, Task>> _dotCommands;

    public RootCommandHandler(string? connectionString, IServiceProvider serviceProvider)
    {
        _connectionString = connectionString;
        _serviceProvider = serviceProvider;
        SetOutputFormat("csv");
        _dotCommands = BuildDotCommands();
    }

    public void SetOutputFormat(string outputFormat)
    {
        try
        {
            _outputFormatter = _serviceProvider.GetRequiredKeyedService<IOutputFormatter>(outputFormat);
        }
        catch (InvalidOperationException ioe)
        {
            throw new InvalidOperationException($"No output formatter found for key '{outputFormat}'", ioe);
        }
    }

    public async Task Run<TConnection>(string? commandText, CancellationToken cancellationToken) where TConnection : DbConnection
    {
        using var connection = _serviceProvider.GetRequiredService<IDbConnectionProvider<TConnection>>().Connect(_connectionString);
        await connection.OpenAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(commandText))
        {
            while (!_terminate)
            {
                // TODO: this is the most basic possible implementation that still does _something_, and is missing several key features
                commandText = AnsiConsole.Prompt(new TextPrompt<string>(">"));
                if (string.IsNullOrWhiteSpace(commandText))
                {
                    continue;
                }
                try
                {
                    if (commandText.StartsWith("."))
                    {
                        await HandleDotCommand(connection, commandText, cancellationToken);
                    }
                    else
                    {
                        using var reader = await HandleSqlCommand(connection, commandText, cancellationToken);
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
            using var reader = await HandleSqlCommand(connection, commandText, cancellationToken);
            _outputFormatter.WriteOutput(reader);
        }
    }

    private async Task HandleDotCommand(DbConnection connection, string commandText, CancellationToken cancellationToken)
    {
        var tokens = commandText.Split(' ');
        if (!_dotCommands.ContainsKey(tokens.First()))
        {
            throw new ArgumentException($"No command found matching {tokens.First()}");
        }
        await _dotCommands[tokens.First()](connection, tokens.Skip(1).ToArray(), cancellationToken);
    }

    private async Task<DbDataReader> HandleSqlCommand(DbConnection connection, string commandText, CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText = commandText;
        return await command.ExecuteReaderAsync(cancellationToken);
    }

    private IDictionary<string, Func<DbConnection, string[], CancellationToken, Task>> BuildDotCommands() => ImmutableDictionary.CreateRange(
        new KeyValuePair<string, Func<DbConnection, string[], CancellationToken, Task>>[] {
            KeyValuePair.Create<string, Func<DbConnection, string[], CancellationToken, Task>>(".exit", Exit),
        }
    );

    private async Task Exit(DbConnection connection, string[] args, CancellationToken cancellationToken) => _terminate = true;
}
