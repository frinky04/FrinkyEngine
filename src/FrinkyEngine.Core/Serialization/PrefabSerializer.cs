using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Prefabs;
using FrinkyEngine.Core.Rendering;

namespace FrinkyEngine.Core.Serialization;

public static class PrefabSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new Vector3Converter(),
            new QuaternionConverter(),
            new ColorConverter(),
            new EntityReferenceConverter(),
            new PostProcessEffectListConverter(),
            new AssetReferenceConverter(),
            new JsonStringEnumConverter()
        }
    };

    public static void Save(PrefabAssetData prefab, string path)
    {
        var json = JsonSerializer.Serialize(prefab, JsonOptions);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(path, json);
    }

    public static PrefabAssetData? Load(string path)
    {
        if (!File.Exists(path))
            return null;

        var json = File.ReadAllText(path);
        var prefab = JsonSerializer.Deserialize<PrefabAssetData>(json, JsonOptions);
        if (prefab?.Root == null)
            return null;

        EnsureStableIds(prefab.Root, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        SanitizeRootNode(prefab.Root);
        return prefab;
    }

    public static PrefabAssetData CreateFromEntity(Entity root, bool preserveStableIds = true)
    {
        var usedStableIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var entityIdToStableId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var data = new PrefabAssetData
        {
            Name = root.Name,
            Root = SerializeNode(root, preserveStableIds, isSerializationRoot: true, usedStableIds, entityIdToStableId)
        };
        NormalizeInternalEntityReferences(data.Root, entityIdToStableId);
        return data;
    }

    public static PrefabNodeData SerializeNode(Entity entity, bool preserveStableIds = true)
    {
        var usedStableIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return SerializeNode(entity, preserveStableIds, isSerializationRoot: true, usedStableIds);
    }

    public static PrefabNodeData SerializeNodeNormalized(Entity entity, bool preserveStableIds = true)
    {
        var usedStableIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var entityIdToStableId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var node = SerializeNode(entity, preserveStableIds, isSerializationRoot: true, usedStableIds, entityIdToStableId);
        NormalizeInternalEntityReferences(node, entityIdToStableId);
        return node;
    }

    private static PrefabNodeData SerializeNode(
        Entity entity,
        bool preserveStableIds,
        bool isSerializationRoot,
        HashSet<string> usedStableIds,
        Dictionary<string, string>? entityIdToStableId = null)
    {
        var stableId = ResolveStableId(entity, preserveStableIds);
        if (string.IsNullOrWhiteSpace(stableId) || usedStableIds.Contains(stableId))
            stableId = Guid.NewGuid().ToString("N");
        usedStableIds.Add(stableId);

        entityIdToStableId?.TryAdd(entity.Id.ToString("N"), stableId);

        var node = new PrefabNodeData
        {
            StableId = stableId,
            Name = entity.Name,
            Active = entity.Active
        };

        foreach (var component in entity.Components)
            node.Components.Add(SerializeComponent(component));

        foreach (var unresolved in entity.UnresolvedComponents)
        {
            node.Components.Add(new PrefabComponentData
            {
                Type = unresolved.Type,
                Enabled = unresolved.Enabled,
                EditorOnly = unresolved.EditorOnly,
                Properties = new Dictionary<string, JsonElement>(unresolved.Properties)
            });
        }

        foreach (var child in entity.Transform.Children)
            node.Children.Add(SerializeNode(child.Entity, preserveStableIds, isSerializationRoot: false, usedStableIds, entityIdToStableId));

        // The root transform is scene placement, not prefab content.
        if (isSerializationRoot)
            SanitizeRootNode(node);

        return node;
    }

    private static void NormalizeInternalEntityReferences(PrefabNodeData node, Dictionary<string, string> entityIdToStableId)
    {
        foreach (var component in node.Components)
        {
            var keysToRemap = new List<string>();
            foreach (var (propName, jsonElement) in component.Properties)
            {
                if (jsonElement.ValueKind == JsonValueKind.String)
                {
                    var str = jsonElement.GetString();
                    if (str != null && Guid.TryParse(str, out var guid))
                    {
                        var key = guid.ToString("N");
                        if (entityIdToStableId.TryGetValue(key, out var stableId) &&
                            !string.Equals(key, stableId, StringComparison.OrdinalIgnoreCase))
                        {
                            keysToRemap.Add(propName);
                        }
                    }
                }
            }

            foreach (var key in keysToRemap)
            {
                var oldGuid = Guid.Parse(component.Properties[key].GetString()!);
                var stableId = entityIdToStableId[oldGuid.ToString("N")];
                var stableGuid = Guid.Parse(stableId);
                component.Properties[key] = JsonSerializer.SerializeToElement(stableGuid.ToString(), JsonOptions);
            }
        }

        foreach (var child in node.Children)
            NormalizeInternalEntityReferences(child, entityIdToStableId);
    }

    public static PrefabComponentData SerializeComponent(Component component)
    {
        var data = new PrefabComponentData
        {
            Type = ComponentTypeResolver.GetTypeName(component.GetType()),
            Enabled = component.Enabled,
            EditorOnly = component.EditorOnly
        };

        var type = component.GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;
            if (prop.Name is "Entity" or "HasStarted" or "Enabled" or "EditorOnly") continue;
            if (prop.Name is "EulerAngles" or "WorldPosition" or "WorldRotation") continue;

            try
            {
                var value = prop.GetValue(component);
                if (value != null)
                {
                    var jsonElement = JsonSerializer.SerializeToElement(value, prop.PropertyType, JsonOptions);
                    data.Properties[prop.Name] = jsonElement;
                }
            }
            catch
            {
                // Skip properties that cannot be serialized.
            }
        }

        return data;
    }

    public static bool ApplyComponentData(Entity entity, PrefabComponentData data)
    {
        var type = ComponentTypeResolver.Resolve(data.Type);
        if (type == null)
        {
            entity.UnresolvedComponents.Add(new ComponentData
            {
                Type = data.Type,
                Enabled = data.Enabled,
                EditorOnly = data.EditorOnly,
                Properties = new Dictionary<string, JsonElement>(data.Properties)
            });
            FrinkyLog.Warning($"Unresolved component type '{data.Type}' on entity '{entity.Name}' — data preserved");
            return false;
        }

        Component component;
        if (type == typeof(Components.TransformComponent))
        {
            component = entity.Transform;
        }
        else
        {
            var existing = entity.GetComponent(type);
            component = existing ?? entity.AddComponent(type);
        }

        component.Enabled = data.Enabled;
        component.EditorOnly = data.EditorOnly;

        foreach (var (propName, jsonElement) in data.Properties)
        {
            var prop = type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null || !prop.CanWrite) continue;
            if (prop.Name is "Entity" or "HasStarted" or "Enabled" or "EditorOnly" or "EulerAngles" or "WorldPosition" or "WorldRotation") continue;

            try
            {
                var value = JsonSerializer.Deserialize(jsonElement.GetRawText(), prop.PropertyType, JsonOptions);
                prop.SetValue(component, value);
            }
            catch
            {
                // Skip values that cannot be deserialized into this component.
            }
        }

        return true;
    }

    public static object? DeserializeValue(JsonElement value, Type targetType)
    {
        try
        {
            return JsonSerializer.Deserialize(value.GetRawText(), targetType, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public static JsonElement SerializeValue(object value, Type valueType)
    {
        return JsonSerializer.SerializeToElement(value, valueType, JsonOptions);
    }

    private static string ResolveStableId(Entity entity, bool preserveStableIds)
    {
        if (preserveStableIds && entity.Prefab != null && !string.IsNullOrWhiteSpace(entity.Prefab.SourceNodeId))
            return entity.Prefab.SourceNodeId;

        if (preserveStableIds)
            return entity.Id.ToString("N");

        return Guid.NewGuid().ToString("N");
    }

    private static void SanitizeRootNode(PrefabNodeData root)
    {
        foreach (var component in root.Components)
        {
            if (!IsTransformComponentType(component.Type))
                continue;

            component.Properties.Remove("LocalPosition");
            component.Properties.Remove("LocalRotation");
            component.Properties.Remove("LocalScale");
            break;
        }
    }

    private static bool IsTransformComponentType(string componentType)
    {
        if (string.Equals(componentType, typeof(Components.TransformComponent).FullName, StringComparison.Ordinal) ||
            string.Equals(componentType, nameof(Components.TransformComponent), StringComparison.Ordinal))
        {
            return true;
        }

        var resolved = ComponentTypeResolver.Resolve(componentType);
        return resolved == typeof(Components.TransformComponent);
    }

    private static void EnsureStableIds(PrefabNodeData node, HashSet<string> usedStableIds)
    {
        if (string.IsNullOrWhiteSpace(node.StableId) || usedStableIds.Contains(node.StableId))
            node.StableId = Guid.NewGuid().ToString("N");
        usedStableIds.Add(node.StableId);

        foreach (var child in node.Children)
            EnsureStableIds(child, usedStableIds);
    }
}
