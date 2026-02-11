namespace FrinkyEngine.Core.ECS;

/// <summary>
/// Controls when an inspector extension (button/message) is visible.
/// </summary>
public enum InspectorUiMode
{
    /// <summary>
    /// Always show the extension.
    /// </summary>
    Always,

    /// <summary>
    /// Show only while the editor is in edit/simulate scene-editable mode.
    /// </summary>
    EditorOnly,

    /// <summary>
    /// Show only while the editor is in runtime mode (play/simulate).
    /// </summary>
    RuntimeOnly
}

/// <summary>
/// Severity used by inspector validation messages.
/// </summary>
public enum InspectorMessageSeverity
{
    /// <summary>
    /// Informational message.
    /// </summary>
    Info,

    /// <summary>
    /// Warning message.
    /// </summary>
    Warning,

    /// <summary>
    /// Error message.
    /// </summary>
    Error
}

/// <summary>
/// Visual style for <see cref="System.Numerics.Vector3"/> properties in the inspector.
/// </summary>
public enum InspectorVector3Style
{
    /// <summary>
    /// Default drag-float control.
    /// </summary>
    Default,

    /// <summary>
    /// Colored XYZ fields with per-axis reset buttons.
    /// </summary>
    ColoredAxisReset
}

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
public sealed class InspectorVisibleIfAttribute(string memberName, bool expectedValue = true) : Attribute
{
    /// <summary>
    /// Name of the sibling bool member (property/field/method) used as the visibility condition source.
    /// </summary>
    public string PropertyName { get; } = memberName;

    /// <summary>
    /// Expected boolean value required for visibility.
    /// </summary>
    public bool ExpectedValue { get; } = expectedValue;
}

/// <summary>
/// Shows the annotated property only when another enum property has a named value.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
public sealed class InspectorVisibleIfEnumAttribute(string memberName, string expectedMemberName) : Attribute
{
    /// <summary>
    /// Name of the sibling enum member (property/field/method) used as the visibility condition source.
    /// </summary>
    public string PropertyName { get; } = memberName;

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
/// Hides a public property from the inspector while keeping it serialized.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class InspectorHiddenAttribute : Attribute;

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

/// <summary>
/// Renders a method as a clickable button in the inspector.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public sealed class InspectorButtonAttribute(string label) : Attribute
{
    /// <summary>
    /// Button label text.
    /// </summary>
    public string Label { get; } = label;

    /// <summary>
    /// Optional section heading shown before the button group.
    /// </summary>
    public string? Section { get; init; }

    /// <summary>
    /// Controls when this button is visible.
    /// </summary>
    public InspectorUiMode Mode { get; init; } = InspectorUiMode.Always;

    /// <summary>
    /// Optional member name returning bool that disables the button when true.
    /// </summary>
    public string? DisableWhen { get; init; }

    /// <summary>
    /// Sort key for multiple buttons.
    /// </summary>
    public int Order { get; init; }
}

/// <summary>
/// Displays a validation/info message when a named bool condition is true.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public sealed class InspectorMessageIfAttribute(string conditionMember, string message) : Attribute
{
    /// <summary>
    /// Name of a bool property/field/method on the inspected object.
    /// </summary>
    public string ConditionMember { get; } = conditionMember;

    /// <summary>
    /// Message text shown in the inspector.
    /// </summary>
    public string Message { get; } = message;

    /// <summary>
    /// Visual severity style.
    /// </summary>
    public InspectorMessageSeverity Severity { get; init; } = InspectorMessageSeverity.Warning;

    /// <summary>
    /// Controls when this message is visible.
    /// </summary>
    public InspectorUiMode Mode { get; init; } = InspectorUiMode.Always;

    /// <summary>
    /// Sort key for multiple messages.
    /// </summary>
    public int Order { get; init; }
}

/// <summary>
/// Calls one or more methods after the property value changes in the inspector.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
public sealed class InspectorOnChangedAttribute(string methodName) : Attribute
{
    /// <summary>
    /// Name of a parameterless instance method to invoke.
    /// </summary>
    public string MethodName { get; } = methodName;
}

/// <summary>
/// Configures how a <see cref="System.Numerics.Vector3"/> property is drawn.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class InspectorVector3StyleAttribute(
    InspectorVector3Style style = InspectorVector3Style.Default,
    float resetX = 0f,
    float resetY = 0f,
    float resetZ = 0f) : Attribute
{
    /// <summary>
    /// Rendering style.
    /// </summary>
    public InspectorVector3Style Style { get; } = style;

    /// <summary>
    /// Reset value for the X axis when using reset-style controls.
    /// </summary>
    public float ResetX { get; } = resetX;

    /// <summary>
    /// Reset value for the Y axis when using reset-style controls.
    /// </summary>
    public float ResetY { get; } = resetY;

    /// <summary>
    /// Reset value for the Z axis when using reset-style controls.
    /// </summary>
    public float ResetZ { get; } = resetZ;
}

/// <summary>
/// Declares a factory method used when adding new elements to a reflected list.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class InspectorListFactoryAttribute(string methodName) : Attribute
{
    /// <summary>
    /// Name of a parameterless instance method used to create list elements.
    /// </summary>
    public string MethodName { get; } = methodName;
}
