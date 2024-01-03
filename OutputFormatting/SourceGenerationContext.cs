using System.Text.Json.Serialization;

[JsonSerializable(typeof(List<Dictionary<string,object>>))]
[JsonSerializable(typeof(Dictionary<string,object>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}
