namespace FrinkyEngine.Core.ECS;

public abstract class Component
{
    public Entity Entity { get; internal set; } = null!;

    private bool _enabled = true;
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value) return;
            _enabled = value;
            if (_enabled) OnEnable();
            else OnDisable();
        }
    }

    public bool HasStarted { get; internal set; }

    public virtual void Awake() { }
    public virtual void Start() { }
    public virtual void Update(float dt) { }
    public virtual void LateUpdate(float dt) { }
    public virtual void OnDestroy() { }
    public virtual void OnEnable() { }
    public virtual void OnDisable() { }
}
