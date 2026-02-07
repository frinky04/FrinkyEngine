namespace FrinkyEngine.Core.ECS;

/// <summary>
/// Base class for all components that can be attached to an <see cref="Entity"/>.
/// </summary>
/// <remarks>
/// Components follow a Unity-style lifecycle: <see cref="Awake"/> is called when the component is added,
/// <see cref="Start"/> runs before the first update (only if <see cref="Enabled"/>),
/// then <see cref="Update"/> and <see cref="LateUpdate"/> run each frame.
/// <see cref="OnDestroy"/> is called when the component is removed or the entity is destroyed.
/// </remarks>
public abstract class Component
{
    /// <summary>
    /// The <see cref="ECS.Entity"/> this component is attached to.
    /// </summary>
    public Entity Entity { get; internal set; } = null!;

    private bool _enabled = true;
    /// <summary>
    /// Whether this component is active. Disabled components skip <see cref="Update"/> and <see cref="LateUpdate"/>.
    /// </summary>
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

    /// <summary>
    /// When <c>true</c>, this component is only active in the editor and is skipped during runtime play.
    /// </summary>
    public bool EditorOnly { get; set; }

    /// <summary>
    /// Indicates whether <see cref="Start"/> has already been called on this component.
    /// </summary>
    public bool HasStarted { get; internal set; }

    /// <summary>
    /// Called once when the component is first added to an entity, before <see cref="Start"/>.
    /// </summary>
    public virtual void Awake() { }

    /// <summary>
    /// Called once before the first <see cref="Update"/>, only if the component is <see cref="Enabled"/>.
    /// </summary>
    public virtual void Start() { }

    /// <summary>
    /// Called every frame while the component is enabled.
    /// </summary>
    /// <param name="dt">Time elapsed since the previous frame, in seconds.</param>
    public virtual void Update(float dt) { }

    /// <summary>
    /// Called every frame after all <see cref="Update"/> calls have completed.
    /// </summary>
    /// <param name="dt">Time elapsed since the previous frame, in seconds.</param>
    public virtual void LateUpdate(float dt) { }

    /// <summary>
    /// Called when the component is removed from its entity or the entity is destroyed.
    /// </summary>
    public virtual void OnDestroy() { }

    /// <summary>
    /// Called when the component transitions from disabled to enabled.
    /// </summary>
    public virtual void OnEnable() { }

    /// <summary>
    /// Called when the component transitions from enabled to disabled.
    /// </summary>
    public virtual void OnDisable() { }
}
