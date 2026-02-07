using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Scene;

/// <summary>
/// A container of <see cref="Entity"/> instances that make up a game level or environment.
/// Maintains quick-access lists for cameras, lights, and renderables.
/// </summary>
public class Scene
{
    /// <summary>
    /// Display name of this scene.
    /// </summary>
    public string Name { get; set; } = "Untitled";

    /// <summary>
    /// File path this scene was last saved to or loaded from.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    // Editor camera metadata (persisted in .fscene)

    /// <summary>
    /// Saved editor camera position, restored when the scene is reopened in the editor.
    /// </summary>
    public System.Numerics.Vector3? EditorCameraPosition { get; set; }

    /// <summary>
    /// Saved editor camera yaw angle in degrees.
    /// </summary>
    public float? EditorCameraYaw { get; set; }

    /// <summary>
    /// Saved editor camera pitch angle in degrees.
    /// </summary>
    public float? EditorCameraPitch { get; set; }

    private readonly List<Entity> _entities = new();

    /// <summary>
    /// All entities currently in this scene.
    /// </summary>
    public IReadOnlyList<Entity> Entities => _entities;

    private readonly ComponentRegistry _registry = new();

    /// <summary>
    /// All active <see cref="CameraComponent"/> instances in the scene.
    /// </summary>
    public IReadOnlyList<CameraComponent> Cameras => _registry.GetComponents<CameraComponent>();

    /// <summary>
    /// All active <see cref="LightComponent"/> instances in the scene.
    /// </summary>
    public IReadOnlyList<LightComponent> Lights => _registry.GetComponents<LightComponent>();

    /// <summary>
    /// All active <see cref="RenderableComponent"/> instances in the scene.
    /// </summary>
    public IReadOnlyList<RenderableComponent> Renderables => _registry.GetComponents<RenderableComponent>();

    /// <summary>
    /// Gets the first enabled camera marked as <see cref="CameraComponent.IsMain"/>, or <c>null</c> if none exists.
    /// </summary>
    public CameraComponent? MainCamera => _registry.GetComponents<CameraComponent>()
        .FirstOrDefault(c => c.IsMain && c.Enabled);

    /// <summary>
    /// Gets all components of type <typeparamref name="T"/> across all entities in the scene.
    /// </summary>
    /// <typeparam name="T">The component type to search for.</typeparam>
    /// <returns>A list of matching components.</returns>
    public List<T> GetComponents<T>() where T : Component => _registry.GetComponents<T>();

    /// <summary>
    /// Gets all components of the specified runtime type across all entities in the scene.
    /// </summary>
    /// <param name="type">The component type to search for.</param>
    /// <returns>A read-only list of matching components.</returns>
    public IReadOnlyList<Component> GetComponents(Type type) => _registry.GetComponentsRaw(type);

    /// <summary>
    /// Creates a new entity with the given name and adds it to the scene.
    /// </summary>
    /// <param name="name">Display name for the entity.</param>
    /// <returns>The newly created entity.</returns>
    public Entity CreateEntity(string name = "Entity")
    {
        var entity = new Entity(name);
        AddEntity(entity);
        return entity;
    }

    /// <summary>
    /// Adds an existing entity to this scene and registers all its components.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    public void AddEntity(Entity entity)
    {
        entity.Scene = this;
        _entities.Add(entity);

        foreach (var c in entity.Components)
            _registry.Register(c);
    }

    /// <summary>
    /// Removes an entity from this scene, destroying all its components.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    public void RemoveEntity(Entity entity)
    {
        var subtree = new List<Entity>();
        CollectEntitySubtree(entity, subtree);

        // Remove children before parents so hierarchy links are cleaned safely.
        for (int i = subtree.Count - 1; i >= 0; i--)
        {
            var current = subtree[i];
            if (current.Scene != this)
                continue;

            current.Transform.SetParent(null);

            foreach (var c in current.Components)
                _registry.Unregister(c);

            current.DestroyComponents();
            current.Scene = null;
            _entities.Remove(current);
        }
    }

    private static void CollectEntitySubtree(Entity entity, List<Entity> results)
    {
        results.Add(entity);
        foreach (var child in entity.Transform.Children.ToList())
            CollectEntitySubtree(child.Entity, results);
    }

    internal void OnComponentAdded(Entity entity, Component component)
    {
        _registry.Register(component);
    }

    internal void OnComponentRemoved(Entity entity, Component component)
    {
        _registry.Unregister(component);
    }

    /// <summary>
    /// Calls <see cref="Component.Start"/> on all components that haven't started yet.
    /// </summary>
    public void Start()
    {
        foreach (var entity in _entities)
        {
            if (entity.Active)
                entity.StartComponents();
        }
    }

    /// <summary>
    /// Runs one frame of the game loop — calls <see cref="Component.Update"/> then <see cref="Component.LateUpdate"/> on all active entities.
    /// </summary>
    /// <param name="dt">Time elapsed since the previous frame, in seconds.</param>
    public void Update(float dt)
    {
        foreach (var entity in _entities)
        {
            if (entity.Active)
                entity.UpdateComponents(dt);
        }

        foreach (var entity in _entities)
        {
            if (entity.Active)
                entity.LateUpdateComponents(dt);
        }
    }
}
