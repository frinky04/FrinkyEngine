using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Styles;

public struct ComputedStyle
{
    // Layout
    public FlexDirection FlexDirection;
    public JustifyContent JustifyContent;
    public AlignItems AlignItems;
    public AlignItems AlignSelf;
    public Display Display;
    public PositionMode Position;
    public Overflow Overflow;

    public Length Width;
    public Length Height;
    public Length MinWidth;
    public Length MinHeight;
    public Length MaxWidth;
    public Length MaxHeight;

    public float FlexGrow;
    public float FlexShrink;
    public Length FlexBasis;
    public float Gap;

    public Edges Padding;
    public Edges Margin;

    public Length Left;
    public Length Top;
    public Length Right;
    public Length Bottom;

    // Visual
    public Color BackgroundColor;
    public Color Color;
    public Color BorderColor;
    public float BorderWidth;
    public float BorderRadius;
    public float FontSize;
    public string? FontFamily;
    public float Opacity;
    public TextAlign TextAlign;

    public static ComputedStyle Default => new()
    {
        FlexDirection = Styles.FlexDirection.Column,
        JustifyContent = Styles.JustifyContent.FlexStart,
        AlignItems = Styles.AlignItems.Stretch,
        AlignSelf = Styles.AlignItems.Auto,
        Display = Styles.Display.Flex,
        Position = PositionMode.Relative,
        Overflow = Styles.Overflow.Visible,

        Width = Length.Auto,
        Height = Length.Auto,
        MinWidth = Length.Auto,
        MinHeight = Length.Auto,
        MaxWidth = Length.Auto,
        MaxHeight = Length.Auto,

        FlexGrow = 0f,
        FlexShrink = 1f,
        FlexBasis = Length.Auto,
        Gap = 0f,

        Left = Length.Auto,
        Top = Length.Auto,
        Right = Length.Auto,
        Bottom = Length.Auto,

        BackgroundColor = new Color(0, 0, 0, 0),
        Color = new Color(255, 255, 255, 255),
        BorderColor = new Color(0, 0, 0, 0),
        BorderWidth = 0f,
        BorderRadius = 0f,
        FontSize = 16f,
        Opacity = 1f,
        TextAlign = TextAlign.Left,
    };
}
