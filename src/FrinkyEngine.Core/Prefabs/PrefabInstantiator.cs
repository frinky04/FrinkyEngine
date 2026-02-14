#pragma warning disable CS1591
using System.Numerics;
using System.Text.Json;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Serialization;

namespace FrinkyEngine.Core.Prefabs;

/// <summary>
/// Provides static methods to instantiate prefabs into a scene at runtime.
/// This logic is shared by both the editor and runtime.
/// </summary>
public static class PrefabInstantiator
{
    public static Entity? Instantiate(Scene.Scene scene, string prefabPath, TransformComponent? parent = null)
    {
        return InstantiatePrefabInternal(prefabPath, scene, parent, new PrefabOverridesData(), forcedRootId: null);
    }

    public static Entity? Instantiate(Scene.Scene scene, string prefabPath, Vector3 position, Quaternion rotation, TransformComponent? parent = null)
    {
        var entity = InstantiatePrefabInternal(prefabPath, scene, parent, new PrefabOverridesData(), forcedRootId: null);
        if (entity != null)
        {
            entity.Transform.WorldPosition = position;
            entity.Transform.WorldRotation = rotation;
        }
        return entity;
    }

    public static Entity? Instantiate(Scene.Scene scene, AssetReference prefab, TransformComponent? parent = null)
    {
        if (prefab.IsEmpty) return null;
        return Instantiate(scene, prefab.Path, parent);
    }

    public static Entity? Instantiate(Scene.Scene scene, AssetReference prefab, Vector3 position, Quaternion rotation, TransformComponent? parent = null)
    {
        if (prefab.IsEmpty) return null;
        return Instantiate(scene, prefab.Path, position, rotation, parent);
    }

    public static Entity? InstantiatePrefabInternal(
        string assetPath,
        Scene.Scene scene,
        TransformComponent? parent,
        PrefabOverridesData? overrides,
        Guid? forcedRootId)
    {
        var prefab = PrefabDatabase.Instance.Load(assetPath, resolveVariants: true);
        if (prefab == null)
            return null;

        var rootNode = prefab.Root.Clone();
        PrefabOverrideUtility.ApplyOverrides(rootNode, overrides);

        var stableIdMapping = BuildStableIdMapping(rootNode, forcedRootId);
        RemapPrefabEntityReferences(rootNode, stableIdMapping);

        return InstantiateNodeRecursive(
            rootNode,
            scene,
            parent,
            assetPath,
            isRoot: true,
            rootOverrides: overrides?.Clone() ?? new PrefabOverridesData(),
            stableIdMapping);
    }

    internal static Entity InstantiateNodeRecursive(
        PrefabNodeData node,
        Scene.Scene scene,
        TransformComponent? parent,
        string assetPath,
        bool isRoot,
        PrefabOverridesData? rootOverrides,
        Dictionary<string, Guid>? stableIdMapping)
    {
        var entity = new Entity(node.Name)
        {
            Active = node.Active,
            Prefab = new PrefabInstanceMetadata
            {
                IsRoot = isRoot,
                AssetPath = assetPath,
                SourceNodeId = node.StableId,
                Overrides = isRoot ? rootOverrides : null
            }
        };

        if (stableIdMapping != null && stableIdMapping.TryGetValue(node.StableId, out var mappedId))
            entity.Id = mappedId;

        foreach (var component in node.Components)
            PrefabSerializer.ApplyComponentData(entity, component);

        scene.AddEntity(entity);
        if (parent != null)
            entity.Transform.SetParent(parent);

        foreach (var child in node.Children)
            InstantiateNodeRecursive(child, scene, entity.Transform, assetPath, false, null, stableIdMapping);

        return entity;
    }

    internal static Dictionary<string, Guid> BuildStableIdMapping(PrefabNodeData root, Guid? forcedRootId)
    {
        var mapping = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        BuildStableIdMappingRecursive(root, mapping, forcedRootId);
        return mapping;
    }

    private static void BuildStableIdMappingRecursive(PrefabNodeData node, Dictionary<string, Guid> mapping, Guid? forcedId)
    {
        mapping[node.StableId] = forcedId ?? Guid.NewGuid();
        foreach (var child in node.Children)
            BuildStableIdMappingRecursive(child, mapping, null);
    }

    internal static void RemapPrefabEntityReferences(PrefabNodeData node, Dictionary<string, Guid> mapping)
    {
        foreach (var component in node.Components)
        {
            var keysToUpdate = new List<(string key, JsonElement remapped)>();
            foreach (var (propName, jsonElement) in component.Properties)
            {
                var remapped = RemapPrefabJsonElement(jsonElement, mapping);
                if (remapped.HasValue)
                    keysToUpdate.Add((propName, remapped.Value));
            }

            foreach (var (key, remapped) in keysToUpdate)
                component.Properties[key] = remapped;
        }

        foreach (var child in node.Children)
            RemapPrefabEntityReferences(child, mapping);
    }

    private static JsonElement? RemapPrefabJsonElement(JsonElement element, Dictionary<string, Guid> mapping)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
            {
                var str = element.GetString();
                if (str != null && Guid.TryParse(str, out var guid))
                {
                    var normalizedKey = guid.ToString("N");
                    if (mapping.TryGetValue(normalizedKey, out var newGuid))
                        return JsonSerializer.SerializeToElement(newGuid.ToString());
                }
                return null;
            }
            case JsonValueKind.Object:
            {
                if (element.TryGetProperty("$type", out _) && element.TryGetProperty("properties", out _))
                {
                    bool changed = false;
                    using var doc = JsonDocument.Parse(element.GetRawText());
                    using var ms = new MemoryStream();
                    using (var writer = new Utf8JsonWriter(ms))
                    {
                        writer.WriteStartObject();
                        foreach (var prop in doc.RootElement.EnumerateObject())
                        {
                            if (prop.Name == "properties")
                            {
                                writer.WritePropertyName("properties");
                                writer.WriteStartObject();
                                foreach (var innerProp in prop.Value.EnumerateObject())
                                {
                                    var remapped = RemapPrefabJsonElement(innerProp.Value, mapping);
                                    writer.WritePropertyName(innerProp.Name);
                                    if (remapped.HasValue)
                                    {
                                        remapped.Value.WriteTo(writer);
                                        changed = true;
                                    }
                                    else
                                    {
                                        innerProp.Value.WriteTo(writer);
                                    }
                                }
                                writer.WriteEndObject();
                            }
                            else
                            {
                                prop.WriteTo(writer);
                            }
                        }
                        writer.WriteEndObject();
                    }

                    if (changed)
                    {
                        var newDoc = JsonDocument.Parse(ms.ToArray());
                        return newDoc.RootElement.Clone();
                    }
                }
                return null;
            }
            case JsonValueKind.Array:
            {
                bool changed = false;
                var elements = new List<(JsonElement original, JsonElement? remapped)>();
                foreach (var item in element.EnumerateArray())
                {
                    var remapped = RemapPrefabJsonElement(item, mapping);
                    elements.Add((item, remapped));
                    if (remapped.HasValue)
                        changed = true;
                }

                if (changed)
                {
                    using var ms = new MemoryStream();
                    using (var writer = new Utf8JsonWriter(ms))
                    {
                        writer.WriteStartArray();
                        foreach (var (original, remapped) in elements)
                            (remapped ?? original).WriteTo(writer);
                        writer.WriteEndArray();
                    }
                    var newDoc = JsonDocument.Parse(ms.ToArray());
                    return newDoc.RootElement.Clone();
                }
                return null;
            }
            default:
                return null;
        }
    }
}
