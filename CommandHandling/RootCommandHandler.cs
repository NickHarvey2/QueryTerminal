using System.Data;
using Microsoft.Extensions.DependencyInjection;
using QueryTerminal.OutputFormatting;
using Spectre.Console;

namespace QueryTerminal.CommandHandling;

public class RootCommandHandler<TConnection> where TConnection : IDbConnection, new()
{
    private readonly string _connectionString;
    private IOutputFormatter _outputFormatter;
    private readonly IServiceProvider _serviceProvider;

    public RootCommandHandler(string? connectionString, string outputFormat, IServiceProvider serviceProvider)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException("connectionString");
        }
        _connectionString = connectionString;
        _serviceProvider = serviceProvider;
        _outputFormatter = _serviceProvider.GetRequiredKeyedService<IOutputFormatter>(outputFormat);
    }

    public async Task Run(string? sqlQuery, CancellationToken cancellationToken)
    {
        using TConnection conn = new TConnection();
        conn.ConnectionString = _connectionString;
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
