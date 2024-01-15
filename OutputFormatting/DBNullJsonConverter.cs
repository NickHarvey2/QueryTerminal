using System.Text.Json;
using System.Text.Json.Serialization;

namespace QueryTerminal.OutputFormatting;

internal class DBNullJsonConverter : JsonConverter<DBNull>
{
    public override DBNull Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("DBNull values are not expected in input JSON data");
    }

    public override void Write(Utf8JsonWriter writer, DBNull value, JsonSerializerOptions options)
    {
        writer.WriteNullValue();
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(DBNull);
    }
}

