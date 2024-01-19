using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QueryTerminal.Data;
using QueryTerminal.OutputFormatting;
using QueryTerminal.Prompting;
using Spectre.Console;

namespace QueryTerminal.CommandHandling;

public class RootCommandHandler
{
    private IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private IOutputFormatter _outputFormatter;
    private string? _query;
    private bool _terminate = false;

    public RootCommandHandler(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _query = configuration["query"];
        SetOutputFormatByName(configuration["outputFormat"]);
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

    public async Task Run(CancellationToken cancellationToken)
    {
        await using var connection = _serviceProvider.GetRequiredKeyedService<IQueryTerminalDbConnection>(_configuration["type"]);
        await connection.ConnectAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(_query))
        {
            await using var dotCommandHandler = _serviceProvider.GetRequiredService<DotCommandHandler>();
            await dotCommandHandler.Initialize(cancellationToken);
            await using var prompt = _serviceProvider.GetRequiredService<QueryTerminalPrompt>();
            while (!_terminate)
            {
                var result = await prompt.ReadLineAsync();
                if (!result.IsSuccess)
                {
                    continue;
                }
                _query = result.Text;
                if (string.IsNullOrWhiteSpace(_query))
                {
                    continue;
                }
                try
                {
                    if (_query.StartsWith("."))
                    {
                        await dotCommandHandler.Handle(_query, cancellationToken);
                    }
                    else
                    {
                        await using var reader = await connection.ExecuteQueryAsync(_query, cancellationToken);
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
            await using var reader = await connection.ExecuteQueryAsync(_query, cancellationToken);
            _outputFormatter.WriteOutput(reader);
        }
    }
}
