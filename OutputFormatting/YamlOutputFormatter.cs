using System.Data;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace QueryTerminal.OutputFormatting;

public class YamlOutputFormatter : IOutputFormatter
{
    private readonly string _name;
    private readonly string _description;

    public YamlOutputFormatter(string name, string description)
    {
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
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = serializer.Serialize(values);
        Console.WriteLine(yaml);
    }
}

