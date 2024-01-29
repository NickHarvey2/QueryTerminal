using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QueryTerminal.Data;
using QueryTerminal.OutputFormatting;
using QueryTerminal.Prompting;
using Spectre.Console;

namespace QueryTerminal.CommandHandling;

public class RootCommandHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private IOutputFormatter _outputFormatter;
    private bool _terminate = false;

    public RootCommandHandler(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        SetOutputFormatByName(_configuration["outputFormat"]);
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

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await using var connection = _serviceProvider.GetRequiredService<IQueryTerminalDbConnection>();
        await connection.OpenAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(_configuration["query"]))
        {
            await using var dotCommandHandler = _serviceProvider.GetRequiredService<DotCommandHandler>();
            await dotCommandHandler.OpenAsync(cancellationToken);
            await using var prompt = _serviceProvider.GetRequiredService<QueryTerminalPrompt>();
            await prompt.OpenAsync(cancellationToken);
            while (!_terminate)
            {
                var result = await prompt.ReadLineAsync();
                if (!result.IsSuccess)
                {
                    continue;
                }
                var query = result.Text.Trim();
                if (string.IsNullOrWhiteSpace(query))
                {
                    continue;
                }
                try
                {
                    if (query.StartsWith("."))
                    {
                        dotCommandHandler.Handle(query);
                    }
                    else
                    {
                        await using var reader = await connection.ExecuteQueryAsync(query, cancellationToken);
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
            await using var reader = await connection.ExecuteQueryAsync(_configuration["query"], cancellationToken);
            _outputFormatter.WriteOutput(reader);
        }
    }
}
