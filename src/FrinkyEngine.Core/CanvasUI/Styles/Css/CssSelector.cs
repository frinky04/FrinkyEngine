namespace FrinkyEngine.Core.CanvasUI.Styles.Css;

internal enum CssCombinator
{
    None,       // first part in chain (no combinator)
    Descendant, // space
    Child,      // >
}

internal class CssSelectorPart
{
    public string? TypeName { get; set; }           // e.g. "Label", "Button", null for universal
    public bool IsUniversal { get; set; }           // *
    public List<string> ClassNames { get; } = new();
    public List<string> PseudoClasses { get; } = new();
    public CssCombinator Combinator { get; set; }
}

internal class CssSelector
{
    public List<CssSelectorPart> Parts { get; } = new();

    private CssSpecificity? _specificity;

    public CssSpecificity Specificity => _specificity ??= ComputeSpecificity();

    private CssSpecificity ComputeSpecificity()
    {
        int classes = 0;
        int types = 0;

        foreach (var part in Parts)
        {
            if (part.TypeName != null) types++;
            classes += part.ClassNames.Count;
            classes += part.PseudoClasses.Count;
        }

        return new CssSpecificity(0, classes, types);
    }
}
