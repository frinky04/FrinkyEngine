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
