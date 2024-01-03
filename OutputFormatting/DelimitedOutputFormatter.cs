using System.Data;

namespace QueryTerminal.OutputFormatting;

public class DelimitedOutputFormatter(bool includeHeaders, char delimiter) : IOutputFormatter
{
    public void WriteOutput(IDataReader reader)
    {
        if (includeHeaders)
        {
            var columnNames = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
            Console.WriteLine(string.Join(delimiter, columnNames));
        }
        while (reader.Read())
        {
            object[] row = new object[reader.FieldCount];
            var count = reader.GetValues(row);
            Console.WriteLine(string.Join(delimiter, row.Select(field => field.ToString())));
        }
    }
}
