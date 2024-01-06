using System.Data;
using Microsoft.Extensions.DependencyInjection;
using QueryTerminal.Data;
using QueryTerminal.OutputFormatting;
using Spectre.Console;

namespace QueryTerminal.CommandHandling;

public abstract class BaseHandler
{
    protected IServiceProvider _serviceProvider;
    protected string? _connectionString;
    protected IOutputFormatter _outputFormatter;

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

    public async Task Run<TConnection>(string? sqlQuery, CancellationToken cancellationToken) where TConnection : IDbConnection
    {
        using var executor = _serviceProvider.GetRequiredService<IQueryExecutor<TConnection>>();
        executor.Connect(_connectionString);

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
                using var reader = await executor.Execute(sqlQuery, cancellationToken);
                _outputFormatter.WriteOutput(reader);
            }
        }
        else
        {
            using var reader = await executor.Execute(sqlQuery, cancellationToken);
            _outputFormatter.WriteOutput(reader);
        }
    }
}

