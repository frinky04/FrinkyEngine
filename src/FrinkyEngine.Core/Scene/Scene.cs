using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Scene;

public class Scene
{
    public string Name { get; set; } = "Untitled";
    public string FilePath { get; set; } = string.Empty;

    private readonly List<Entity> _entities = new();
    public IReadOnlyList<Entity> Entities => _entities;

    private readonly List<CameraComponent> _cameras = new();
    private readonly List<LightComponent> _lights = new();
    private readonly List<MeshRendererComponent> _renderers = new();
    private readonly List<CubeRendererComponent> _cubeRenderers = new();

    public IReadOnlyList<CameraComponent> Cameras => _cameras;
    public IReadOnlyList<LightComponent> Lights => _lights;
    public IReadOnlyList<MeshRendererComponent> Renderers => _renderers;
    public IReadOnlyList<CubeRendererComponent> CubeRenderers => _cubeRenderers;

    public CameraComponent? MainCamera => _cameras.FirstOrDefault(c => c.IsMain && c.Enabled);

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
            RegisterComponent(c);
    }

    public void RemoveEntity(Entity entity)
    {
        foreach (var c in entity.Components)
            UnregisterComponent(c);

        entity.DestroyComponents();
        entity.Scene = null;
        _entities.Remove(entity);
    }

    internal void OnComponentAdded(Entity entity, Component component)
    {
        RegisterComponent(component);
    }

    internal void OnComponentRemoved(Entity entity, Component component)
    {
        UnregisterComponent(component);
    }

    private void RegisterComponent(Component component)
    {
        switch (component)
        {
            case CameraComponent cam: _cameras.Add(cam); break;
            case LightComponent light: _lights.Add(light); break;
            case MeshRendererComponent renderer: _renderers.Add(renderer); break;
            case CubeRendererComponent cube: _cubeRenderers.Add(cube); break;
        }
    }

    private void UnregisterComponent(Component component)
    {
        switch (component)
        {
            case CameraComponent cam: _cameras.Remove(cam); break;
            case LightComponent light: _lights.Remove(light); break;
            case MeshRendererComponent renderer: _renderers.Remove(renderer); break;
            case CubeRendererComponent cube: _cubeRenderers.Remove(cube); break;
        }
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
