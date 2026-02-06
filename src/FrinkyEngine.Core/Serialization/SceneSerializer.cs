using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Core.Serialization;

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
            new JsonStringEnumConverter()
        }
    };

    public static void Save(Scene.Scene scene, string path)
    {
        var data = SerializeScene(scene);
        var json = JsonSerializer.Serialize(data, JsonOptions);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(path, json);
    }

    public static Scene.Scene? Load(string path)
    {
        if (!File.Exists(path)) return null;
        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<SceneData>(json, JsonOptions);
        if (data == null) return null;
        return DeserializeScene(data);
    }

    public static string SerializeToString(Scene.Scene scene)
    {
        var data = SerializeScene(scene);
        return JsonSerializer.Serialize(data, JsonOptions);
    }

    public static Scene.Scene? DeserializeFromString(string json)
    {
        var data = JsonSerializer.Deserialize<SceneData>(json, JsonOptions);
        if (data == null) return null;
        return DeserializeScene(data);
    }

    private static SceneData SerializeScene(Scene.Scene scene)
    {
        var data = new SceneData { Name = scene.Name };
        foreach (var entity in scene.Entities)
        {
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
            Active = entity.Active
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
            Enabled = component.Enabled
        };

        var type = component.GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;
            if (prop.Name is "Entity" or "HasStarted" or "Enabled") continue;
            if (prop.Name == "LoadedModel") continue;

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
        var scene = new Scene.Scene { Name = data.Name };
        var parentMap = new Dictionary<Entity, List<EntityData>>();

        foreach (var entityData in data.Entities)
        {
            DeserializeEntityTree(entityData, scene, null);
        }

        return scene;
    }

    private static void DeserializeEntityTree(EntityData data, Scene.Scene scene, TransformComponent? parent)
    {
        var entity = new Entity(data.Name)
        {
            Id = data.Id,
            Active = data.Active
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

        foreach (var (propName, jsonElement) in data.Properties)
        {
            var prop = type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null || !prop.CanWrite) continue;
            if (prop.Name is "Entity" or "HasStarted" or "Enabled") continue;

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

public class SceneData
{
    public string Name { get; set; } = "Untitled";
    public List<EntityData> Entities { get; set; } = new();
}

public class EntityData
{
    public string Name { get; set; } = "Entity";
    public Guid Id { get; set; }
    public bool Active { get; set; } = true;
    public List<ComponentData> Components { get; set; } = new();
    public List<EntityData> Children { get; set; } = new();
}

public class ComponentData
{
    [JsonPropertyName("$type")]
    public string Type { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public Dictionary<string, JsonElement> Properties { get; set; } = new();
}
