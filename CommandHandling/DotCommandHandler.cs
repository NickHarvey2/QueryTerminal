using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using QueryTerminal.Data;
using QueryTerminal.OutputFormatting;
using Spectre.Console;

namespace QueryTerminal.CommandHandling;

public class DotCommandHandler : IAsyncDisposable
{
    private readonly RootCommandHandler _rootCommandHandler;
    private readonly IQueryTerminalDbConnection _connection;
    private readonly IDictionary<string, Func<ImmutableArray<string>, CancellationToken, Task>> _dotCommands;

    public DotCommandHandler(IServiceProvider serviceProvider)
    {
        _rootCommandHandler = serviceProvider.GetRequiredService<RootCommandHandler>();
        _connection = serviceProvider.GetRequiredService<IQueryTerminalDbConnection>();
        _dotCommands = BuildDotCommands();
    }

    private IDictionary<string, Func<ImmutableArray<string>, CancellationToken, Task>> BuildDotCommands() => ImmutableDictionary.CreateRange(
        new KeyValuePair<string, Func<ImmutableArray<string>, CancellationToken, Task>>[] {
            KeyValuePair.Create<string, Func<ImmutableArray<string>, CancellationToken, Task>>(".exit", Exit),
            KeyValuePair.Create<string, Func<ImmutableArray<string>, CancellationToken, Task>>(".listTables", ListTables),
            KeyValuePair.Create<string, Func<ImmutableArray<string>, CancellationToken, Task>>(".listColumns", ListColumns),
            KeyValuePair.Create<string, Func<ImmutableArray<string>, CancellationToken, Task>>(".listOutputFormats", ListOutputFormats),
            KeyValuePair.Create<string, Func<ImmutableArray<string>, CancellationToken, Task>>(".getOutputFormat", GetOutputFormat),
            KeyValuePair.Create<string, Func<ImmutableArray<string>, CancellationToken, Task>>(".setOutputFormat", SetOutputFormat),
        }
    );

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        await _connection.OpenAsync(cancellationToken);
    }

    public async Task Handle(string commandText, CancellationToken cancellationToken)
    {
        var tokens = commandText.Split(' ');
        if (!_dotCommands.ContainsKey(tokens.First()))
        {
            throw new ArgumentException($"No command found matching {tokens.First()}");
        }
        await _dotCommands[tokens.First()](tokens.Skip(1).ToImmutableArray(), cancellationToken);
    }

    public Task Exit(ImmutableArray<string> args, CancellationToken cancellationToken)
    {
        _rootCommandHandler.Terminate();
        return Task.CompletedTask;
    }

    public async Task ListTables(ImmutableArray<string> args, CancellationToken cancellationToken)
    {
        var dbTables = await _connection.GetTablesAsync(cancellationToken);
        var table = new Table();
        table.AddColumns($"[bold blue]Name[/]", "[bold blue]Type[/]");
        foreach (var dbTable in dbTables)
        {
            table.AddRow(dbTable.Name, dbTable.Type);
        }
        AnsiConsole.Write(table);
    }

    public async Task ListColumns(ImmutableArray<string> args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Required parameter missing: tableName");
            return;
        }
        var dbColumns = await _connection.GetColumnsAsync(args[0], cancellationToken);
        var table = new Table();
        table.AddColumns($"[bold blue]Name[/]", "[bold blue]Type[/]");
        foreach (var dbColumn in dbColumns)
        {
            table.AddRow(dbColumn.Name, dbColumn.Type);
        }
        AnsiConsole.Write(table);
    }

    public Task ListOutputFormats(ImmutableArray<string> args, CancellationToken cancellationToken)
    {
        var table = new Table();
        table.AddColumns($"[bold blue]Format[/]", "[bold blue]Description[/]");
        foreach (var outputFormat in OutputFormat.List())
        {
            table.AddRow(outputFormat.Name, outputFormat.Description);
        }
        AnsiConsole.Write(table);
        return Task.CompletedTask;
    }

    public Task GetOutputFormat(ImmutableArray<string> args, CancellationToken cancellationToken)
    {
        var table = new Table();
        table.AddColumns($"[bold blue]Format[/]", "[bold blue]Description[/]");
        table.AddRow(_rootCommandHandler.OutputFormatter.Name, _rootCommandHandler.OutputFormatter.Description);
        AnsiConsole.Write(table);
        return Task.CompletedTask;
    }

    public Task SetOutputFormat(ImmutableArray<string> args, CancellationToken cancellationToken)
    {
        _rootCommandHandler.SetOutputFormatByName(args[0]);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
