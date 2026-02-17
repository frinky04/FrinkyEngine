namespace FrinkyEngine.Core.CanvasUI.Styles.Css;

internal static class CssParser
{
    public static List<CssStyleRule> Parse(string css)
    {
        var tokens = CssTokenizer.Tokenize(css);
        var rules = new List<CssStyleRule>();
        int pos = 0;

        SkipWhitespace(tokens, ref pos);

        while (pos < tokens.Count && tokens[pos].Type != CssTokenType.EOF)
        {
            SkipWhitespace(tokens, ref pos);
            if (pos >= tokens.Count || tokens[pos].Type == CssTokenType.EOF) break;

            // Parse selector list (comma-separated selectors)
            var selectors = ParseSelectorList(tokens, ref pos);

            SkipWhitespace(tokens, ref pos);

            // Expect {
            if (pos >= tokens.Count || tokens[pos].Type != CssTokenType.OpenBrace)
            {
                SkipToCloseBrace(tokens, ref pos);
                continue;
            }
            pos++; // skip {

            // Parse declarations
            var declarations = ParseDeclarations(tokens, ref pos);

            // Expect }
            if (pos < tokens.Count && tokens[pos].Type == CssTokenType.CloseBrace)
                pos++;

            // Create a rule for each selector
            foreach (var selector in selectors)
            {
                rules.Add(new CssStyleRule(selector, declarations));
            }
        }

        return rules;
    }

    private static List<CssSelector> ParseSelectorList(List<CssToken> tokens, ref int pos)
    {
        var selectors = new List<CssSelector>();

        var first = ParseSelector(tokens, ref pos);
        if (first != null) selectors.Add(first);

        while (pos < tokens.Count && tokens[pos].Type == CssTokenType.Comma)
        {
            pos++; // skip comma
            SkipWhitespace(tokens, ref pos);
            var next = ParseSelector(tokens, ref pos);
            if (next != null) selectors.Add(next);
        }

        return selectors;
    }

