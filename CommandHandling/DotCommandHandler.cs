using System.Collections.Immutable;
using System.Data.Common;
using QueryTerminal.Data;
using Spectre.Console;

namespace QueryTerminal.CommandHandling;

public class DotCommandHandler<TConnection> where TConnection : DbConnection
{
    private readonly RootCommandHandler _rootCommandHandler;
    private readonly TConnection _connection;
    private readonly IDictionary<string, Func<string[], CancellationToken, Task>> _dotCommands;
    private readonly IDbMetadataProvider<TConnection> _metadataProvider;

    public DotCommandHandler(RootCommandHandler rootCommandHandler, TConnection connection, IDbMetadataProvider<TConnection> metadataProvider)
    {
        _rootCommandHandler = rootCommandHandler;
        _connection = connection;
        _dotCommands = BuildDotCommands();
        _metadataProvider = metadataProvider;
    }

    private IDictionary<string, Func<string[], CancellationToken, Task>> BuildDotCommands() => ImmutableDictionary.CreateRange(
        new KeyValuePair<string, Func<string[], CancellationToken, Task>>[] {
            KeyValuePair.Create<string, Func<string[], CancellationToken, Task>>(".exit", Exit),
            KeyValuePair.Create<string, Func<string[], CancellationToken, Task>>(".listTables", ListTables),
            KeyValuePair.Create<string, Func<string[], CancellationToken, Task>>(".listColumns", ListColumns)
        }
    );

    public async Task Handle(string commandText, CancellationToken cancellationToken)
    {
        var tokens = commandText.Split(' ');
        if (!_dotCommands.ContainsKey(tokens.First()))
        {
            throw new ArgumentException($"No command found matching {tokens.First()}");
        }
        await _dotCommands[tokens.First()](tokens.Skip(1).ToArray(), cancellationToken);
    }

    public async Task Exit(string[] args, CancellationToken cancellationToken)
    {
        _rootCommandHandler.Terminate();
    }

    public async Task ListTables(string[] args, CancellationToken cancellationToken)
    {
        var dbTables = await _metadataProvider.GetTables(_connection, cancellationToken);
        var table = new Table();
        table.AddColumns($"[bold blue]Name[/]", "[bold blue]Type[/]");
        foreach (var dbTable in dbTables)
        {
            table.AddRow(dbTable.Name, dbTable.Type);
        }
        AnsiConsole.Write(table);
    }

    public async Task ListColumns(string[] args, CancellationToken cancellationToken)
    {
        var dbColumns = await _metadataProvider.GetColumns(_connection, args.Single(), cancellationToken);
        var table = new Table();
        table.AddColumns($"[bold blue]Name[/]", "[bold blue]Type[/]");
        foreach (var dbColumn in dbColumns)
        {
            table.AddRow(dbColumn.Name, dbColumn.Type);
        }
        AnsiConsole.Write(table);
    }
}
