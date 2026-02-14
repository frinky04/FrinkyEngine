using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.Prefabs;
using FrinkyEngine.Core.Serialization;
using System.Reflection;

namespace FrinkyEngine.Core.ECS;

/// <summary>
/// A game object composed of <see cref="Component"/> instances.
/// Every entity always has a <see cref="TransformComponent"/> that cannot be removed.
/// </summary>
public class Entity
{
    /// <summary>
    /// Display name of this entity.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Globally unique identifier for this entity, assigned automatically on creation.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Whether this entity participates in updates and rendering. Inactive entities are skipped.
    /// </summary>
    public bool Active { get; set; } = true;

    /// <summary>
    /// The <see cref="Scene.Scene"/> this entity currently belongs to, or <c>null</c> if not in a scene.
    /// </summary>
    public Scene.Scene? Scene { get; internal set; }

    /// <summary>
    /// The transform component that is always present on every entity.
    /// </summary>
    public TransformComponent Transform { get; }

    /// <summary>
    /// Optional prefab instance metadata for this entity.
    /// </summary>
    public PrefabInstanceMetadata? Prefab { get; set; }

    private readonly List<Component> _components = new();

    /// <summary>
    /// All components currently attached to this entity, including the <see cref="Transform"/>.
    /// </summary>
    public IReadOnlyList<Component> Components => _components;

    private readonly List<ComponentData> _unresolvedComponents = new();

    /// <summary>
    /// Component data that could not be resolved to a type during deserialization.
    /// Preserved so that saving the scene does not lose data for unloaded assemblies.
    /// </summary>
    public IReadOnlyList<ComponentData> UnresolvedComponents => _unresolvedComponents;

    /// <summary>
    /// Returns <c>true</c> if this entity has any unresolved component data from deserialization.
    /// </summary>
    public bool HasUnresolvedComponents => _unresolvedComponents.Count > 0;

    /// <summary>
    /// Adds unresolved component data preserved from deserialization.
    /// </summary>
    public void AddUnresolvedComponent(ComponentData data) => _unresolvedComponents.Add(data);

    /// <summary>
    /// Removes a specific unresolved component entry.
    /// </summary>
    public bool RemoveUnresolvedComponent(ComponentData data) => _unresolvedComponents.Remove(data);

    /// <summary>
    /// Removes all unresolved component data from this entity.
    /// </summary>
    public void ClearUnresolvedComponents() => _unresolvedComponents.Clear();

    /// <summary>
    /// Creates a new entity with the specified name and a default <see cref="TransformComponent"/>.
    /// </summary>
    /// <param name="name">Display name for the entity (defaults to "Entity").</param>
    public Entity(string name = "Entity")
    {
        Name = name;
        Transform = new TransformComponent();
        AddComponentInternal(Transform);
    }

    /// <summary>
    /// Creates and attaches a new component of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The component type to add. Must have a parameterless constructor.</typeparam>
    /// <returns>The newly created component instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <typeparamref name="T"/> is <see cref="TransformComponent"/>.</exception>
    public T AddComponent<T>() where T : Component, new()
    {
        if (typeof(T) == typeof(TransformComponent))
            throw new InvalidOperationException("Cannot add a second TransformComponent.");

        var component = new T();
        AddComponentInternal(component);
        return component;
    }

    /// <summary>
    /// Creates and attaches a new component of the specified runtime type.
    /// </summary>
    /// <param name="type">The component type to add. Must derive from <see cref="Component"/> and have a parameterless constructor.</param>
    /// <returns>The newly created component instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="type"/> is <see cref="TransformComponent"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="type"/> does not derive from <see cref="Component"/>.</exception>
    public Component AddComponent(Type type)
    {
        if (type == typeof(TransformComponent))
            throw new InvalidOperationException("Cannot add a second TransformComponent.");

        if (!type.IsSubclassOf(typeof(Component)))
            throw new ArgumentException($"{type.Name} is not a Component.");

        if (!TryAddComponent(type, out var component, out var failureReason))
            throw new InvalidOperationException(failureReason);

        return component!;
    }

