using System.Data;
using Microsoft.Extensions.DependencyInjection;
using QueryTerminal.Data;
using QueryTerminal.OutputFormatting;
using Spectre.Console;

namespace QueryTerminal.CommandHandling;

public abstract class BaseCommandHandler<TConnection> where TConnection : IDbConnection
{
    protected IServiceProvider _serviceProvider;
    protected string _connectionString;
    protected IOutputFormatter _outputFormatter;

    public abstract TConnection Connect(string connectionString);
    public abstract IQueryExecutor<TConnection> QueryExecutor { get; }

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

    public async Task Run(string? sqlQuery, CancellationToken cancellationToken)
    {
        using var conn = Connect(_connectionString);
        conn.Open();

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
                using var reader = await QueryExecutor.Execute(conn, sqlQuery, cancellationToken);
                _outputFormatter.WriteOutput(reader);
            }
        }
        else
        {
            using var reader = await QueryExecutor.Execute(conn, sqlQuery, cancellationToken);
            _outputFormatter.WriteOutput(reader);
        }
    }
}

