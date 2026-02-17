using FrinkyEngine.Core.CanvasUI.Styles.Css;

namespace FrinkyEngine.Core.CanvasUI.Styles;

internal static class StyleResolver
{
    /// <summary>
    /// Pre-sorts rules by (specificity, source order) so callers can apply them
    /// in a single pass without per-panel allocation.
    /// Call once when the rule list changes, not per frame.
    /// </summary>
    public static void SortRules(List<CssStyleRule> rules)
    {
        // Stable-sort preserves source order for equal specificity.
        // We tag each rule with its current index for tie-breaking.
        var indexed = new (CssStyleRule rule, int order)[rules.Count];
        for (int i = 0; i < rules.Count; i++)
            indexed[i] = (rules[i], i);

        Array.Sort(indexed, (a, b) =>
        {
            int cmp = a.rule.Selector.Specificity.CompareTo(b.rule.Selector.Specificity);
            return cmp != 0 ? cmp : a.order.CompareTo(b.order);
        });

        for (int i = 0; i < rules.Count; i++)
            rules[i] = indexed[i].rule;
    }

    public static ComputedStyle Resolve(Panel panel, IReadOnlyList<CssStyleRule> sortedRules, ComputedStyle? parent = null)
    {
        var computed = ComputedStyle.Default;

        // Seed inheritable properties from parent
        if (parent.HasValue)
        {
            computed.Color = parent.Value.Color;
            computed.FontSize = parent.Value.FontSize;
            computed.FontFamily = parent.Value.FontFamily;
            computed.TextAlign = parent.Value.TextAlign;
        }

        // Rules are already sorted by (specificity, source order) — apply matching ones in order.
        for (int i = 0; i < sortedRules.Count; i++)
        {
            var rule = sortedRules[i];
            if (CssSelectorMatcher.Matches(panel, rule.Selector))
                ApplySheet(ref computed, rule.Declarations);
        }

        // Apply inline styles last (highest priority)
        ApplySheet(ref computed, panel.Style);

        return computed;
    }

    private static void ApplySheet(ref ComputedStyle computed, StyleSheet s)
    {
        if (s.FlexDirection.HasValue) computed.FlexDirection = s.FlexDirection.Value;
        if (s.JustifyContent.HasValue) computed.JustifyContent = s.JustifyContent.Value;
        if (s.AlignItems.HasValue) computed.AlignItems = s.AlignItems.Value;
        if (s.AlignSelf.HasValue) computed.AlignSelf = s.AlignSelf.Value;
        if (s.Display.HasValue) computed.Display = s.Display.Value;
        if (s.Position.HasValue) computed.Position = s.Position.Value;
        if (s.Overflow.HasValue) computed.Overflow = s.Overflow.Value;

        if (s.Width.HasValue) computed.Width = s.Width.Value;
        if (s.Height.HasValue) computed.Height = s.Height.Value;
        if (s.MinWidth.HasValue) computed.MinWidth = s.MinWidth.Value;
        if (s.MinHeight.HasValue) computed.MinHeight = s.MinHeight.Value;
        if (s.MaxWidth.HasValue) computed.MaxWidth = s.MaxWidth.Value;
        if (s.MaxHeight.HasValue) computed.MaxHeight = s.MaxHeight.Value;

        if (s.FlexGrow.HasValue) computed.FlexGrow = s.FlexGrow.Value;
        if (s.FlexShrink.HasValue) computed.FlexShrink = s.FlexShrink.Value;
        if (s.FlexBasis.HasValue) computed.FlexBasis = s.FlexBasis.Value;
        if (s.Gap.HasValue) computed.Gap = s.Gap.Value;

        if (s.Padding.HasValue) computed.Padding = s.Padding.Value;
        if (s.Margin.HasValue) computed.Margin = s.Margin.Value;

        if (s.Left.HasValue) computed.Left = s.Left.Value;
        if (s.Top.HasValue) computed.Top = s.Top.Value;
        if (s.Right.HasValue) computed.Right = s.Right.Value;
        if (s.Bottom.HasValue) computed.Bottom = s.Bottom.Value;

        if (s.BackgroundColor.HasValue) computed.BackgroundColor = s.BackgroundColor.Value;
        if (s.Color.HasValue) computed.Color = s.Color.Value;
        if (s.BorderColor.HasValue) computed.BorderColor = s.BorderColor.Value;
        if (s.BorderWidth.HasValue) computed.BorderWidth = s.BorderWidth.Value;
        if (s.BorderRadius.HasValue) computed.BorderRadius = s.BorderRadius.Value;
        if (s.FontSize.HasValue) computed.FontSize = s.FontSize.Value;
        if (s.FontFamily != null) computed.FontFamily = s.FontFamily;
        if (s.Opacity.HasValue) computed.Opacity = s.Opacity.Value;
        if (s.TextAlign.HasValue) computed.TextAlign = s.TextAlign.Value;
    }
}
