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
    private readonly IOutputFormats _outputFormats;
    private string _outputFormat;
    private bool _terminate = false;

    public RootCommandHandler(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _outputFormat = configuration["outputFormat"];
        _outputFormats = _serviceProvider.GetRequiredService<IOutputFormats>();
    }

    public void SetOutputFormatByName(string outputFormatName)
    {
        _outputFormat = outputFormatName;
    }

    public IOutputFormatter OutputFormatter { get => _outputFormats.Get(_outputFormat); }

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
                        _outputFormats.Get(_outputFormat).WriteOutput(reader);
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
            _outputFormats.Get(_outputFormat).WriteOutput(reader);
        }
    }
}
