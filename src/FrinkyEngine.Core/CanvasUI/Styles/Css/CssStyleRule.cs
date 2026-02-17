namespace FrinkyEngine.Core.CanvasUI.Styles.Css;

internal class CssStyleRule
{
    public CssSelector Selector { get; }
    public StyleSheet Declarations { get; }

    public CssStyleRule(CssSelector selector, StyleSheet declarations)
    {
        Selector = selector;
        Declarations = declarations;
    }
}
