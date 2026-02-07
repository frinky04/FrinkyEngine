using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Scene;

/// <summary>
/// Fast polymorphic component lookup used internally by <see cref="Scene"/> to maintain quick-access lists.
/// Components are indexed by their concrete type and all base types up to (but not including) <see cref="Component"/>.
/// </summary>
public class ComponentRegistry
{
    private readonly Dictionary<Type, List<Component>> _components = new();

    /// <summary>
    /// Registers a component, indexing it under its concrete type and all intermediate base types.
    /// </summary>
    /// <param name="component">The component to register.</param>
    public void Register(Component component)
    {
        var type = component.GetType();
        while (type != null && type != typeof(Component))
        {
            if (!_components.TryGetValue(type, out var list))
            {
                list = new List<Component>();
                _components[type] = list;
            }
            list.Add(component);
            type = type.BaseType;
        }
    }

    /// <summary>
    /// Removes a component from all type-indexed lists.
    /// </summary>
    /// <param name="component">The component to unregister.</param>
    public void Unregister(Component component)
    {
        var type = component.GetType();
        while (type != null && type != typeof(Component))
        {
            if (_components.TryGetValue(type, out var list))
            {
                list.Remove(component);
            }
            type = type.BaseType;
        }
    }

    /// <summary>
    /// Gets all registered components of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The component type to query.</typeparam>
    /// <returns>A list of matching components (may be empty).</returns>
    public List<T> GetComponents<T>() where T : Component
    {
        if (_components.TryGetValue(typeof(T), out var list))
            return list.Cast<T>().ToList();
        return new List<T>();
    }

    /// <summary>
    /// Gets all registered components of the specified runtime type.
    /// </summary>
    /// <param name="type">The component type to query.</param>
    /// <returns>A read-only list of matching components (may be empty).</returns>
    public IReadOnlyList<Component> GetComponentsRaw(Type type)
    {
        if (_components.TryGetValue(type, out var list))
            return list;
        return Array.Empty<Component>();
    }
}
