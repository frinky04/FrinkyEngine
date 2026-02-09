using System.Text.Json;
using System.Text.Json.Serialization;
using FrinkyEngine.Core.Assets;

namespace FrinkyEngine.Core.Serialization;

/// <summary>
/// Serializes <see cref="AssetReference"/> as a plain JSON string (just the path).
/// Provides full backward compatibility with existing string-based asset paths.
/// </summary>
public class AssetReferenceConverter : JsonConverter<AssetReference>
{
    public override AssetReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new AssetReference(reader.GetString() ?? "");
    }

    public override void Write(Utf8JsonWriter writer, AssetReference value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Path ?? "");
    }
}
