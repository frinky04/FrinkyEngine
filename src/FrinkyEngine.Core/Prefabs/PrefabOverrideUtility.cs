using System.Text.Json;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Serialization;

namespace FrinkyEngine.Core.Prefabs;

public static class PrefabOverrideUtility
{
    public const string EntityPseudoComponent = "__entity";
    public const string NameProperty = "__name";
    public const string ActiveProperty = "__active";
    public const string EnabledProperty = "__enabled";
    public const string EditorOnlyProperty = "__editorOnly";
    private static readonly Dictionary<string, JsonElement> DefaultPropertyValueCache = new(StringComparer.Ordinal);

    public static PrefabOverridesData ComputeOverrides(PrefabNodeData sourceRoot, PrefabNodeData instanceRoot)
    {
        var overrides = new PrefabOverridesData();
        CompareNode(sourceRoot, instanceRoot, overrides, sourceRoot.StableId);
        return overrides;
    }

    public static void ApplyOverrides(PrefabNodeData root, PrefabOverridesData? overrides)
    {
        if (overrides == null)
            return;

        if (overrides.RemovedChildren.Count > 0)
        {
            var removed = new HashSet<string>(overrides.RemovedChildren, StringComparer.OrdinalIgnoreCase);
            RemoveChildrenByStableId(root, removed);
        }

        var nodeMap = BuildNodeMap(root);
        foreach (var added in overrides.AddedChildren)
        {
            if (nodeMap.TryGetValue(added.ParentNodeId, out var parent))
                parent.Children.Add(added.Child.Clone());
        }

        nodeMap = BuildNodeMap(root);
        foreach (var removed in overrides.RemovedComponents)
        {
            if (!nodeMap.TryGetValue(removed.NodeId, out var node))
                continue;

            node.Components.RemoveAll(c => string.Equals(c.Type, removed.ComponentType, StringComparison.Ordinal));
        }

        foreach (var added in overrides.AddedComponents)
        {
            if (!nodeMap.TryGetValue(added.NodeId, out var node))
                continue;

            var existing = node.Components.FirstOrDefault(c => string.Equals(c.Type, added.Component.Type, StringComparison.Ordinal));
            if (existing != null)
                node.Components.Remove(existing);

            node.Components.Add(added.Component.Clone());
        }

        nodeMap = BuildNodeMap(root);
        foreach (var propertyOverride in overrides.PropertyOverrides)
        {
            if (!nodeMap.TryGetValue(propertyOverride.NodeId, out var node))
                continue;

            ApplyPropertyOverride(root.StableId, node, propertyOverride);
        }
    }

    private static void CompareNode(PrefabNodeData source, PrefabNodeData instance, PrefabOverridesData overrides, string rootNodeId)
    {
        if (!IsRootEntityNameProperty(rootNodeId, source.StableId) &&
            !string.Equals(source.Name, instance.Name, StringComparison.Ordinal))
        {
            overrides.PropertyOverrides.Add(new PrefabPropertyOverrideData
            {
                NodeId = source.StableId,
                ComponentType = EntityPseudoComponent,
                PropertyName = NameProperty,
                Value = PrefabSerializer.SerializeValue(instance.Name, typeof(string))
            });
        }

        if (source.Active != instance.Active)
        {
            overrides.PropertyOverrides.Add(new PrefabPropertyOverrideData
            {
                NodeId = source.StableId,
                ComponentType = EntityPseudoComponent,
                PropertyName = ActiveProperty,
                Value = PrefabSerializer.SerializeValue(instance.Active, typeof(bool))
            });
        }

        var sourceComponentsByType = BuildComponentQueues(source.Components);
        var instanceComponentsByType = BuildComponentQueues(instance.Components);
        var componentTypes = new HashSet<string>(sourceComponentsByType.Keys, StringComparer.Ordinal);
        componentTypes.UnionWith(instanceComponentsByType.Keys);

        foreach (var componentType in componentTypes)
        {
            sourceComponentsByType.TryGetValue(componentType, out var sourceQueue);
            instanceComponentsByType.TryGetValue(componentType, out var instanceQueue);
            sourceQueue ??= new Queue<PrefabComponentData>();
            instanceQueue ??= new Queue<PrefabComponentData>();

            while (sourceQueue.Count > 0 && instanceQueue.Count > 0)
                CompareComponent(rootNodeId, source.StableId, sourceQueue.Dequeue(), instanceQueue.Dequeue(), overrides);

            while (sourceQueue.Count > 0)
            {
                sourceQueue.Dequeue();
                overrides.RemovedComponents.Add(new PrefabRemovedComponentOverrideData
                {
                    NodeId = source.StableId,
                    ComponentType = componentType
                });
            }

            while (instanceQueue.Count > 0)
            {
                var addedComponent = instanceQueue.Dequeue();
                overrides.AddedComponents.Add(new PrefabComponentOverrideData
                {
                    NodeId = source.StableId,
                    Component = addedComponent.Clone()
                });
            }
        }

        var sourceChildrenById = BuildChildQueues(source.Children);

        foreach (var instanceChild in instance.Children)
        {
            if (!string.IsNullOrWhiteSpace(instanceChild.StableId) &&
                sourceChildrenById.TryGetValue(instanceChild.StableId, out var sourceQueue) &&
                sourceQueue.Count > 0)
            {
                var sourceChild = sourceQueue.Dequeue();
                CompareNode(sourceChild, instanceChild, overrides, rootNodeId);
                continue;
            }

            EnsureStableIds(instanceChild);
            overrides.AddedChildren.Add(new PrefabAddedChildOverrideData
            {
                ParentNodeId = source.StableId,
                Child = instanceChild.Clone()
            });
        }

        foreach (var (_, sourceQueue) in sourceChildrenById)
        {
            while (sourceQueue.Count > 0)
            {
                var removedChild = sourceQueue.Dequeue();
                overrides.RemovedChildren.Add(removedChild.StableId);
            }
        }
    }

