using System.Data;
using Spectre.Console;

namespace QueryTerminal.OutputFormatting;

public class TableOutputFormatter : IOutputFormatter
{
    private readonly string _border;
    private readonly string _name;
    private readonly string _description;

    public TableOutputFormatter(string name, string description, string border = "square")
    {
        _border = border;
        _name = name;
        _description = description;
    }

    public string Name => _name;

    public string Description => _description;

    public void WriteOutput(IDataReader reader)
    {
        var table = new Table();
        if (_border == "square")
        {
            table.AddColumns(Enumerable.Range(0, reader.FieldCount).Select(i => new TableColumn($"[bold blue]{reader.GetName(i)}[/]")).ToArray());
        }
        else
        {
            table.AddColumns(Enumerable.Range(0, reader.FieldCount).Select(i => new TableColumn(reader.GetName(i))).ToArray());
        }

        while (reader.Read())
        {
            object[] row = new object[reader.FieldCount];
            var count = reader.GetValues(row);
            table.AddRow(row.Select(c => c.ToString() ?? "").ToArray());
        }
        switch (_border)
        {
            case "none":
                table.NoBorder();
                break;
            case "markdown":
                table.MarkdownBorder();
                break;
            case "simple":
                table.SimpleBorder();
                break;
            case "square":
                table.SquareBorder();
                break;
            default:
                table.SquareBorder();
                break;
        }
        AnsiConsole.Write(table);
    }
}


