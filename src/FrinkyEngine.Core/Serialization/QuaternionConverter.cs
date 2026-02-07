using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrinkyEngine.Core.Serialization;

/// <summary>
/// JSON converter that serializes <see cref="Quaternion"/> as <c>{ "x": ..., "y": ..., "z": ..., "w": ... }</c>.
/// </summary>
public class QuaternionConverter : JsonConverter<Quaternion>
{
    /// <inheritdoc />
    public override Quaternion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        float x = 0, y = 0, z = 0, w = 1;
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
                    case "w": w = reader.GetSingle(); break;
                }
            }
        }
        return new Quaternion(x, y, z, w);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteNumber("z", value.Z);
        writer.WriteNumber("w", value.W);
        writer.WriteEndObject();
    }
}