    private static void CompareComponent(
        string rootNodeId,
        string nodeId,
        PrefabComponentData sourceComponent,
        PrefabComponentData instanceComponent,
        PrefabOverridesData overrides)
    {
        if (sourceComponent.Enabled != instanceComponent.Enabled)
        {
            overrides.PropertyOverrides.Add(new PrefabPropertyOverrideData
            {
                NodeId = nodeId,
                ComponentType = sourceComponent.Type,
                PropertyName = EnabledProperty,
                Value = PrefabSerializer.SerializeValue(instanceComponent.Enabled, typeof(bool))
            });
        }

        if (sourceComponent.EditorOnly != instanceComponent.EditorOnly)
        {
            overrides.PropertyOverrides.Add(new PrefabPropertyOverrideData
            {
                NodeId = nodeId,
                ComponentType = sourceComponent.Type,
                PropertyName = EditorOnlyProperty,
                Value = PrefabSerializer.SerializeValue(instanceComponent.EditorOnly, typeof(bool))
            });
        }

        var keys = new HashSet<string>(sourceComponent.Properties.Keys, StringComparer.Ordinal);
        keys.UnionWith(instanceComponent.Properties.Keys);

        foreach (var key in keys)
        {
            if (ShouldIgnoreRootTransformPlacementProperty(rootNodeId, nodeId, sourceComponent.Type, key))
                continue;

            bool hasSource = sourceComponent.Properties.TryGetValue(key, out var sourceValue);
            bool hasInstance = instanceComponent.Properties.TryGetValue(key, out var instanceValue);

            if (hasSource != hasInstance &&
                TryGetDefaultPropertyValue(sourceComponent.Type, key, out var defaultValue))
            {
                var presentValue = hasSource ? sourceValue : instanceValue;
                if (JsonEquals(presentValue, defaultValue))
                    continue;
            }

            if (hasSource && hasInstance && JsonEquals(sourceValue, instanceValue))
                continue;

            overrides.PropertyOverrides.Add(new PrefabPropertyOverrideData
            {
                NodeId = nodeId,
                ComponentType = sourceComponent.Type,
                PropertyName = key,
                Value = hasInstance ? PrefabJson.CloneElement(instanceValue) : JsonSerializer.SerializeToElement<string?>(null)
            });
        }
    }

    private static bool JsonEquals(JsonElement left, JsonElement right)
    {
        if (left.ValueKind != right.ValueKind)
        {
            // Treat numeric representations as equal when their values match.
            if (left.ValueKind == JsonValueKind.Number && right.ValueKind == JsonValueKind.Number)
                return JsonNumberEquals(left, right);
            return false;
        }

        switch (left.ValueKind)
        {
            case JsonValueKind.Object:
                return JsonObjectEquals(left, right);
            case JsonValueKind.Array:
                return JsonArrayEquals(left, right);
            case JsonValueKind.Number:
                return JsonNumberEquals(left, right);
            case JsonValueKind.String:
                return string.Equals(left.GetString(), right.GetString(), StringComparison.Ordinal);
            case JsonValueKind.True:
            case JsonValueKind.False:
                return left.GetBoolean() == right.GetBoolean();
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return true;
            default:
                return string.Equals(left.GetRawText(), right.GetRawText(), StringComparison.Ordinal);
        }
    }

