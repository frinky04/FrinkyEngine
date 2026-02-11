#pragma warning disable CS1591

using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Serialization;

/// <summary>
/// JSON converter factory that handles serialization of <see cref="FObject"/> subclasses
/// and <see cref="List{T}"/> where T derives from <see cref="FObject"/>.
/// </summary>
public class FObjectConverterFactory : JsonConverterFactory
{
    private static readonly HashSet<string> ExcludedProperties = new()
    {
        nameof(FObject.DisplayName)
    };

    public override bool CanConvert(Type typeToConvert)
    {
        if (typeof(FObject).IsAssignableFrom(typeToConvert))
            return true;

        if (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(List<>))
        {
            var elementType = typeToConvert.GetGenericArguments()[0];
            if (typeof(FObject).IsAssignableFrom(elementType))
                return true;
        }

        return false;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(List<>))
        {
            var elementType = typeToConvert.GetGenericArguments()[0];
            var converterType = typeof(FObjectListConverter<>).MakeGenericType(elementType);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }

        var singleConverterType = typeof(FObjectConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(singleConverterType)!;
    }

    private class FObjectConverter<T> : JsonConverter<T> where T : FObject
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object for FObject.");

            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            return (T?)ReadFObject(root, typeToConvert, options);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            WriteFObject(writer, value, options);
        }
    }

    private class FObjectListConverter<T> : JsonConverter<List<T>> where T : FObject
    {
        public override List<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return new List<T>();

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected start of array for FObject list.");

            var list = new List<T>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType == JsonTokenType.Null)
                    continue;

                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Expected start of object for FObject.");

                using var doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;

                var obj = ReadFObject(root, typeof(T), options);
                if (obj is T typed)
                    list.Add(typed);
            }

            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var item in value)
            {
                if (item == null)
                {
                    writer.WriteNullValue();
                    continue;
                }

                WriteFObject(writer, item, options);
            }

            writer.WriteEndArray();
        }
    }

    internal static FObject? ReadFObject(JsonElement root, Type declaredType, JsonSerializerOptions options)
    {
        if (!root.TryGetProperty("$type", out var typeElement))
            return null;

        var typeName = typeElement.GetString();
        if (string.IsNullOrEmpty(typeName))
            return null;

        var objectType = FObjectTypeResolver.Resolve(typeName);
        if (objectType == null || !declaredType.IsAssignableFrom(objectType))
            return null;

        FObject? obj;
        try
        {
            obj = (FObject?)Activator.CreateInstance(objectType);
        }
        catch
        {
            return null;
        }

        if (obj == null)
            return null;

        if (root.TryGetProperty("properties", out var propsElement) && propsElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var jsonProp in propsElement.EnumerateObject())
            {
                var prop = objectType.GetProperty(jsonProp.Name, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null || !prop.CanWrite) continue;
                if (ExcludedProperties.Contains(prop.Name)) continue;

                try
                {
                    var value = JsonSerializer.Deserialize(jsonProp.Value.GetRawText(), prop.PropertyType, options);
                    prop.SetValue(obj, value);
                }
                catch
                {
                    // Skip properties that can't be deserialized
                }
            }
        }

        return obj;
    }

    internal static void WriteFObject(Utf8JsonWriter writer, FObject value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var objectType = value.GetType();
        writer.WriteString("$type", FObjectTypeResolver.GetTypeName(objectType));

        writer.WritePropertyName("properties");
        writer.WriteStartObject();

        foreach (var prop in objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;
            if (ExcludedProperties.Contains(prop.Name)) continue;

            try
            {
                var propValue = prop.GetValue(value);
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
        writer.WriteEndObject(); // object
    }
}
