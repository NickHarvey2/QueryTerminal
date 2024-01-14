using System.Collections.Immutable;
using System.Data.Common;
using QueryTerminal.Data;
using QueryTerminal.OutputFormatting;
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
            KeyValuePair.Create<string, Func<string[], CancellationToken, Task>>(".listColumns", ListColumns),
            KeyValuePair.Create<string, Func<string[], CancellationToken, Task>>(".listOutputFormats", ListOutputFormats),
            KeyValuePair.Create<string, Func<string[], CancellationToken, Task>>(".getOutputFormat", GetOutputFormat),
            KeyValuePair.Create<string, Func<string[], CancellationToken, Task>>(".setOutputFormat", SetOutputFormat),
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
        if (args.Length == 0)
        {
            Console.WriteLine("Required parameter missing: tableName");
            return;
        }
        var dbColumns = await _metadataProvider.GetColumns(_connection, args[0], cancellationToken);
        var table = new Table();
        table.AddColumns($"[bold blue]Name[/]", "[bold blue]Type[/]");
        foreach (var dbColumn in dbColumns)
        {
            table.AddRow(dbColumn.Name, dbColumn.Type);
        }
        AnsiConsole.Write(table);
    }

    public async Task ListOutputFormats(string[] args, CancellationToken cancellationToken)
    {
        var table = new Table();
        table.AddColumns($"[bold blue]Format[/]", "[bold blue]Description[/]");
        foreach (var outputFormat in OutputFormat.List())
        {
            table.AddRow(outputFormat.Name, outputFormat.Description);
        }
        AnsiConsole.Write(table);
    }

    public async Task GetOutputFormat(string[] args, CancellationToken cancellationToken)
    {
        var table = new Table();
        table.AddColumns($"[bold blue]Format[/]", "[bold blue]Description[/]");
        table.AddRow(_rootCommandHandler.OutputFormatter.Name, _rootCommandHandler.OutputFormatter.Description);
        AnsiConsole.Write(table);
    }

    public async Task SetOutputFormat(string[] args, CancellationToken cancellationToken)
    {
        _rootCommandHandler.SetOutputFormatByName(args[0]);
    }
}
