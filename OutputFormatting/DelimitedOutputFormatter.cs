using System.Data;

namespace QueryTerminal.OutputFormatting;

public class DelimitedOutputFormatter : IOutputFormatter
{
    private readonly bool _includeHeaders;
    private readonly char _delimiter;
    private readonly string _name;
    private readonly string _description;

    public DelimitedOutputFormatter(string name, string description, bool includeHeaders, char delimiter)
    {
        _includeHeaders = includeHeaders;
        _delimiter = delimiter;
        _name = name;
        _description = description;
    }

    public string Name => _name;

    public string Description => _description;

    public void WriteOutput(IDataReader reader)
    {
        if (_includeHeaders)
        {
            var columnNames = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
            Console.WriteLine(string.Join(_delimiter, columnNames));
        }
        while (reader.Read())
        {
            object[] row = new object[reader.FieldCount];
            var count = reader.GetValues(row);
            Console.WriteLine(string.Join(_delimiter, row.Select(field => field.ToString())));
        }
    }
}
