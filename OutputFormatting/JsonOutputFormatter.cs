using System.Data;
using System.Text.Json;

namespace QueryTerminal.OutputFormatting;

public class JsonOutputFormatter : IOutputFormatter
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public JsonOutputFormatter(bool pretty = false)
    {
        _jsonSerializerOptions = new JsonSerializerOptions {
            WriteIndented = pretty,
            TypeInfoResolver = SourceGenerationContext.Default
        };
    }

    public void WriteOutput(IDataReader reader)
    {
        var values = new List<Dictionary<string,object>>();
        while (reader.Read())
        {
            object[] row = new object[reader.FieldCount];
            var count = reader.GetValues(row);
            values.Add(Enumerable.Range(0, count).Select(i => new KeyValuePair<string,object>(reader.GetName(i), row[i])).ToDictionary<string,object>());
        }
        var json = JsonSerializer.Serialize(values, typeof(List<Dictionary<string,object>>), _jsonSerializerOptions);
        Console.WriteLine(json);
    }
}
