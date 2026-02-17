using Facebook.Yoga;
using FrinkyEngine.Core.CanvasUI.Styles;

namespace FrinkyEngine.Core.CanvasUI.Layout;

internal class YogaLayoutEngine
{
    public void Calculate(RootPanel root, int screenWidth, int screenHeight)
    {
        SyncStylesRecursive(root);
        root.YogaNode.Width = YogaValue.Point(screenWidth);
        root.YogaNode.Height = YogaValue.Point(screenHeight);
        root.YogaNode.CalculateLayout(screenWidth, screenHeight);
    }

    private static void SyncStylesRecursive(Panel panel)
    {
        SyncToYoga(panel);

        // Yoga 1.18 has no gap property — simulate via margin on children.
        // Apply after each child is synced so we don't overwrite their own margins.
        float gap = panel.ComputedStyle.Gap;
        bool hasGap = gap > 0 && panel.Children.Count > 1;
        bool isRow = panel.ComputedStyle.FlexDirection is FlexDirection.Row or FlexDirection.RowReverse;

        for (int i = 0; i < panel.Children.Count; i++)
        {
            var child = panel.Children[i];
            SyncStylesRecursive(child);

            if (hasGap && i > 0)
            {
                if (isRow)
                {
                    var unit = child.ComputedStyle.Margin.Left.Unit;
                    if (unit is LengthUnit.Pixels or LengthUnit.Auto)
                        child.YogaNode.MarginLeft = YogaValue.Point(
                            (unit == LengthUnit.Auto ? 0f : child.ComputedStyle.Margin.Left.Value) + gap);
                }
                else
                {
                    var unit = child.ComputedStyle.Margin.Top.Unit;
                    if (unit is LengthUnit.Pixels or LengthUnit.Auto)
                        child.YogaNode.MarginTop = YogaValue.Point(
                            (unit == LengthUnit.Auto ? 0f : child.ComputedStyle.Margin.Top.Value) + gap);
                }
            }
        }
    }

    private static void SyncToYoga(Panel panel)
    {
        var node = panel.YogaNode;
        var s = panel.ComputedStyle;

        node.FlexDirection = s.FlexDirection switch
        {
            FlexDirection.Row => YogaFlexDirection.Row,
            FlexDirection.RowReverse => YogaFlexDirection.RowReverse,
            FlexDirection.Column => YogaFlexDirection.Column,
            FlexDirection.ColumnReverse => YogaFlexDirection.ColumnReverse,
            _ => YogaFlexDirection.Column,
        };

        node.JustifyContent = s.JustifyContent switch
        {
            JustifyContent.FlexStart => YogaJustify.FlexStart,
            JustifyContent.Center => YogaJustify.Center,
            JustifyContent.FlexEnd => YogaJustify.FlexEnd,
            JustifyContent.SpaceBetween => YogaJustify.SpaceBetween,
            JustifyContent.SpaceAround => YogaJustify.SpaceAround,
            JustifyContent.SpaceEvenly => YogaJustify.SpaceEvenly,
            _ => YogaJustify.FlexStart,
        };

        node.AlignItems = MapAlign(s.AlignItems);
        node.AlignSelf = MapAlign(s.AlignSelf);

        node.Display = s.Display == Display.None ? YogaDisplay.None : YogaDisplay.Flex;
        node.Overflow = s.Overflow switch
        {
            Overflow.Hidden => YogaOverflow.Hidden,
            Overflow.Scroll => YogaOverflow.Scroll,
            _ => YogaOverflow.Visible,
        };

        node.PositionType = s.Position == PositionMode.Absolute
            ? YogaPositionType.Absolute
            : YogaPositionType.Relative;

        node.Width = s.Width.ToYoga();
        node.Height = s.Height.ToYoga();
        node.MinWidth = s.MinWidth.ToYogaMinMax();
        node.MinHeight = s.MinHeight.ToYogaMinMax();
        node.MaxWidth = s.MaxWidth.ToYogaMinMax();
        node.MaxHeight = s.MaxHeight.ToYogaMinMax();

        node.FlexGrow = s.FlexGrow;
        node.FlexShrink = s.FlexShrink;
        node.FlexBasis = s.FlexBasis.ToYoga();

        // Padding
        node.PaddingTop = s.Padding.Top.ToYoga();
        node.PaddingRight = s.Padding.Right.ToYoga();
        node.PaddingBottom = s.Padding.Bottom.ToYoga();
        node.PaddingLeft = s.Padding.Left.ToYoga();

        // Margin
        node.MarginTop = s.Margin.Top.ToYoga();
        node.MarginRight = s.Margin.Right.ToYoga();
        node.MarginBottom = s.Margin.Bottom.ToYoga();
        node.MarginLeft = s.Margin.Left.ToYoga();

        // Border
        node.BorderWidth = s.BorderWidth;

        // Absolute positioning
        if (s.Position == PositionMode.Absolute)
        {
            if (s.Left.Unit != LengthUnit.Auto) node.Left = s.Left.ToYoga();
            if (s.Top.Unit != LengthUnit.Auto) node.Top = s.Top.ToYoga();
            if (s.Right.Unit != LengthUnit.Auto) node.Right = s.Right.ToYoga();
            if (s.Bottom.Unit != LengthUnit.Auto) node.Bottom = s.Bottom.ToYoga();
        }
    }

    private static YogaAlign MapAlign(AlignItems align)
    {
        return align switch
        {
            AlignItems.Auto => YogaAlign.Auto,
            AlignItems.FlexStart => YogaAlign.FlexStart,
            AlignItems.Center => YogaAlign.Center,
            AlignItems.FlexEnd => YogaAlign.FlexEnd,
            AlignItems.Stretch => YogaAlign.Stretch,
            AlignItems.Baseline => YogaAlign.Baseline,
            _ => YogaAlign.Stretch,
        };
    }
}
