namespace FrinkyEngine.Core.ECS;

/// <summary>
/// Declares the category path for a component in the Add Component menu.
/// Supports slash-separated nesting (e.g. "Physics/Colliders").
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ComponentCategoryAttribute(string category) : Attribute
{
    /// <summary>The category path.</summary>
    public string Category { get; } = category;
}

/// <summary>
/// Overrides the auto-generated display name for a component in the editor UI.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ComponentDisplayNameAttribute(string name) : Attribute
{
    /// <summary>The display name shown in the editor.</summary>
    public string DisplayName { get; } = name;
}
