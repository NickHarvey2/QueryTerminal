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
    private readonly IDotCommands _dotCommands;
    private readonly IOutputFormats _outputFormats;
    private bool _terminate = false;

    public RootCommandHandler(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _outputFormats = _serviceProvider.GetRequiredService<IOutputFormats>();
    }

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
            await using var dotCommands = _serviceProvider.GetRequiredService<IDotCommands>();
            await dotCommands.OpenAsync(cancellationToken);
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
                        var tokens = query.Split(' ');
                        _terminate = dotCommands.Get(tokens.First()).Invoke(tokens.Skip(1).ToArray());
                    }
                    else
                    {
                        await using var reader = await connection.ExecuteQueryAsync(query, cancellationToken);
                        _outputFormats.GetCurrent().WriteOutput(reader);
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
            _outputFormats.GetCurrent().WriteOutput(reader);
        }
    }
}
