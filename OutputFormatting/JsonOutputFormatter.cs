using System.Data;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Json;

namespace QueryTerminal.OutputFormatting;

public class JsonOutputFormatter : IOutputFormatter
{
    private readonly bool _pretty;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public JsonOutputFormatter(bool pretty = false)
    {
        _pretty = pretty;
        _jsonSerializerOptions = new JsonSerializerOptions {
            WriteIndented = pretty,
            TypeInfoResolver = SourceGenerationContext.Default
        };
        _jsonSerializerOptions.Converters.Add(new DBNullJsonConverter());
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
        if (_pretty)
        {
            AnsiConsole.Write(new JsonText(json));
            AnsiConsole.WriteLine();
        }
        else
        {
            Console.WriteLine(json);
        }
    }
}
