using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Scene;

public class Scene
{
    public string Name { get; set; } = "Untitled";
    public string FilePath { get; set; } = string.Empty;

    private readonly List<Entity> _entities = new();
    public IReadOnlyList<Entity> Entities => _entities;

    private readonly ComponentRegistry _registry = new();

    public IReadOnlyList<CameraComponent> Cameras => _registry.GetComponents<CameraComponent>();
    public IReadOnlyList<LightComponent> Lights => _registry.GetComponents<LightComponent>();
    public IReadOnlyList<MeshRendererComponent> Renderers => _registry.GetComponents<MeshRendererComponent>();
    public IReadOnlyList<PrimitiveComponent> Primitives => _registry.GetComponents<PrimitiveComponent>();

    public CameraComponent? MainCamera => _registry.GetComponents<CameraComponent>()
        .FirstOrDefault(c => c.IsMain && c.Enabled);

    public List<T> GetComponents<T>() where T : Component => _registry.GetComponents<T>();

    public IReadOnlyList<Component> GetComponents(Type type) => _registry.GetComponentsRaw(type);

    public Entity CreateEntity(string name = "Entity")
    {
        var entity = new Entity(name);
        AddEntity(entity);
        return entity;
    }

    public void AddEntity(Entity entity)
    {
        entity.Scene = this;
        _entities.Add(entity);

        foreach (var c in entity.Components)
            _registry.Register(c);
    }

    public void RemoveEntity(Entity entity)
    {
        foreach (var c in entity.Components)
            _registry.Unregister(c);

        entity.DestroyComponents();
        entity.Scene = null;
        _entities.Remove(entity);
    }

    internal void OnComponentAdded(Entity entity, Component component)
    {
        _registry.Register(component);
    }

    internal void OnComponentRemoved(Entity entity, Component component)
    {
        _registry.Unregister(component);
    }

    public void Start()
    {
        foreach (var entity in _entities)
        {
            if (entity.Active)
                entity.StartComponents();
        }
    }

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
