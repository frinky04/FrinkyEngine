namespace FrinkyEngine.Core.CanvasUI.Styles.Css;

internal static class CssSelectorMatcher
{
    public static bool Matches(Panel panel, CssSelector selector)
    {
        if (selector.Parts.Count == 0) return false;

        // Match right-to-left: last part must match the target panel
        int partIndex = selector.Parts.Count - 1;
        var currentPanel = panel;

        // The rightmost part must match the target panel
        if (!MatchesPart(currentPanel, selector.Parts[partIndex]))
            return false;

        partIndex--;

        // Walk backwards through selector parts, matching ancestors
        while (partIndex >= 0)
        {
            var part = selector.Parts[partIndex];
            var combinator = selector.Parts[partIndex + 1].Combinator;
            Panel? nextPanel;

            switch (combinator)
            {
                case CssCombinator.Child:
                    // Must match immediate parent
                    nextPanel = currentPanel.Parent;
                    if (nextPanel == null || !MatchesPart(nextPanel, part))
                        return false;
                    currentPanel = nextPanel;
                    break;

                case CssCombinator.Descendant:
                    // Walk up ancestors to find a match
                    bool found = false;
                    nextPanel = currentPanel.Parent;
                    while (nextPanel != null)
                    {
                        if (MatchesPart(nextPanel, part))
                        {
                            found = true;
                            break;
                        }
                        nextPanel = nextPanel.Parent;
                    }
                    if (!found) return false;
                    currentPanel = nextPanel!;
                    break;

                default:
                    return false;
            }

            partIndex--;
        }

        return true;
    }

    private static bool MatchesPart(Panel panel, CssSelectorPart part)
    {
        // Type check
        if (part.TypeName != null)
        {
            if (!panel.GetType().Name.Equals(part.TypeName, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // Class check
        foreach (var cls in part.ClassNames)
        {
            if (!panel.HasClass(cls))
                return false;
        }

        // Pseudo-class check
        foreach (var pseudo in part.PseudoClasses)
        {
            if (!MatchesPseudoClass(panel, pseudo))
                return false;
        }

        return true;
    }

    private static bool MatchesPseudoClass(Panel panel, string pseudo)
    {
        return pseudo.ToLowerInvariant() switch
        {
            "hover" => panel.PseudoClasses.HasFlag(PseudoClassFlags.Hover),
            "active" => panel.PseudoClasses.HasFlag(PseudoClassFlags.Active),
            "focus" => panel.PseudoClasses.HasFlag(PseudoClassFlags.Focus),
            "disabled" => panel.PseudoClasses.HasFlag(PseudoClassFlags.Disabled),
            _ => false,
        };
    }
}
