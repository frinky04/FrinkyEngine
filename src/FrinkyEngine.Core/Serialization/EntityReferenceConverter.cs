using System.Text.Json;
using System.Text.Json.Serialization;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Serialization;


/// <summary>
/// JSON converter that serializes <see cref="EntityReference"/> as a GUID string.
/// </summary>
public class EntityReferenceConverter : JsonConverter<EntityReference>
{
    public override EntityReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrEmpty(str) || !Guid.TryParse(str, out var guid))
            return EntityReference.None;
        return new EntityReference(guid);
    }

    public override void Write(Utf8JsonWriter writer, EntityReference value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.IsValid ? value.Id.ToString() : string.Empty);
    }
}
