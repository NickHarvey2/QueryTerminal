using System.Data;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace QueryTerminal.OutputFormatting;

public class YamlOutputFormatter : IOutputFormatter
{
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