    /// <summary>
    /// Tries to create and attach a new component of the specified runtime type.
    /// </summary>
    /// <param name="type">The component type to add.</param>
    /// <param name="component">The newly created component when successful; otherwise <c>null</c>.</param>
    /// <param name="failureReason">Human-readable failure reason when creation fails.</param>
    /// <returns><c>true</c> if the component was created and added; otherwise <c>false</c>.</returns>
    public bool TryAddComponent(Type type, out Component? component, out string? failureReason)
    {
        if (type == typeof(TransformComponent))
        {
            component = null;
            failureReason = "Cannot add a second TransformComponent.";
            return false;
        }

        if (!type.IsSubclassOf(typeof(Component)))
        {
            component = null;
            failureReason = $"{type.Name} is not a Component.";
            return false;
        }

        if (type.IsAbstract)
        {
            component = null;
            failureReason = $"Component type '{type.FullName}' is abstract and cannot be instantiated.";
            return false;
        }

        if (type.ContainsGenericParameters)
        {
            component = null;
            failureReason = $"Component type '{type.FullName}' has unbound generic parameters and cannot be instantiated.";
            return false;
        }

        if (type.GetConstructor(Type.EmptyTypes) == null)
        {
            component = null;
            failureReason = $"Component type '{type.FullName}' must define a public parameterless constructor.";
            return false;
        }

        try
        {
            component = (Component)Activator.CreateInstance(type)!;
            AddComponentInternal(component);
            failureReason = null;
            return true;
        }
        catch (Exception ex)
        {
            component = null;
            var cause = ex is TargetInvocationException tie && tie.InnerException != null
                ? tie.InnerException
                : ex;
            failureReason = $"Failed to instantiate component type '{type.FullName}': {cause.Message}";
            return false;
        }
    }

    private void AddComponentInternal(Component component)
    {
        component.Entity = this;
        _components.Add(component);
        component.Awake();
        Scene?.OnComponentAdded(this, component);
    }

    /// <summary>
    /// Gets the first component of type <typeparamref name="T"/> attached to this entity.
    /// </summary>
    /// <typeparam name="T">The component type to search for.</typeparam>
    /// <returns>The component instance, or <c>null</c> if none is found.</returns>
    public T? GetComponent<T>() where T : Component
    {
        foreach (var c in _components)
            if (c is T typed) return typed;
        return null;
    }

    /// <summary>
    /// Gets the first component of the specified runtime type attached to this entity.
    /// </summary>
    /// <param name="type">The component type to search for.</param>
    /// <returns>The component instance, or <c>null</c> if none is found.</returns>
    public Component? GetComponent(Type type)
    {
        foreach (var c in _components)
            if (type.IsInstanceOfType(c)) return c;
        return null;
    }

    /// <summary>
    /// Checks whether this entity has a component of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The component type to check for.</typeparam>
    /// <returns><c>true</c> if a matching component exists.</returns>
    public bool HasComponent<T>() where T : Component => GetComponent<T>() != null;