    private static bool JsonObjectEquals(JsonElement left, JsonElement right)
    {
        var leftProps = left.EnumerateObject().ToList();
        var rightProps = right.EnumerateObject().ToList();
        if (leftProps.Count != rightProps.Count)
            return false;

        var rightMap = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var prop in rightProps)
            rightMap[prop.Name] = prop.Value;

        foreach (var prop in leftProps)
        {
            if (!rightMap.TryGetValue(prop.Name, out var rightValue))
                return false;
            if (!JsonEquals(prop.Value, rightValue))
                return false;
        }

        return true;
    }

    private static bool JsonArrayEquals(JsonElement left, JsonElement right)
    {
        var leftItems = left.EnumerateArray().ToList();
        var rightItems = right.EnumerateArray().ToList();
        if (leftItems.Count != rightItems.Count)
            return false;

        for (int i = 0; i < leftItems.Count; i++)
        {
            if (!JsonEquals(leftItems[i], rightItems[i]))
                return false;
        }

        return true;
    }

    private static bool JsonNumberEquals(JsonElement left, JsonElement right)
    {
        const double epsilon = 1e-6;

        if (left.TryGetInt64(out var leftInt) && right.TryGetInt64(out var rightInt))
            return leftInt == rightInt;

        if (!left.TryGetDouble(out var leftDouble) || !right.TryGetDouble(out var rightDouble))
            return string.Equals(left.GetRawText(), right.GetRawText(), StringComparison.Ordinal);

        return Math.Abs(leftDouble - rightDouble) <= epsilon;
    }

    private static bool TryGetDefaultPropertyValue(string componentTypeName, string propertyName, out JsonElement defaultValue)
    {
        string cacheKey = componentTypeName + "|" + propertyName;
        if (DefaultPropertyValueCache.TryGetValue(cacheKey, out var cached))
        {
            defaultValue = PrefabJson.CloneElement(cached);
            return true;
        }

        defaultValue = default;
        var componentType = ComponentTypeResolver.Resolve(componentTypeName);
        if (componentType == null)
            return false;

        var property = componentType.GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (property == null || !property.CanRead)
            return false;

        try
        {
            Component? instance = componentType == typeof(FrinkyEngine.Core.Components.TransformComponent)
                ? new FrinkyEngine.Core.Components.TransformComponent()
                : Activator.CreateInstance(componentType) as Component;
            if (instance == null)
                return false;

            object? value = property.GetValue(instance);
            var serialized = value == null
                ? JsonSerializer.SerializeToElement<string?>(null)
                : PrefabSerializer.SerializeValue(value, property.PropertyType);

            DefaultPropertyValueCache[cacheKey] = PrefabJson.CloneElement(serialized);
            defaultValue = serialized;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void ApplyPropertyOverride(string rootNodeId, PrefabNodeData node, PrefabPropertyOverrideData propertyOverride)
    {
        if (string.Equals(propertyOverride.ComponentType, EntityPseudoComponent, StringComparison.Ordinal))
        {
            if (string.Equals(propertyOverride.PropertyName, NameProperty, StringComparison.Ordinal))
            {
                if (IsRootEntityNameProperty(rootNodeId, propertyOverride.NodeId))
                    return;

                node.Name = propertyOverride.Value.ValueKind == JsonValueKind.String
                    ? propertyOverride.Value.GetString() ?? node.Name
                    : node.Name;
            }
            else if (string.Equals(propertyOverride.PropertyName, ActiveProperty, StringComparison.Ordinal))
                node.Active = propertyOverride.Value.ValueKind == JsonValueKind.True
                              || (propertyOverride.Value.ValueKind == JsonValueKind.False
                                  ? false
                                  : node.Active);
            return;
        }

        var component = node.Components.FirstOrDefault(c => string.Equals(c.Type, propertyOverride.ComponentType, StringComparison.Ordinal));
        if (component == null)
            return;

        if (IsRootTransformPlacementOverride(rootNodeId, propertyOverride, component.Type))
            return;

        if (string.Equals(propertyOverride.PropertyName, EnabledProperty, StringComparison.Ordinal))
        {
            if (propertyOverride.Value.ValueKind is JsonValueKind.True or JsonValueKind.False)
                component.Enabled = propertyOverride.Value.GetBoolean();
            return;
        }

        if (string.Equals(propertyOverride.PropertyName, EditorOnlyProperty, StringComparison.Ordinal))
        {
            if (propertyOverride.Value.ValueKind is JsonValueKind.True or JsonValueKind.False)
                component.EditorOnly = propertyOverride.Value.GetBoolean();
            return;
        }

        component.Properties[propertyOverride.PropertyName] = PrefabJson.CloneElement(propertyOverride.Value);
    }

    private static bool IsRootTransformPlacementOverride(string rootNodeId, PrefabPropertyOverrideData propertyOverride, string componentType)
    {
        if (!string.Equals(propertyOverride.NodeId, rootNodeId, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!IsTransformComponentType(componentType))
            return false;

        return string.Equals(propertyOverride.PropertyName, "LocalPosition", StringComparison.Ordinal)
               || string.Equals(propertyOverride.PropertyName, "LocalRotation", StringComparison.Ordinal)
               || string.Equals(propertyOverride.PropertyName, "LocalScale", StringComparison.Ordinal);
    }

    private static bool IsRootEntityNameProperty(string rootNodeId, string nodeId)
    {
        return string.Equals(nodeId, rootNodeId, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTransformComponentType(string componentType)
    {
        if (string.Equals(componentType, typeof(FrinkyEngine.Core.Components.TransformComponent).FullName, StringComparison.Ordinal)
            || string.Equals(componentType, nameof(FrinkyEngine.Core.Components.TransformComponent), StringComparison.Ordinal))
        {
            return true;
        }

        var resolved = ComponentTypeResolver.Resolve(componentType);
        return resolved == typeof(FrinkyEngine.Core.Components.TransformComponent);
    }

    private static Dictionary<string, PrefabNodeData> BuildNodeMap(PrefabNodeData root)
    {
        var map = new Dictionary<string, PrefabNodeData>(StringComparer.OrdinalIgnoreCase);
        Traverse(root, node => map[node.StableId] = node);
        return map;
    }

    private static void Traverse(PrefabNodeData node, Action<PrefabNodeData> visitor)
    {
        visitor(node);
        foreach (var child in node.Children)
            Traverse(child, visitor);
    }

    private static void RemoveChildrenByStableId(PrefabNodeData parent, HashSet<string> removedIds)
    {
        parent.Children.RemoveAll(child => removedIds.Contains(child.StableId));
        foreach (var child in parent.Children)
            RemoveChildrenByStableId(child, removedIds);
    }

    private static void EnsureStableIds(PrefabNodeData node)
    {
        if (string.IsNullOrWhiteSpace(node.StableId))
            node.StableId = Guid.NewGuid().ToString("N");

        foreach (var child in node.Children)
            EnsureStableIds(child);
    }

    private static Dictionary<string, Queue<PrefabNodeData>> BuildChildQueues(IEnumerable<PrefabNodeData> children)
    {
        var lookup = new Dictionary<string, Queue<PrefabNodeData>>(StringComparer.OrdinalIgnoreCase);
        foreach (var child in children)
        {
            if (string.IsNullOrWhiteSpace(child.StableId))
                continue;

            if (!lookup.TryGetValue(child.StableId, out var queue))
            {
                queue = new Queue<PrefabNodeData>();
                lookup[child.StableId] = queue;
            }

            queue.Enqueue(child);
        }

        return lookup;
    }

    private static Dictionary<string, Queue<PrefabComponentData>> BuildComponentQueues(IEnumerable<PrefabComponentData> components)
    {
        var lookup = new Dictionary<string, Queue<PrefabComponentData>>(StringComparer.Ordinal);
        foreach (var component in components)
        {
            var key = component.Type ?? string.Empty;
            if (!lookup.TryGetValue(key, out var queue))
            {
                queue = new Queue<PrefabComponentData>();
                lookup[key] = queue;
            }

            queue.Enqueue(component);
        }

        return lookup;
    }

    private static bool ShouldIgnoreRootTransformPlacementProperty(
        string rootNodeId,
        string nodeId,
        string componentType,
        string propertyName)
    {
        if (!string.Equals(nodeId, rootNodeId, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!IsTransformComponentType(componentType))
            return false;

        return string.Equals(propertyName, "LocalPosition", StringComparison.Ordinal)
               || string.Equals(propertyName, "LocalRotation", StringComparison.Ordinal)
               || string.Equals(propertyName, "LocalScale", StringComparison.Ordinal);
    }
}
