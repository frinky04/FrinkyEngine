using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Styles;

public class StyleSheet
{
    // Layout
    public FlexDirection? FlexDirection { get; set; }
    public JustifyContent? JustifyContent { get; set; }
    public AlignItems? AlignItems { get; set; }
    public AlignItems? AlignSelf { get; set; }
    public Display? Display { get; set; }
    public PositionMode? Position { get; set; }
    public Overflow? Overflow { get; set; }

    public Length? Width { get; set; }
    public Length? Height { get; set; }
    public Length? MinWidth { get; set; }
    public Length? MinHeight { get; set; }
    public Length? MaxWidth { get; set; }
    public Length? MaxHeight { get; set; }

    public float? FlexGrow { get; set; }
    public float? FlexShrink { get; set; }
    public Length? FlexBasis { get; set; }
    public float? Gap { get; set; }

    public Edges? Padding { get; set; }
    public Edges? Margin { get; set; }

    // Positioning (for Position = Absolute)
    public Length? Left { get; set; }
    public Length? Top { get; set; }
    public Length? Right { get; set; }
    public Length? Bottom { get; set; }

    // Visual
    public Color? BackgroundColor { get; set; }
    public Color? Color { get; set; }
    public Color? BorderColor { get; set; }
    public float? BorderWidth { get; set; }
    public float? BorderRadius { get; set; }
    public float? FontSize { get; set; }
    public string? FontFamily { get; set; }
    public float? Opacity { get; set; }
    public TextAlign? TextAlign { get; set; }
}
