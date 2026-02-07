using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrinkyEngine.Core.Serialization;

/// <summary>
/// JSON converter that serializes <see cref="Vector3"/> as <c>{ "x": ..., "y": ..., "z": ... }</c>.
/// </summary>
public class Vector3Converter : JsonConverter<Vector3>
{
    /// <inheritdoc />
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        float x = 0, y = 0, z = 0;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var prop = reader.GetString();
                reader.Read();
                switch (prop)
                {
                    case "x": x = reader.GetSingle(); break;
                    case "y": y = reader.GetSingle(); break;
                    case "z": z = reader.GetSingle(); break;
                }
            }
        }
        return new Vector3(x, y, z);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteNumber("z", value.Z);
        writer.WriteEndObject();
    }
}
