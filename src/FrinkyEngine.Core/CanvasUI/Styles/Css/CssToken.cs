namespace FrinkyEngine.Core.CanvasUI.Styles.Css;

internal enum CssTokenType
{
    Ident,
    Hash,         // #abc
    Dot,          // .
    Colon,        // :
    OpenBrace,    // {
    CloseBrace,   // }
    Semicolon,    // ;
    Number,       // 12
    Percentage,   // 50%
    Dimension,    // 12px
    String,       // "hello" or 'hello'
    Comma,        // ,
    Whitespace,
    Function,     // rgb(
    GreaterThan,  // >
    Star,         // *
    EOF,
}

internal readonly struct CssToken
{
    public CssTokenType Type { get; }
    public string Value { get; }
    public int Line { get; }
    public int Column { get; }

    public CssToken(CssTokenType type, string value, int line, int column)
    {
        Type = type;
        Value = value;
        Line = line;
        Column = column;
    }

    public override string ToString() => $"{Type}({Value}) at {Line}:{Column}";
}
