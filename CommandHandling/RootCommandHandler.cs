using System.Data;
using Microsoft.Extensions.DependencyInjection;
using QueryTerminal.Data;
using QueryTerminal.OutputFormatting;
using Spectre.Console;

namespace QueryTerminal.CommandHandling;

public class RootCommandHandler<TConnection> where TConnection : IDbConnection
{
    private readonly string _connectionString;
    private IOutputFormatter _outputFormatter;
    private readonly IServiceProvider _serviceProvider;

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

    public RootCommandHandler(string? connectionString, IServiceProvider serviceProvider)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException("connectionString");
        }
        _connectionString = connectionString;
        _serviceProvider = serviceProvider;
        SetOutputFormat("csv");
    }

    public async Task Run(string? sqlQuery, CancellationToken cancellationToken)
    {
        using var conn = _serviceProvider.GetRequiredService<IDbConnectionProvider<TConnection>>().Connect(_connectionString);
        conn.Open();
        var executor = _serviceProvider.GetRequiredService<IQueryExecutor<TConnection>>();

        if (string.IsNullOrWhiteSpace(sqlQuery))
        {
            while (true)
            {
                // TODO: this is the most basic possible implementation that still does _something_, and is missing several key features
                sqlQuery = AnsiConsole.Prompt(new TextPrompt<string>(">"));
                if (sqlQuery.StartsWith("."))
                {
                    if (sqlQuery == ".exit")
                    {
                        break;
                    }
                }
                using var reader = await executor.Execute(conn, sqlQuery, cancellationToken);
                _outputFormatter.WriteOutput(reader);
            }
        }
        else
        {
            using var reader = await executor.Execute(conn, sqlQuery, cancellationToken);
            _outputFormatter.WriteOutput(reader);
        }
    }
}
