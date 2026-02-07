using System.Text.Json;

namespace FrinkyEngine.Core.Prefabs;

#pragma warning disable CS1591
public class PrefabInstanceMetadata
{
    public bool IsRoot { get; set; }
    public string AssetPath { get; set; } = string.Empty;
    public string SourceNodeId { get; set; } = string.Empty;
    public PrefabOverridesData? Overrides { get; set; }

    public PrefabInstanceMetadata Clone()
    {
        return new PrefabInstanceMetadata
        {
            IsRoot = IsRoot,
            AssetPath = AssetPath,
            SourceNodeId = SourceNodeId,
            Overrides = Overrides?.Clone()
        };
    }
}

public class PrefabAssetData
{
    public string Name { get; set; } = "Prefab";
    public string SourcePrefab { get; set; } = string.Empty;
    public PrefabNodeData Root { get; set; } = new();
    public PrefabOverridesData VariantOverrides { get; set; } = new();

    public PrefabAssetData Clone()
    {
        return new PrefabAssetData
        {
            Name = Name,
            SourcePrefab = SourcePrefab,
            Root = Root.Clone(),
            VariantOverrides = VariantOverrides.Clone()
        };
    }
}

public class PrefabNodeData
{
    public string StableId { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Entity";
    public bool Active { get; set; } = true;
    public List<PrefabComponentData> Components { get; set; } = new();
    public List<PrefabNodeData> Children { get; set; } = new();

    public PrefabNodeData Clone()
    {
        return new PrefabNodeData
        {
            StableId = StableId,
            Name = Name,
            Active = Active,
            Components = Components.Select(c => c.Clone()).ToList(),
            Children = Children.Select(c => c.Clone()).ToList()
        };
    }
}

public class PrefabComponentData
{
    public string Type { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool EditorOnly { get; set; }
    public Dictionary<string, JsonElement> Properties { get; set; } = new();

    public PrefabComponentData Clone()
    {
        var clone = new PrefabComponentData
        {
            Type = Type,
            Enabled = Enabled,
            EditorOnly = EditorOnly
        };

        foreach (var (key, value) in Properties)
            clone.Properties[key] = PrefabJson.CloneElement(value);

        return clone;
    }
}

public class PrefabOverridesData
{
    public List<PrefabPropertyOverrideData> PropertyOverrides { get; set; } = new();
    public List<PrefabComponentOverrideData> AddedComponents { get; set; } = new();
    public List<PrefabRemovedComponentOverrideData> RemovedComponents { get; set; } = new();
    public List<PrefabAddedChildOverrideData> AddedChildren { get; set; } = new();
    public List<string> RemovedChildren { get; set; } = new();

    public PrefabOverridesData Clone()
    {
        return new PrefabOverridesData
        {
            PropertyOverrides = PropertyOverrides.Select(o => o.Clone()).ToList(),
            AddedComponents = AddedComponents.Select(o => o.Clone()).ToList(),
            RemovedComponents = RemovedComponents.Select(o => o.Clone()).ToList(),
            AddedChildren = AddedChildren.Select(o => o.Clone()).ToList(),
            RemovedChildren = RemovedChildren.ToList()
        };
    }
}

public class PrefabPropertyOverrideData
{
    public string NodeId { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public JsonElement Value { get; set; }

    public PrefabPropertyOverrideData Clone()
    {
        return new PrefabPropertyOverrideData
        {
            NodeId = NodeId,
            ComponentType = ComponentType,
            PropertyName = PropertyName,
            Value = PrefabJson.CloneElement(Value)
        };
    }
}

public class PrefabComponentOverrideData
{
    public string NodeId { get; set; } = string.Empty;
    public PrefabComponentData Component { get; set; } = new();

    public PrefabComponentOverrideData Clone()
    {
        return new PrefabComponentOverrideData
        {
            NodeId = NodeId,
            Component = Component.Clone()
        };
    }
}

public class PrefabRemovedComponentOverrideData
{
    public string NodeId { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;

    public PrefabRemovedComponentOverrideData Clone()
    {
        return new PrefabRemovedComponentOverrideData
        {
            NodeId = NodeId,
            ComponentType = ComponentType
        };
    }
}

public class PrefabAddedChildOverrideData
{
    public string ParentNodeId { get; set; } = string.Empty;
    public PrefabNodeData Child { get; set; } = new();

    public PrefabAddedChildOverrideData Clone()
    {
        return new PrefabAddedChildOverrideData
        {
            ParentNodeId = ParentNodeId,
            Child = Child.Clone()
        };
    }
}

public static class PrefabJson
{
    public static JsonElement CloneElement(JsonElement element)
    {
        using var document = JsonDocument.Parse(element.GetRawText());
        return document.RootElement.Clone();
    }
}
#pragma warning restore CS1591