    /// <summary>
    /// Removes the first component of type <typeparamref name="T"/> from this entity.
    /// </summary>
    /// <typeparam name="T">The component type to remove.</typeparam>
    /// <returns><c>true</c> if a component was found and removed.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <typeparamref name="T"/> is <see cref="TransformComponent"/>.</exception>
    public bool RemoveComponent<T>() where T : Component
    {
        if (typeof(T) == typeof(TransformComponent))
            throw new InvalidOperationException("Cannot remove TransformComponent.");

        for (int i = 0; i < _components.Count; i++)
        {
            if (_components[i] is T component)
            {
                Scene?.OnComponentRemoved(this, component);
                component.CancelAllCoroutinesAndTimers();
                component.OnDestroy();
                _components.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Removes a specific component instance from this entity.
    /// </summary>
    /// <param name="component">The component to remove.</param>
    /// <returns><c>true</c> if the component was found and removed.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="component"/> is a <see cref="TransformComponent"/>.</exception>
    public bool RemoveComponent(Component component)
    {
        if (component is TransformComponent)
            throw new InvalidOperationException("Cannot remove TransformComponent.");

        if (_components.Remove(component))
        {
            Scene?.OnComponentRemoved(this, component);
            component.CancelAllCoroutinesAndTimers();
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

    internal void UpdateComponents(float dt, float unscaledDt)
    {
        for (int i = 0; i < _components.Count; i++)
        {
            var c = _components[i];
            if (!c.HasStarted && c.Enabled)
            {
                c.Start();
                c.HasStarted = true;
            }
            if (c.Enabled)
            {
                c.Update(dt);
                c.TickCoroutinesAndTimers(dt, unscaledDt);
            }
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

    /// <summary>
    /// Gets all components of type <typeparamref name="T"/> attached to this entity.
    /// </summary>
    /// <typeparam name="T">The component type to search for.</typeparam>
    /// <returns>A list of matching component instances.</returns>
    public List<T> GetComponents<T>() where T : Component
    {
        var results = new List<T>();
        foreach (var c in _components)
            if (c is T typed) results.Add(typed);
        return results;
    }

    /// <summary>
    /// Gets the first component of type <typeparamref name="T"/> in this entity's children (depth-first).
    /// </summary>
    /// <typeparam name="T">The component type to search for.</typeparam>
    /// <param name="includeInactive">If true, also searches inactive entities.</param>
    /// <returns>The first matching component, or <c>null</c> if none is found.</returns>
    public T? GetComponentInChildren<T>(bool includeInactive = false) where T : Component
    {
        foreach (var child in Transform.Children)
        {
            if (!includeInactive && !child.Entity.Active)
                continue;

            var component = child.Entity.GetComponent<T>();
            if (component != null)
                return component;

            var recursive = child.Entity.GetComponentInChildren<T>(includeInactive);
            if (recursive != null)
                return recursive;
        }
        return null;
    }

    /// <summary>
    /// Gets all components of type <typeparamref name="T"/> in this entity's children (depth-first).
    /// </summary>
    /// <typeparam name="T">The component type to search for.</typeparam>
    /// <param name="includeInactive">If true, also searches inactive entities.</param>
    /// <returns>A list of all matching components in the subtree.</returns>
    public List<T> GetComponentsInChildren<T>(bool includeInactive = false) where T : Component
    {
        var results = new List<T>();
        GetComponentsInChildrenRecursive(results, includeInactive);
        return results;
    }

    private void GetComponentsInChildrenRecursive<T>(List<T> results, bool includeInactive) where T : Component
    {
        foreach (var child in Transform.Children)
        {
            if (!includeInactive && !child.Entity.Active)
                continue;

            foreach (var c in child.Entity._components)
                if (c is T typed) results.Add(typed);

            child.Entity.GetComponentsInChildrenRecursive(results, includeInactive);
        }
    }

    /// <summary>
    /// Gets the first component of type <typeparamref name="T"/> by walking up the parent chain.
    /// </summary>
    /// <typeparam name="T">The component type to search for.</typeparam>
    /// <returns>The first matching component found in a parent, or <c>null</c> if none is found.</returns>
    public T? GetComponentInParent<T>() where T : Component
    {
        var current = Transform.Parent;
        while (current != null)
        {
            var component = current.Entity.GetComponent<T>();
            if (component != null)
                return component;
            current = current.Parent;
        }
        return null;
    }

    /// <summary>
    /// Immediately removes this entity from its scene, destroying it and all its children.
    /// </summary>
    public void Destroy()
    {
        Scene?.RemoveEntity(this);
    }

    /// <summary>
    /// Queues this entity for destruction after the specified delay in seconds.
    /// A delay of 0 destroys the entity at the end of the current frame.
    /// </summary>
    /// <param name="delaySeconds">Time in seconds before the entity is destroyed.</param>
    public void Destroy(float delaySeconds)
    {
        Scene?.QueueDestroy(this, delaySeconds);
    }

    internal void DestroyComponents()
    {
        foreach (var c in _components)
        {
            c.CancelAllCoroutinesAndTimers();
            c.OnDestroy();
        }
    }
}
