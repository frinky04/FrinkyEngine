using FrinkyEngine.Core.CanvasUI.Styles;

namespace FrinkyEngine.Core.CanvasUI.Authoring;

internal static class CanvasStyleSheetMerge
{
    public static void MergeInto(StyleSheet target, StyleSheet source)
    {
        if (source.FlexDirection.HasValue) target.FlexDirection = source.FlexDirection;
        if (source.JustifyContent.HasValue) target.JustifyContent = source.JustifyContent;
        if (source.AlignItems.HasValue) target.AlignItems = source.AlignItems;
        if (source.AlignSelf.HasValue) target.AlignSelf = source.AlignSelf;
        if (source.Display.HasValue) target.Display = source.Display;
        if (source.Position.HasValue) target.Position = source.Position;
        if (source.Overflow.HasValue) target.Overflow = source.Overflow;

        if (source.Width.HasValue) target.Width = source.Width;
        if (source.Height.HasValue) target.Height = source.Height;
        if (source.MinWidth.HasValue) target.MinWidth = source.MinWidth;
        if (source.MinHeight.HasValue) target.MinHeight = source.MinHeight;
        if (source.MaxWidth.HasValue) target.MaxWidth = source.MaxWidth;
        if (source.MaxHeight.HasValue) target.MaxHeight = source.MaxHeight;

        if (source.FlexGrow.HasValue) target.FlexGrow = source.FlexGrow;
        if (source.FlexShrink.HasValue) target.FlexShrink = source.FlexShrink;
        if (source.FlexBasis.HasValue) target.FlexBasis = source.FlexBasis;
        if (source.Gap.HasValue) target.Gap = source.Gap;

        if (source.Padding.HasValue) target.Padding = source.Padding;
        if (source.Margin.HasValue) target.Margin = source.Margin;

        if (source.Left.HasValue) target.Left = source.Left;
        if (source.Top.HasValue) target.Top = source.Top;
        if (source.Right.HasValue) target.Right = source.Right;
        if (source.Bottom.HasValue) target.Bottom = source.Bottom;

        if (source.BackgroundColor.HasValue) target.BackgroundColor = source.BackgroundColor;
        if (source.Color.HasValue) target.Color = source.Color;
        if (source.BorderColor.HasValue) target.BorderColor = source.BorderColor;
        if (source.BorderWidth.HasValue) target.BorderWidth = source.BorderWidth;
        if (source.BorderRadius.HasValue) target.BorderRadius = source.BorderRadius;
        if (source.FontSize.HasValue) target.FontSize = source.FontSize;
        if (source.FontFamily != null) target.FontFamily = source.FontFamily;
        if (source.Opacity.HasValue) target.Opacity = source.Opacity;
        if (source.TextAlign.HasValue) target.TextAlign = source.TextAlign;
    }
}
