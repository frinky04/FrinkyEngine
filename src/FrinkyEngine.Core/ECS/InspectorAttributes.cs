namespace FrinkyEngine.Core.ECS;

/// <summary>
/// Groups a property under a labeled inspector section in the editor.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class InspectorSectionAttribute(string title) : Attribute
{
    /// <summary>
    /// Section title shown above the property.
    /// </summary>
    public string Title { get; } = title;
}

/// <summary>
/// Overrides the property label shown in the inspector.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class InspectorLabelAttribute(string label) : Attribute
{
    /// <summary>
    /// Display label.
    /// </summary>
    public string Label { get; } = label;
}

/// <summary>
/// Shows the annotated property only when another boolean property matches the expected value.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
public sealed class InspectorVisibleIfAttribute(string propertyName, bool expectedValue = true) : Attribute
{
    /// <summary>
    /// Name of the sibling property used as the visibility condition source.
    /// </summary>
    public string PropertyName { get; } = propertyName;

    /// <summary>
    /// Expected boolean value required for visibility.
    /// </summary>
    public bool ExpectedValue { get; } = expectedValue;
}

/// <summary>
/// Shows the annotated property only when another enum property has a named value.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
public sealed class InspectorVisibleIfEnumAttribute(string propertyName, string expectedMemberName) : Attribute
{
    /// <summary>
    /// Name of the sibling enum property used as the visibility condition source.
    /// </summary>
    public string PropertyName { get; } = propertyName;

    /// <summary>
    /// Required enum member name.
    /// </summary>
    public string ExpectedMemberName { get; } = expectedMemberName;
}

/// <summary>
/// Forces the inspector to show a readable value for the property without allowing edits.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class InspectorReadOnlyAttribute : Attribute;

/// <summary>
/// Renders enum properties with a searchable picker UI in the inspector.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class InspectorSearchableEnumAttribute : Attribute;

/// <summary>
/// Specifies min/max bounds and optional drag speed for numeric properties.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class InspectorRangeAttribute(float min, float max, float speed = 0.1f) : Attribute
{
    /// <summary>
    /// Minimum allowed value.
    /// </summary>
    public float Min { get; } = min;

    /// <summary>
    /// Maximum allowed value.
    /// </summary>
    public float Max { get; } = max;

    /// <summary>
    /// Drag speed multiplier for the inspector control.
    /// </summary>
    public float Speed { get; } = speed;
}

/// <summary>
/// Displays a tooltip when hovering over the property label.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class InspectorTooltipAttribute(string tooltip) : Attribute
{
    /// <summary>
    /// Tooltip text displayed on hover.
    /// </summary>
    public string Tooltip { get; } = tooltip;
}

/// <summary>
/// Inserts vertical spacing before the property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class InspectorSpaceAttribute(float height = 8f) : Attribute
{
    /// <summary>
    /// Spacing height in pixels.
    /// </summary>
    public float Height { get; } = height;
}

/// <summary>
/// Inserts a header label before the property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class InspectorHeaderAttribute(string title) : Attribute
{
    /// <summary>
    /// Header text.
    /// </summary>
    public string Title { get; } = title;
}

/// <summary>
/// Indents the property in the inspector.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class InspectorIndentAttribute(int levels = 1) : Attribute
{
    /// <summary>
    /// Number of indentation levels.
    /// </summary>
    public int Levels { get; } = levels;
}
