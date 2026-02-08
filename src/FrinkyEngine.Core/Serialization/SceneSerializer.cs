using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Physics;
using FrinkyEngine.Core.Prefabs;
using Raylib_cs;

namespace FrinkyEngine.Core.Serialization;

/// <summary>
/// Handles saving and loading scenes in the <c>.fscene</c> JSON format.
/// Also provides entity duplication via serialization round-trips.
/// </summary>
public static class SceneSerializer
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
            new JsonStringEnumConverter()
        }
    };

    /// <summary>
    /// Saves a scene to a <c>.fscene</c> file.
    /// </summary>
    /// <param name="scene">The scene to save.</param>
    /// <param name="path">Destination file path.</param>
    public static void Save(Scene.Scene scene, string path)
    {
        var data = SerializeScene(scene);
        var json = JsonSerializer.Serialize(data, JsonOptions);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Loads a scene from a <c>.fscene</c> file.
    /// </summary>
    /// <param name="path">Path to the scene file.</param>
    /// <returns>The loaded scene, or <c>null</c> if the file doesn't exist or is invalid.</returns>
    public static Scene.Scene? Load(string path)
    {
        if (!File.Exists(path)) return null;
        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<SceneData>(json, JsonOptions);
        if (data == null) return null;
        return DeserializeScene(data);
    }

    /// <summary>
    /// Serializes a scene to a JSON string (useful for snapshots and clipboard operations).
    /// </summary>
    /// <param name="scene">The scene to serialize.</param>
    /// <returns>The JSON string.</returns>
    public static string SerializeToString(Scene.Scene scene)
    {
        var data = SerializeScene(scene);
        return JsonSerializer.Serialize(data, JsonOptions);
    }

    /// <summary>
    /// Deserializes a scene from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>The deserialized scene, or <c>null</c> if the JSON is invalid.</returns>
    public static Scene.Scene? DeserializeFromString(string json)
    {
        var data = JsonSerializer.Deserialize<SceneData>(json, JsonOptions);
        if (data == null) return null;
        return DeserializeScene(data);
    }

    private static SceneData SerializeScene(Scene.Scene scene)
    {
        var data = new SceneData
        {
            Name = scene.Name,
            EditorCameraPosition = scene.EditorCameraPosition,
            EditorCameraYaw = scene.EditorCameraYaw,
            EditorCameraPitch = scene.EditorCameraPitch,
            Physics = scene.PhysicsSettings.Clone()
        };
        foreach (var entity in scene.Entities)
        {
            if (entity.Transform.Parent != null) continue;
            data.Entities.Add(SerializeEntity(entity));
        }
        return data;
    }

    private static EntityData SerializeEntity(Entity entity)
    {
        var data = new EntityData
        {
            Name = entity.Name,
            Id = entity.Id,
            Active = entity.Active,
            Prefab = entity.Prefab?.Clone()
        };

        foreach (var component in entity.Components)
        {
            data.Components.Add(SerializeComponent(component));
        }

        foreach (var child in entity.Transform.Children)
        {
            data.Children.Add(SerializeEntity(child.Entity));
        }

        return data;
    }

    private static ComponentData SerializeComponent(Component component)
    {
        var data = new ComponentData
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
                // Skip properties that can't be serialized
            }
        }

        return data;
    }

    private static Scene.Scene DeserializeScene(SceneData data)
    {
        var scene = new Scene.Scene
        {
            Name = data.Name,
            EditorCameraPosition = data.EditorCameraPosition,
            EditorCameraYaw = data.EditorCameraYaw,
            EditorCameraPitch = data.EditorCameraPitch,
            PhysicsSettings = data.Physics?.Clone() ?? new PhysicsSettings()
        };
        scene.PhysicsSettings.Normalize();

        foreach (var entityData in data.Entities)
        {
            DeserializeEntityTree(entityData, scene, null);
        }

        return scene;
    }

    private static Entity DeserializeEntityTree(EntityData data, Scene.Scene scene, TransformComponent? parent)
    {
        var entity = new Entity(data.Name)
        {
            Id = data.Id,
            Active = data.Active,
            Prefab = data.Prefab?.Clone()
        };

        foreach (var componentData in data.Components)
        {
            DeserializeComponent(entity, componentData);
        }

        scene.AddEntity(entity);

        if (parent != null)
            entity.Transform.SetParent(parent);

        foreach (var childData in data.Children)
        {
            DeserializeEntityTree(childData, scene, entity.Transform);
        }

        return entity;
    }

    /// <summary>
    /// Creates a deep copy of an entity (and its children) and adds it to the scene.
    /// </summary>
    /// <param name="source">The entity to duplicate.</param>
    /// <param name="scene">The scene to add the duplicate to.</param>
    /// <returns>The duplicated entity, or <c>null</c> if duplication failed.</returns>
    public static Entity? DuplicateEntity(Entity source, Scene.Scene scene)
    {
        var data = SerializeEntity(source);
        var oldToNew = AssignNewIds(data);
        RemapEntityReferences(data, oldToNew);
        data.Name = GenerateDuplicateName(data.Name);

        // Find the parent of the source entity
        var parent = source.Transform.Parent;

        return DeserializeEntityTree(data, scene, parent);
    }

    private static Dictionary<Guid, Guid> AssignNewIds(EntityData data)
    {
        var mapping = new Dictionary<Guid, Guid>();
        AssignNewIdsRecursive(data, mapping);
        return mapping;
    }

    private static void AssignNewIdsRecursive(EntityData data, Dictionary<Guid, Guid> mapping)
    {
        var oldId = data.Id;
        var newId = Guid.NewGuid();
        mapping[oldId] = newId;
        data.Id = newId;
        foreach (var child in data.Children)
            AssignNewIdsRecursive(child, mapping);
    }

    private static void RemapEntityReferences(EntityData data, Dictionary<Guid, Guid> oldToNew)
    {
        foreach (var component in data.Components)
        {
            var keysToRemap = new List<string>();
            foreach (var (propName, jsonElement) in component.Properties)
            {
                if (jsonElement.ValueKind == JsonValueKind.String)
                {
                    var str = jsonElement.GetString();
                    if (str != null && Guid.TryParse(str, out var guid) && oldToNew.ContainsKey(guid))
                        keysToRemap.Add(propName);
                }
            }

            foreach (var key in keysToRemap)
            {
                var oldGuid = Guid.Parse(component.Properties[key].GetString()!);
                var newGuid = oldToNew[oldGuid];
                component.Properties[key] = JsonSerializer.SerializeToElement(newGuid.ToString(), JsonOptions);
            }
        }

        foreach (var child in data.Children)
            RemapEntityReferences(child, oldToNew);
    }

    /// <summary>
    /// Generates a duplicate name by appending or incrementing a " (N)" suffix.
    /// </summary>
    /// <param name="name">The original entity name.</param>
    /// <returns>The new name with an incremented suffix.</returns>
    public static string GenerateDuplicateName(string name)
    {
        // Check if name ends with " (N)" pattern
        var match = System.Text.RegularExpressions.Regex.Match(name, @"^(.*) \((\d+)\)$");
        if (match.Success)
        {
            var baseName = match.Groups[1].Value;
            var number = int.Parse(match.Groups[2].Value);
            return $"{baseName} ({number + 1})";
        }
        return $"{name} (1)";
    }

    private static void DeserializeComponent(Entity entity, ComponentData data)
    {
        var type = ComponentTypeResolver.Resolve(data.Type);
        if (type == null) return;

        Component component;
        if (type == typeof(TransformComponent))
        {
            component = entity.Transform;
        }
        else
        {
            component = entity.AddComponent(type);
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
                // Skip properties that can't be deserialized
            }
        }
    }
}

