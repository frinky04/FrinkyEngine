#pragma warning disable CS1591

namespace FrinkyEngine.Core.ECS;

/// <summary>
/// Base class for polymorphic data objects owned by components.
/// Subclass this to create configurable, type-selectable data (AI behaviors, weapon configs, etc.).
/// Public read/write properties are auto-serialized and drawn in the inspector.
/// </summary>
public abstract class FObject
{
    /// <summary>
    /// Human-readable name shown in the editor UI. Defaults to the type name.
    /// </summary>
    public virtual string DisplayName => GetType().Name;
}
