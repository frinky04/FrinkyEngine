using System.Text.Json;
using System.Text.Json.Serialization;
using Raylib_cs;

namespace FrinkyEngine.Core.Serialization;

public class ColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        byte r = 255, g = 255, b = 255, a = 255;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var prop = reader.GetString();
                reader.Read();
                switch (prop)
                {
                    case "r": r = reader.GetByte(); break;
                    case "g": g = reader.GetByte(); break;
                    case "b": b = reader.GetByte(); break;
                    case "a": a = reader.GetByte(); break;
                }
            }
        }
        return new Color(r, g, b, a);
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("r", value.R);
        writer.WriteNumber("g", value.G);
        writer.WriteNumber("b", value.B);
        writer.WriteNumber("a", value.A);
        writer.WriteEndObject();
    }
}
