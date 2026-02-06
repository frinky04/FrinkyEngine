using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Scene;

public class ComponentRegistry
{
    private readonly Dictionary<Type, List<Component>> _components = new();

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

    public List<T> GetComponents<T>() where T : Component
    {
        if (_components.TryGetValue(typeof(T), out var list))
            return list.Cast<T>().ToList();
        return new List<T>();
    }

    public IReadOnlyList<Component> GetComponentsRaw(Type type)
    {
        if (_components.TryGetValue(type, out var list))
            return list;
        return Array.Empty<Component>();
    }
}
