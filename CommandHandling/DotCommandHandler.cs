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
    private readonly IDictionary<string, Action<ImmutableArray<string>>> _dotCommands;

    public DotCommandHandler(IServiceProvider serviceProvider)
    {
        _rootCommandHandler = serviceProvider.GetRequiredService<RootCommandHandler>();
        _connection = serviceProvider.GetRequiredService<IQueryTerminalDbConnection>();
        _dotCommands = BuildDotCommands();
    }

    private IDictionary<string, Action<ImmutableArray<string>>> BuildDotCommands() => ImmutableDictionary.CreateRange(
        new KeyValuePair<string, Action<ImmutableArray<string>>>[] {
            KeyValuePair.Create<string, Action<ImmutableArray<string>>>(".exit", Exit),
            KeyValuePair.Create<string, Action<ImmutableArray<string>>>(".listTables", ListTables),
            KeyValuePair.Create<string, Action<ImmutableArray<string>>>(".listColumns", ListColumns),
            KeyValuePair.Create<string, Action<ImmutableArray<string>>>(".listOutputFormats", ListOutputFormats),
            KeyValuePair.Create<string, Action<ImmutableArray<string>>>(".getOutputFormat", GetOutputFormat),
            KeyValuePair.Create<string, Action<ImmutableArray<string>>>(".setOutputFormat", SetOutputFormat),
        }
    );

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        await _connection.OpenAsync(cancellationToken);
    }

    public void Handle(string commandText)
    {
        var tokens = commandText.Split(' ');
        if (!_dotCommands.ContainsKey(tokens.First()))
        {
            throw new ArgumentException($"No command found matching {tokens.First()}");
        }
        _dotCommands[tokens.First()](tokens.Skip(1).ToImmutableArray());
    }

    public void Exit(ImmutableArray<string> args)
    {
        _rootCommandHandler.Terminate();
    }

    public void ListTables(ImmutableArray<string> args)
    {
        var dbTables = _connection.GetTables();
        var table = new Table();
        table.AddColumns($"[bold blue]Name[/]", "[bold blue]Type[/]");
        foreach (var dbTable in dbTables)
        {
            table.AddRow(dbTable.Name, dbTable.Type);
        }
        AnsiConsole.Write(table);
    }

    public void ListColumns(ImmutableArray<string> args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Required parameter missing: tableName");
            return;
        }
        var dbColumns = _connection.GetColumns(args[0]);
        var table = new Table();
        table.AddColumns($"[bold blue]Name[/]", "[bold blue]Type[/]");
        foreach (var dbColumn in dbColumns)
        {
            table.AddRow(dbColumn.Name, dbColumn.Type);
        }
        AnsiConsole.Write(table);
    }

    public void ListOutputFormats(ImmutableArray<string> args)
    {
        var table = new Table();
        table.AddColumns($"[bold blue]Format[/]", "[bold blue]Description[/]");
        foreach (var outputFormat in OutputFormat.List())
        {
            table.AddRow(outputFormat.Name, outputFormat.Description);
        }
        AnsiConsole.Write(table);
    }

    public void GetOutputFormat(ImmutableArray<string> args)
    {
        var table = new Table();
        table.AddColumns($"[bold blue]Format[/]", "[bold blue]Description[/]");
        table.AddRow(_rootCommandHandler.OutputFormatter.Name, _rootCommandHandler.OutputFormatter.Description);
        AnsiConsole.Write(table);
    }

    public void SetOutputFormat(ImmutableArray<string> args)
    {
        _rootCommandHandler.SetOutputFormatByName(args[0]);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
