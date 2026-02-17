namespace FrinkyEngine.Core.CanvasUI.Styles.Css;

internal readonly struct CssSpecificity : IComparable<CssSpecificity>
{
    public int Ids { get; }
    public int Classes { get; }
    public int Types { get; }

    public CssSpecificity(int ids, int classes, int types)
    {
        Ids = ids;
        Classes = classes;
        Types = types;
    }

    public int CompareTo(CssSpecificity other)
    {
        int cmp = Ids.CompareTo(other.Ids);
        if (cmp != 0) return cmp;
        cmp = Classes.CompareTo(other.Classes);
        if (cmp != 0) return cmp;
        return Types.CompareTo(other.Types);
    }
}
