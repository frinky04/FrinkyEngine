using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using FrinkyEngine.Core.Rendering.PostProcessing;

namespace FrinkyEngine.Core.Serialization;

/// <summary>
/// JSON converter for <see cref="List{PostProcessEffect}"/> that serializes each effect
/// with a <c>$type</c> discriminator and its public read/write properties.
/// </summary>
public class PostProcessEffectListConverter : JsonConverter<List<PostProcessEffect>>
{
    private static readonly HashSet<string> ExcludedProperties = new()
    {
        nameof(PostProcessEffect.DisplayName),
        nameof(PostProcessEffect.IsInitialized),
        nameof(PostProcessEffect.NeedsDepth)
    };

    /// <inheritdoc/>
    public override List<PostProcessEffect>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return new List<PostProcessEffect>();

        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array for PostProcessEffect list.");

        var effects = new List<PostProcessEffect>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object for PostProcessEffect.");

            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            if (!root.TryGetProperty("$type", out var typeElement))
                continue;

            var typeName = typeElement.GetString();
            if (string.IsNullOrEmpty(typeName))
                continue;

            var effectType = PostProcessEffectResolver.Resolve(typeName);
            if (effectType == null)
                continue;

            PostProcessEffect? effect;
            try
            {
                effect = (PostProcessEffect?)Activator.CreateInstance(effectType);
            }
            catch
            {
                continue;
            }

            if (effect == null)
                continue;

            // Read Enabled
            if (root.TryGetProperty("enabled", out var enabledElement))
                effect.Enabled = enabledElement.GetBoolean();

            // Read properties via reflection
            if (root.TryGetProperty("properties", out var propsElement) && propsElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var jsonProp in propsElement.EnumerateObject())
                {
                    var prop = effectType.GetProperty(jsonProp.Name, BindingFlags.Public | BindingFlags.Instance);
                    if (prop == null || !prop.CanWrite) continue;
                    if (ExcludedProperties.Contains(prop.Name)) continue;
                    if (prop.Name == nameof(PostProcessEffect.Enabled)) continue;

                    try
                    {
                        var value = JsonSerializer.Deserialize(jsonProp.Value.GetRawText(), prop.PropertyType, options);
                        prop.SetValue(effect, value);
                    }
                    catch
                    {
                        // Skip properties that can't be deserialized
                    }
                }
            }

            effects.Add(effect);
        }

        return effects;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, List<PostProcessEffect> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var effect in value)
        {
            writer.WriteStartObject();

            var effectType = effect.GetType();
            writer.WriteString("$type", PostProcessEffectResolver.GetTypeName(effectType));
            writer.WriteBoolean("enabled", effect.Enabled);

            writer.WritePropertyName("properties");
            writer.WriteStartObject();

            foreach (var prop in effectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead || !prop.CanWrite) continue;
                if (ExcludedProperties.Contains(prop.Name)) continue;
                if (prop.Name == nameof(PostProcessEffect.Enabled)) continue;

                try
                {
                    var propValue = prop.GetValue(effect);
                    if (propValue != null)
                    {
                        writer.WritePropertyName(prop.Name);
                        JsonSerializer.Serialize(writer, propValue, prop.PropertyType, options);
                    }
                }
                catch
                {
                    // Skip properties that can't be serialized
                }
            }

            writer.WriteEndObject(); // properties
            writer.WriteEndObject(); // effect
        }

        writer.WriteEndArray();
    }
}
