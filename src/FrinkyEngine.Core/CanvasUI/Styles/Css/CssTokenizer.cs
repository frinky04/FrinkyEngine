using System.Text;

namespace FrinkyEngine.Core.CanvasUI.Styles.Css;

internal static class CssTokenizer
{
    public static List<CssToken> Tokenize(string css)
    {
        var tokens = new List<CssToken>();
        int i = 0;
        int line = 1;
        int col = 1;

        while (i < css.Length)
        {
            char c = css[i];

            // Skip comments
            if (c == '/' && i + 1 < css.Length && css[i + 1] == '*')
            {
                i += 2; col += 2;
                while (i + 1 < css.Length && !(css[i] == '*' && css[i + 1] == '/'))
                {
                    if (css[i] == '\n') { line++; col = 1; } else { col++; }
                    i++;
                }
                if (i + 1 < css.Length) { i += 2; col += 2; } // skip */
                continue;
            }

            // Whitespace
            if (char.IsWhiteSpace(c))
            {
                int startCol = col;
                while (i < css.Length && char.IsWhiteSpace(css[i]))
                {
                    if (css[i] == '\n') { line++; col = 1; } else { col++; }
                    i++;
                }
                tokens.Add(new CssToken(CssTokenType.Whitespace, " ", line, startCol));
                continue;
            }

            // Single-char tokens
            if (c == '{') { tokens.Add(new CssToken(CssTokenType.OpenBrace, "{", line, col)); i++; col++; continue; }
            if (c == '}') { tokens.Add(new CssToken(CssTokenType.CloseBrace, "}", line, col)); i++; col++; continue; }
            if (c == ';') { tokens.Add(new CssToken(CssTokenType.Semicolon, ";", line, col)); i++; col++; continue; }
            if (c == ',') { tokens.Add(new CssToken(CssTokenType.Comma, ",", line, col)); i++; col++; continue; }
            if (c == '>') { tokens.Add(new CssToken(CssTokenType.GreaterThan, ">", line, col)); i++; col++; continue; }
            if (c == '*') { tokens.Add(new CssToken(CssTokenType.Star, "*", line, col)); i++; col++; continue; }
            if (c == '.') { tokens.Add(new CssToken(CssTokenType.Dot, ".", line, col)); i++; col++; continue; }
            if (c == ':') { tokens.Add(new CssToken(CssTokenType.Colon, ":", line, col)); i++; col++; continue; }

            // Hash
            if (c == '#')
            {
                int startCol = col;
                i++; col++;
                var sb = new StringBuilder();
                while (i < css.Length && IsNameChar(css[i]))
                {
                    sb.Append(css[i]); i++; col++;
                }
                tokens.Add(new CssToken(CssTokenType.Hash, sb.ToString(), line, startCol));
                continue;
            }

            // Strings
            if (c == '"' || c == '\'')
            {
                int startCol = col;
                char quote = c;
                i++; col++;
                var sb = new StringBuilder();
                while (i < css.Length && css[i] != quote)
                {
                    if (css[i] == '\\' && i + 1 < css.Length) { sb.Append(css[i + 1]); i += 2; col += 2; }
                    else { sb.Append(css[i]); i++; col++; }
                }
                if (i < css.Length) { i++; col++; } // skip closing quote
                tokens.Add(new CssToken(CssTokenType.String, sb.ToString(), line, startCol));
                continue;
            }

            // Numbers (including negative)
            if (char.IsDigit(c) || (c == '-' && i + 1 < css.Length && (char.IsDigit(css[i + 1]) || css[i + 1] == '.')))
            {
                int startCol = col;
                var sb = new StringBuilder();
                if (c == '-') { sb.Append(c); i++; col++; }
                while (i < css.Length && (char.IsDigit(css[i]) || css[i] == '.'))
                {
                    sb.Append(css[i]); i++; col++;
                }
                string numStr = sb.ToString();

                // Check for unit or %
                if (i < css.Length && css[i] == '%')
                {
                    i++; col++;
                    tokens.Add(new CssToken(CssTokenType.Percentage, numStr, line, startCol));
                }
                else if (i < css.Length && char.IsLetter(css[i]))
                {
                    var unit = new StringBuilder();
                    while (i < css.Length && char.IsLetter(css[i]))
                    {
                        unit.Append(css[i]); i++; col++;
                    }
                    tokens.Add(new CssToken(CssTokenType.Dimension, numStr + unit, line, startCol));
                }
                else
                {
                    tokens.Add(new CssToken(CssTokenType.Number, numStr, line, startCol));
                }
                continue;
            }

            // Ident or Function
            if (IsNameStart(c) || c == '-')
            {
                int startCol = col;
                var sb = new StringBuilder();
                while (i < css.Length && IsNameChar(css[i]))
                {
                    sb.Append(css[i]); i++; col++;
                }
                string ident = sb.ToString();

                if (i < css.Length && css[i] == '(')
                {
                    i++; col++;
                    tokens.Add(new CssToken(CssTokenType.Function, ident, line, startCol));
                }
                else
                {
                    tokens.Add(new CssToken(CssTokenType.Ident, ident, line, startCol));
                }
                continue;
            }

            // Skip unknown characters
            i++; col++;
        }

        tokens.Add(new CssToken(CssTokenType.EOF, "", line, col));
        return tokens;
    }

    private static bool IsNameStart(char c) => char.IsLetter(c) || c == '_';
    private static bool IsNameChar(char c) => char.IsLetterOrDigit(c) || c == '-' || c == '_';
}