/// <summary>
/// JSON-serializable representation of a scene.
/// </summary>
public class SceneData
{
    /// <summary>
    /// Scene display name.
    /// </summary>
    public string Name { get; set; } = "Untitled";

    /// <summary>
    /// Saved editor camera position.
    /// </summary>
    public System.Numerics.Vector3? EditorCameraPosition { get; set; }

    /// <summary>
    /// Saved editor camera yaw angle.
    /// </summary>
    public float? EditorCameraYaw { get; set; }

    /// <summary>
    /// Saved editor camera pitch angle.
    /// </summary>
    public float? EditorCameraPitch { get; set; }

    /// <summary>
    /// Scene physics configuration.
    /// </summary>
    public PhysicsSettings? Physics { get; set; } = new();

    /// <summary>
    /// Serialized root entities (children are nested within each entity).
    /// </summary>
    public List<EntityData> Entities { get; set; } = new();
}

/// <summary>
/// JSON-serializable representation of an entity.
/// </summary>
public class EntityData
{
    /// <summary>
    /// Entity display name.
    /// </summary>
    public string Name { get; set; } = "Entity";

    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Whether the entity is active.
    /// </summary>
    public bool Active { get; set; } = true;

    /// <summary>
    /// Optional prefab instance metadata.
    /// </summary>
    public PrefabInstanceMetadata? Prefab { get; set; }

    /// <summary>
    /// Serialized components attached to this entity.
    /// </summary>
    public List<ComponentData> Components { get; set; } = new();

    /// <summary>
    /// Serialized child entities.
    /// </summary>
    public List<EntityData> Children { get; set; } = new();
}

/// <summary>
/// JSON-serializable representation of a component, discriminated by the <c>$type</c> field.
/// </summary>
public class ComponentData
{
    /// <summary>
    /// Fully qualified type name used by <see cref="ComponentTypeResolver"/> for deserialization.
    /// </summary>
    [JsonPropertyName("$type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Whether the component is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Whether the component is editor-only.
    /// </summary>
    public bool EditorOnly { get; set; }

    /// <summary>
    /// Serialized public properties as key-value pairs of JSON elements.
    /// </summary>
    public Dictionary<string, JsonElement> Properties { get; set; } = new();
}
