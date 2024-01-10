using System.Collections.Immutable;
using System.Data;
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
    private IDictionary<string, Action<string[]>> _dotCommands;

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

    public async Task Run<TConnection>(string? command, CancellationToken cancellationToken) where TConnection : IDbConnection
    {
        using var executor = _serviceProvider.GetRequiredService<IQueryExecutor<TConnection>>();
        executor.Connect(_connectionString);

        if (string.IsNullOrWhiteSpace(command))
        {
            while (!_terminate)
            {
                // TODO: this is the most basic possible implementation that still does _something_, and is missing several key features
                command = AnsiConsole.Prompt(new TextPrompt<string>(">"));
                try
                {
                    if (command.StartsWith("."))
                    {
                        var tokens = command.Split(' ');
                        if (!_dotCommands.ContainsKey(tokens.First()))
                        {
                            throw new ArgumentException($"No command found matching {tokens.First()}");
                        }
                        _dotCommands[tokens.First()].Invoke(tokens.Skip(1).ToArray());
                    }
                    else
                    {
                        using var reader = await executor.Execute(command, cancellationToken);
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
            using var reader = await executor.Execute(command, cancellationToken);
            _outputFormatter.WriteOutput(reader);
        }
    }

    private IDictionary<string, Action<string[]>> BuildDotCommands() => ImmutableDictionary.CreateRange(
        new KeyValuePair<string, Action<string[]>>[] {
            KeyValuePair.Create<string, Action<string[]>>(".exit", Exit)
        }
    );

    private void Exit(string[] _) => _terminate = true;
}

