using System.Data;

namespace QueryTerminal.OutputFormatting;

public class DelimitedOutputFormatter : IOutputFormatter
{
    private readonly bool _includeHeaders;
    private readonly char _delimiter;
    private readonly string _name;
    private readonly string _description;
    private readonly IRenderer _renderer;

    public DelimitedOutputFormatter(IRenderer renderer, string name, string description, bool includeHeaders, char delimiter)
    {
        _includeHeaders = includeHeaders;
        _delimiter = delimiter;
        _name = name;
        _description = description;
        _renderer = renderer;
    }

    public string Name => _name;

    public string Description => _description;

    public void WriteOutput(IDataReader reader)
    {
        if (_includeHeaders)
        {
            var columnNames = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
            _renderer.Render(string.Join(_delimiter, columnNames));
        }
        while (reader.Read())
        {
            object[] row = new object[reader.FieldCount];
            var count = reader.GetValues(row);
            _renderer.Render(string.Join(_delimiter, row.Select(field => field.ToString())));
        }
    }
}