    private static CssSelector? ParseSelector(List<CssToken> tokens, ref int pos)
    {
        var selector = new CssSelector();

        var firstPart = ParseSelectorPart(tokens, ref pos);
        if (firstPart == null) return null;

        firstPart.Combinator = CssCombinator.None;
        selector.Parts.Add(firstPart);

        while (pos < tokens.Count)
        {
            var t = tokens[pos];
            if (t.Type == CssTokenType.OpenBrace || t.Type == CssTokenType.Comma || t.Type == CssTokenType.EOF)
                break;

            CssCombinator combinator;

            if (t.Type == CssTokenType.GreaterThan)
            {
                combinator = CssCombinator.Child;
                pos++;
                SkipWhitespace(tokens, ref pos);
            }
            else if (t.Type == CssTokenType.Whitespace)
            {
                SkipWhitespace(tokens, ref pos);
                // Check if next token is > (child combinator with spaces around it)
                if (pos < tokens.Count && tokens[pos].Type == CssTokenType.GreaterThan)
                {
                    combinator = CssCombinator.Child;
                    pos++;
                    SkipWhitespace(tokens, ref pos);
                }
                else if (pos < tokens.Count && IsSelectorStart(tokens[pos]))
                {
                    combinator = CssCombinator.Descendant;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }

            var nextPart = ParseSelectorPart(tokens, ref pos);
            if (nextPart == null) break;

            nextPart.Combinator = combinator;
            selector.Parts.Add(nextPart);
        }

        return selector;
    }

    private static CssSelectorPart? ParseSelectorPart(List<CssToken> tokens, ref int pos)
    {
        if (pos >= tokens.Count) return null;

        var part = new CssSelectorPart();
        bool hasSomething = false;

        // Type name or universal
        if (tokens[pos].Type == CssTokenType.Star)
        {
            part.IsUniversal = true;
            pos++;
            hasSomething = true;
        }
        else if (tokens[pos].Type == CssTokenType.Ident)
        {
            part.TypeName = tokens[pos].Value;
            pos++;
            hasSomething = true;
        }

        // Class names and pseudo-classes (can chain: .foo.bar:hover)
        while (pos < tokens.Count)
        {
            if (tokens[pos].Type == CssTokenType.Dot)
            {
                pos++; // skip .
                if (pos < tokens.Count && tokens[pos].Type == CssTokenType.Ident)
                {
                    part.ClassNames.Add(tokens[pos].Value);
                    pos++;
                    hasSomething = true;
                }
            }
            else if (tokens[pos].Type == CssTokenType.Colon)
            {
                pos++; // skip :
                if (pos < tokens.Count && tokens[pos].Type == CssTokenType.Ident)
                {
                    part.PseudoClasses.Add(tokens[pos].Value);
                    pos++;
                    hasSomething = true;
                }
            }
            else
            {
                break;
            }
        }

        return hasSomething ? part : null;
    }

    private static StyleSheet ParseDeclarations(List<CssToken> tokens, ref int pos)
    {
        var sheet = new StyleSheet();

        while (pos < tokens.Count && tokens[pos].Type != CssTokenType.CloseBrace && tokens[pos].Type != CssTokenType.EOF)
        {
            SkipWhitespace(tokens, ref pos);
            if (pos >= tokens.Count || tokens[pos].Type == CssTokenType.CloseBrace) break;

            // Property name
            if (tokens[pos].Type != CssTokenType.Ident)
            {
                pos++;
                continue;
            }

            string propName = tokens[pos].Value;
            pos++;

            SkipWhitespace(tokens, ref pos);

            // Expect colon
            if (pos >= tokens.Count || tokens[pos].Type != CssTokenType.Colon)
            {
                SkipToSemicolonOrBrace(tokens, ref pos);
                continue;
            }
            pos++; // skip :

            SkipWhitespace(tokens, ref pos);

            // Collect value tokens until ; or }
            var valueTokens = new List<CssToken>();
            while (pos < tokens.Count &&
                   tokens[pos].Type != CssTokenType.Semicolon &&
                   tokens[pos].Type != CssTokenType.CloseBrace &&
                   tokens[pos].Type != CssTokenType.EOF)
            {
                valueTokens.Add(tokens[pos]);
                pos++;
            }

            // Apply property
            if (valueTokens.Count > 0)
                CssPropertyMap.Apply(sheet, propName, valueTokens);

            // Skip semicolon
            if (pos < tokens.Count && tokens[pos].Type == CssTokenType.Semicolon)
                pos++;
        }

        return sheet;
    }

    private static bool IsSelectorStart(CssToken token) =>
        token.Type is CssTokenType.Ident or CssTokenType.Dot or CssTokenType.Colon
            or CssTokenType.Hash or CssTokenType.Star;

    private static void SkipWhitespace(List<CssToken> tokens, ref int pos)
    {
        while (pos < tokens.Count && tokens[pos].Type == CssTokenType.Whitespace)
            pos++;
    }

    private static void SkipToCloseBrace(List<CssToken> tokens, ref int pos)
    {
        while (pos < tokens.Count && tokens[pos].Type != CssTokenType.CloseBrace && tokens[pos].Type != CssTokenType.EOF)
            pos++;
        if (pos < tokens.Count && tokens[pos].Type == CssTokenType.CloseBrace)
            pos++;
    }

    private static void SkipToSemicolonOrBrace(List<CssToken> tokens, ref int pos)
    {
        while (pos < tokens.Count &&
               tokens[pos].Type != CssTokenType.Semicolon &&
               tokens[pos].Type != CssTokenType.CloseBrace &&
               tokens[pos].Type != CssTokenType.EOF)
            pos++;
        if (pos < tokens.Count && tokens[pos].Type == CssTokenType.Semicolon)
            pos++;
    }
}
