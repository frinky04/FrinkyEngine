using FrinkyEngine.Core.Components;

namespace FrinkyEngine.Core.ECS;

public class Entity
{
    public string Name { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool Active { get; set; } = true;

    public Scene.Scene? Scene { get; internal set; }
    public TransformComponent Transform { get; }

    private readonly List<Component> _components = new();
    public IReadOnlyList<Component> Components => _components;

    public Entity(string name = "Entity")
    {
        Name = name;
        Transform = new TransformComponent();
        AddComponentInternal(Transform);
    }

    public T AddComponent<T>() where T : Component, new()
    {
        if (typeof(T) == typeof(TransformComponent))
            throw new InvalidOperationException("Cannot add a second TransformComponent.");

        var component = new T();
        AddComponentInternal(component);
        return component;
    }

    public Component AddComponent(Type type)
    {
        if (type == typeof(TransformComponent))
            throw new InvalidOperationException("Cannot add a second TransformComponent.");

        if (!type.IsSubclassOf(typeof(Component)))
            throw new ArgumentException($"{type.Name} is not a Component.");

        var component = (Component)Activator.CreateInstance(type)!;
        AddComponentInternal(component);
        return component;
    }

    private void AddComponentInternal(Component component)
    {
        component.Entity = this;
        _components.Add(component);
        component.Awake();
        Scene?.OnComponentAdded(this, component);
    }

    public T? GetComponent<T>() where T : Component
    {
        foreach (var c in _components)
            if (c is T typed) return typed;
        return null;
    }

    public Component? GetComponent(Type type)
    {
        foreach (var c in _components)
            if (type.IsInstanceOfType(c)) return c;
        return null;
    }

    public bool HasComponent<T>() where T : Component => GetComponent<T>() != null;

    public bool RemoveComponent<T>() where T : Component
    {
        if (typeof(T) == typeof(TransformComponent))
            throw new InvalidOperationException("Cannot remove TransformComponent.");

        for (int i = 0; i < _components.Count; i++)
        {
            if (_components[i] is T component)
            {
                Scene?.OnComponentRemoved(this, component);
                component.OnDestroy();
                _components.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public bool RemoveComponent(Component component)
    {
        if (component is TransformComponent)
            throw new InvalidOperationException("Cannot remove TransformComponent.");

        if (_components.Remove(component))
        {
            Scene?.OnComponentRemoved(this, component);
            component.OnDestroy();
            return true;
        }
        return false;
    }

    internal void StartComponents()
    {
        foreach (var c in _components)
        {
            if (!c.HasStarted && c.Enabled)
            {
                c.Start();
                c.HasStarted = true;
            }
        }
    }

    internal void UpdateComponents(float dt)
    {
        for (int i = 0; i < _components.Count; i++)
        {
            var c = _components[i];
            if (!c.HasStarted && c.Enabled)
            {
                c.Start();
                c.HasStarted = true;
            }
            if (c.Enabled) c.Update(dt);
        }
    }

    internal void LateUpdateComponents(float dt)
    {
        for (int i = 0; i < _components.Count; i++)
        {
            var c = _components[i];
            if (c.Enabled) c.LateUpdate(dt);
        }
    }

    internal void DestroyComponents()
    {
        foreach (var c in _components)
            c.OnDestroy();
    }
}
