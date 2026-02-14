using System.Collections;
using FrinkyEngine.Core.Coroutines;

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
    private CoroutineRunner? _coroutineRunner;
    private TimerRunner? _timerRunner;
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
            if (_enabled)
            {
                ResumeCoroutines();
                OnEnable();
            }
            else
            {
                PauseCoroutines();
                OnDisable();
            }
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

    /// <summary>
    /// Called when a trigger collider on this entity first overlaps another entity.
    /// </summary>
    /// <param name="other">The other entity involved in the trigger overlap.</param>
    public virtual void OnTriggerEnter(Entity other) { }

    /// <summary>
    /// Called each frame while a trigger collider on this entity continues to overlap another entity.
    /// </summary>
    /// <param name="other">The other entity involved in the trigger overlap.</param>
    public virtual void OnTriggerStay(Entity other) { }

    /// <summary>
    /// Called when a trigger collider on this entity stops overlapping another entity.
    /// </summary>
    /// <param name="other">The other entity involved in the trigger overlap.</param>
    public virtual void OnTriggerExit(Entity other) { }

    /// <summary>
    /// Called when a physics collision first begins with another entity.
    /// </summary>
    /// <param name="info">Information about the collision contact.</param>
    public virtual void OnCollisionEnter(Physics.CollisionInfo info) { }

    /// <summary>
    /// Called each frame while a physics collision continues with another entity.
    /// </summary>
    /// <param name="info">Information about the collision contact.</param>
    public virtual void OnCollisionStay(Physics.CollisionInfo info) { }

    /// <summary>
    /// Called when a physics collision ends with another entity.
    /// </summary>
    /// <param name="info">Information about the collision contact.</param>
    public virtual void OnCollisionExit(Physics.CollisionInfo info) { }

    // ── Coroutines ──────────────────────────────────────────────────

    /// <summary>
    /// Starts a coroutine on this component. The coroutine runs each frame during the component update loop
    /// and pauses when the component is disabled. All coroutines are cancelled when the component is destroyed.
    /// </summary>
    /// <param name="routine">An iterator method that yields <see cref="YieldInstruction"/> objects or <c>null</c>.</param>
    /// <returns>A <see cref="Coroutine"/> handle that can be used to stop the coroutine.</returns>
    public Coroutine StartCoroutine(IEnumerator routine)
    {
        ArgumentNullException.ThrowIfNull(routine);
        _coroutineRunner ??= new CoroutineRunner();
        return _coroutineRunner.Start(routine);
    }

    /// <summary>
    /// Stops a specific coroutine that was started on this component.
    /// </summary>
    /// <param name="coroutine">The coroutine handle returned by <see cref="StartCoroutine"/>.</param>
    public void StopCoroutine(Coroutine coroutine)
    {
        _coroutineRunner?.Stop(coroutine);
    }

    /// <summary>
    /// Stops all coroutines running on this component.
    /// </summary>
    public void StopAllCoroutines()
    {
        _coroutineRunner?.StopAll();
    }

    // ── Timers ──────────────────────────────────────────────────────

    /// <summary>
    /// Schedules a callback to be invoked after a delay. The timer respects <see cref="Scene.Scene.TimeScale"/>.
    /// </summary>
    /// <param name="callback">The action to invoke.</param>
    /// <param name="delaySeconds">Time in scaled seconds before the callback fires.</param>
    public void Invoke(Action callback, float delaySeconds)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _timerRunner ??= new TimerRunner();
        _timerRunner.Invoke(callback, delaySeconds);
    }

    /// <summary>
    /// Schedules a callback to be invoked repeatedly. The first invocation occurs after <paramref name="delay"/>,
    /// then every <paramref name="interval"/> seconds. Timers respect <see cref="Scene.Scene.TimeScale"/>.
    /// </summary>
    /// <param name="callback">The action to invoke.</param>
    /// <param name="delay">Initial delay in scaled seconds.</param>
    /// <param name="interval">Interval in scaled seconds between subsequent invocations.</param>
    public void InvokeRepeating(Action callback, float delay, float interval)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _timerRunner ??= new TimerRunner();
        _timerRunner.InvokeRepeating(callback, delay, interval);
    }

    /// <summary>
    /// Cancels all pending timer invocations on this component.
    /// </summary>
    public void CancelInvoke()
    {
        _timerRunner?.CancelAll();
    }

    /// <summary>
    /// Cancels all pending timer invocations that reference a specific callback.
    /// </summary>
    /// <param name="callback">The callback to cancel.</param>
    public void CancelInvoke(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _timerRunner?.Cancel(callback);
    }

    // ── Internal tick methods ───────────────────────────────────────

    /// <summary>
    /// Ticks coroutines and timers. Called by the entity update loop.
    /// </summary>
    internal void TickCoroutinesAndTimers(float scaledDt, float unscaledDt)
    {
        _coroutineRunner?.Tick(scaledDt, unscaledDt);
        _timerRunner?.Tick(scaledDt);
    }

    /// <summary>
    /// Pauses coroutines when the component is disabled. Called internally.
    /// </summary>
    internal void PauseCoroutines()
    {
        _coroutineRunner?.PauseAll();
    }

    /// <summary>
    /// Resumes coroutines when the component is re-enabled. Called internally.
    /// </summary>
    internal void ResumeCoroutines()
    {
        _coroutineRunner?.ResumeAll();
    }

    /// <summary>
    /// Cancels all coroutines and timers. Called on destroy.
    /// </summary>
    internal void CancelAllCoroutinesAndTimers()
    {
        _coroutineRunner?.StopAll();
        _timerRunner?.CancelAll();
    }
}
