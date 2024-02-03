using System.Collections.Immutable;
using QueryTerminal.Data;
using QueryTerminal.OutputFormatting;
using Spectre.Console;

namespace QueryTerminal.CommandHandling;

public class DotCommands : IDotCommands, IAsyncDisposable
{
    private readonly IReadOnlyDictionary<string, IDotCommand> _commands;
    private readonly IQueryTerminalDbConnection _connection;
    private readonly IOutputFormats _outputFormats;

    public DotCommands(IQueryTerminalDbConnection connection, IOutputFormats outputFormats)
    {
        _connection = connection;
        _outputFormats = outputFormats;

        IDotCommand exit = new DotCommmand(
            name: ".exit",
            description: "Exit the program",
            command: args => true
        );

        IDotCommand listTables = new DotCommmand(
            name: ".listTables",
            description: "List all tables in the current schema",
            command: args =>
            {
                var dbTables = _connection.GetTables();
                var table = new Table();
                table.AddColumns($"[bold blue]Name[/]", "[bold blue]Type[/]");
                foreach (var dbTable in dbTables)
                {
                    table.AddRow(dbTable.Name, dbTable.Type);
                }
                AnsiConsole.Write(table);
                return false;
            }
        );

        IDotCommand listColumns = new DotCommmand(
            name: ".listColumns",
            description: "List the columns in the specified table",
            command: args =>
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Required parameter missing: tableName");
                    return false;
                }
                var dbColumns = _connection.GetColumns(args[0]);
                var table = new Table();
                table.AddColumns($"[bold blue]Name[/]", "[bold blue]Type[/]");
                foreach (var dbColumn in dbColumns)
                {
                    table.AddRow(dbColumn.Name, dbColumn.Type);
                }
                AnsiConsole.Write(table);
                return false;
            }
        );

        IDotCommand listOutputFormats = new DotCommmand(
            name: ".listOutputFormats",
            description: "List the available output formats",
            command: args =>
            {
                var table = new Table();
                table.AddColumns($"[bold blue]Format[/]", "[bold blue]Description[/]");
                foreach (var outputFormat in _outputFormats.List())
                {
                    table.AddRow(outputFormat.Name, outputFormat.Description);
                }
                AnsiConsole.Write(table);
                return false;
            }
        );

        IDotCommand getOutputFormat = new DotCommmand(
            name: ".getOutputFormat",
            description: "Get the currently set output format",
            command: args =>
            {
                var table = new Table();
                table.AddColumns($"[bold blue]Format[/]", "[bold blue]Description[/]");
                table.AddRow(_outputFormats.GetCurrent().Name, _outputFormats.GetCurrent().Description);
                AnsiConsole.Write(table);
                return false;
            }
        );

        IDotCommand setOutputFormat = new DotCommmand(
            name: ".setOutputFormat",
            description: "Get the currently set output format",
            command: args =>
            {
                _outputFormats.SetCurrent(args[0]);
                return false;
            }
        );

        _commands = ImmutableDictionary.CreateRange(
            new KeyValuePair<string, IDotCommand>[] {
                KeyValuePair.Create(exit.Name,              exit             ),
                KeyValuePair.Create(listTables.Name,        listTables       ),
                KeyValuePair.Create(listColumns.Name,       listColumns      ),
                KeyValuePair.Create(listOutputFormats.Name, listOutputFormats),
                KeyValuePair.Create(getOutputFormat.Name,   getOutputFormat  ),
                KeyValuePair.Create(setOutputFormat.Name,   setOutputFormat  ),
            }
        );

    }

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        await _connection.OpenAsync(cancellationToken);
    }

    public IEnumerable<IDotCommand> List => _commands.Values;

    public IDotCommand this[string commandName]
    {
        get {
            if (!_commands.ContainsKey(commandName))
            {
                throw new ArgumentException($"Command Not Found: {commandName}");
            }
            return _commands[commandName];
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
