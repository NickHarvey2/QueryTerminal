using System.Text.Json.Serialization;

namespace QueryTerminal.OutputFormatting;

[JsonSerializable(typeof(List<Dictionary<string,object>>))]
[JsonSerializable(typeof(Dictionary<string,object>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(System.Int64))]
[JsonSerializable(typeof(System.DBNull))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}
