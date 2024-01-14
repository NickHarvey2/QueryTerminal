using System.Data;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Json;

namespace QueryTerminal.OutputFormatting;

public class JsonOutputFormatter : IOutputFormatter
{
    private readonly bool _pretty;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly string _name;
    private readonly string _description;

    public JsonOutputFormatter(string name, string description, bool pretty = false)
    {
        _pretty = pretty;
        _jsonSerializerOptions = new JsonSerializerOptions {
            WriteIndented = pretty,
            TypeInfoResolver = SourceGenerationContext.Default
        };
        _jsonSerializerOptions.Converters.Add(new DBNullJsonConverter());
        _name = name;
        _description = description;
    }

    public string Name => _name;

    public string Description => _description;

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
