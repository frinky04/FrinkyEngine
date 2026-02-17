using System.Globalization;
using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Styles.Css;

internal static class CssPropertyMap
{
    public static void Apply(StyleSheet sheet, string property, List<CssToken> valueTokens)
    {
        // Strip whitespace tokens
        var tokens = valueTokens.Where(t => t.Type != CssTokenType.Whitespace).ToList();
        if (tokens.Count == 0) return;

        switch (property)
        {
            case "background-color": sheet.BackgroundColor = ParseColor(tokens); break;
            case "color": sheet.Color = ParseColor(tokens); break;
            case "border-color": sheet.BorderColor = ParseColor(tokens); break;

            case "opacity": sheet.Opacity = ParseFloat(tokens); break;
            case "font-size": sheet.FontSize = ParseFloatFromLength(tokens); break;
            case "border-width": sheet.BorderWidth = ParseFloatFromLength(tokens); break;
            case "border-radius": sheet.BorderRadius = ParseFloatFromLength(tokens); break;

            case "width": sheet.Width = ParseLength(tokens); break;
            case "height": sheet.Height = ParseLength(tokens); break;
            case "min-width": sheet.MinWidth = ParseLength(tokens); break;
            case "min-height": sheet.MinHeight = ParseLength(tokens); break;
            case "max-width": sheet.MaxWidth = ParseLength(tokens); break;
            case "max-height": sheet.MaxHeight = ParseLength(tokens); break;

            case "flex-grow": sheet.FlexGrow = ParseFloat(tokens); break;
            case "flex-shrink": sheet.FlexShrink = ParseFloat(tokens); break;
            case "flex-basis": sheet.FlexBasis = ParseLength(tokens); break;
            case "gap": sheet.Gap = ParseFloatFromLength(tokens); break;

            case "flex-direction": sheet.FlexDirection = ParseEnum<FlexDirection>(tokens); break;
            case "align-items": sheet.AlignItems = ParseEnum<AlignItems>(tokens); break;
            case "align-self": sheet.AlignSelf = ParseEnum<AlignItems>(tokens); break;
            case "justify-content": sheet.JustifyContent = ParseEnum<JustifyContent>(tokens); break;
            case "display": sheet.Display = ParseEnum<Display>(tokens); break;
            case "position": sheet.Position = ParseEnum<PositionMode>(tokens); break;
            case "overflow": sheet.Overflow = ParseEnum<Overflow>(tokens); break;

            case "top": sheet.Top = ParseLength(tokens); break;
            case "right": sheet.Right = ParseLength(tokens); break;
            case "bottom": sheet.Bottom = ParseLength(tokens); break;
            case "left": sheet.Left = ParseLength(tokens); break;

            // Shorthand: padding
            case "padding": ApplyEdgeShorthand(tokens, (e) => sheet.Padding = e); break;
            case "padding-top": sheet.Padding = SetEdgeSide(sheet.Padding, ParseLength(tokens), 0); break;
            case "padding-right": sheet.Padding = SetEdgeSide(sheet.Padding, ParseLength(tokens), 1); break;
            case "padding-bottom": sheet.Padding = SetEdgeSide(sheet.Padding, ParseLength(tokens), 2); break;
            case "padding-left": sheet.Padding = SetEdgeSide(sheet.Padding, ParseLength(tokens), 3); break;

            // Shorthand: margin
            case "margin": ApplyEdgeShorthand(tokens, (e) => sheet.Margin = e); break;
            case "margin-top": sheet.Margin = SetEdgeSide(sheet.Margin, ParseLength(tokens), 0); break;
            case "margin-right": sheet.Margin = SetEdgeSide(sheet.Margin, ParseLength(tokens), 1); break;
            case "margin-bottom": sheet.Margin = SetEdgeSide(sheet.Margin, ParseLength(tokens), 2); break;
            case "margin-left": sheet.Margin = SetEdgeSide(sheet.Margin, ParseLength(tokens), 3); break;

            // Shorthand: border
            case "border":
                // border: <width> <style> <color> — we ignore style, take width and color
                ParseBorderShorthand(sheet, tokens);
                break;
        }
    }

    private static Color? ParseColor(List<CssToken> tokens)
    {
        if (tokens.Count == 0) return null;

        var first = tokens[0];

        // Hash color: #rgb, #rrggbb, #rrggbbaa
        if (first.Type == CssTokenType.Hash)
        {
            if (CssColorNames.TryParseHex(first.Value, out var color))
                return color;
            return null;
        }

        // Named color
        if (first.Type == CssTokenType.Ident)
        {
            if (CssColorNames.TryParse(first.Value, out var color))
                return color;
            return null;
        }

        // rgb() / rgba()
        if (first.Type == CssTokenType.Function &&
            (first.Value.Equals("rgb", StringComparison.OrdinalIgnoreCase) ||
             first.Value.Equals("rgba", StringComparison.OrdinalIgnoreCase)))
        {
            var args = CollectFunctionArgs(tokens, 1);
            if (CssColorNames.TryParseRgbFunction(first.Value, args, out var color))
                return color;
        }

        return null;
    }

    private static List<string> CollectFunctionArgs(List<CssToken> tokens, int startIndex)
    {
        var args = new List<string>();
        var current = "";

        for (int i = startIndex; i < tokens.Count; i++)
        {
            var t = tokens[i];
            if (t.Type == CssTokenType.Comma)
            {
                args.Add(current.Trim());
                current = "";
            }
            else
            {
                current += t.Value;
            }
        }
        if (!string.IsNullOrWhiteSpace(current))
        {
            // Trim trailing ')'
            current = current.TrimEnd(')');
            args.Add(current.Trim());
        }

        return args;
    }

    private static float? ParseFloat(List<CssToken> tokens)
    {
        if (tokens.Count == 0) return null;
        var v = tokens[0].Value;
        if (float.TryParse(v, CultureInfo.InvariantCulture, out float f))
            return f;
        return null;
    }

    private static string StripUnit(string dimensionValue)
    {
        for (int i = 0; i < dimensionValue.Length; i++)
        {
            if (char.IsLetter(dimensionValue[i]))
                return dimensionValue[..i];
        }
        return dimensionValue;
    }

    private static float? ParseFloatFromLength(List<CssToken> tokens)
    {
        if (tokens.Count == 0) return null;
        var t = tokens[0];
        string numStr = t.Type == CssTokenType.Dimension ? StripUnit(t.Value) : t.Value;
        return float.TryParse(numStr, CultureInfo.InvariantCulture, out float f) ? f : null;
    }

    private static Length? ParseLength(List<CssToken> tokens)
    {
        if (tokens.Count == 0) return null;
        var t = tokens[0];

        if (t.Type == CssTokenType.Ident && t.Value.Equals("auto", StringComparison.OrdinalIgnoreCase))
            return Length.Auto;

        if (t.Type == CssTokenType.Number &&
            float.TryParse(t.Value, CultureInfo.InvariantCulture, out float px))
            return Length.Px(px);

        if (t.Type == CssTokenType.Percentage &&
            float.TryParse(t.Value, CultureInfo.InvariantCulture, out float pct))
            return Length.Pct(pct);

        if (t.Type == CssTokenType.Dimension &&
            float.TryParse(StripUnit(t.Value), CultureInfo.InvariantCulture, out float dim))
            return Length.Px(dim);

        return null;
    }

    private static T? ParseEnum<T>(List<CssToken> tokens) where T : struct, Enum
    {
        if (tokens.Count == 0) return null;
        string value = NormalizeCssIdent(tokens[0].Value);

        foreach (var enumVal in Enum.GetValues<T>())
        {
            if (enumVal.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                return enumVal;
        }
        return null;
    }

    private static string NormalizeCssIdent(string cssIdent)
    {
        // Convert kebab-case to PascalCase: "flex-start" -> "FlexStart"
        var parts = cssIdent.Split('-');
        return string.Concat(parts.Select(p =>
            p.Length > 0 ? char.ToUpper(p[0]) + p[1..] : ""));
    }

    private static void ApplyEdgeShorthand(List<CssToken> tokens, Action<Edges> setter)
    {
        var lengths = new List<Length>();
        foreach (var t in tokens)
        {
            var singleList = new List<CssToken> { t };
            var len = ParseLength(singleList);
            if (len.HasValue) lengths.Add(len.Value);
        }

        if (lengths.Count == 0) return;

        Edges edges;
        switch (lengths.Count)
        {
            case 1:
                edges = new Edges(lengths[0], lengths[0], lengths[0], lengths[0]);
                break;
            case 2:
                edges = new Edges(lengths[0], lengths[1], lengths[0], lengths[1]);
                break;
            case 3:
                edges = new Edges(lengths[0], lengths[1], lengths[2], lengths[1]);
                break;
            default:
                edges = new Edges(lengths[0], lengths[1], lengths[2], lengths[3]);
                break;
        }
        setter(edges);
    }

    private static void ParseBorderShorthand(StyleSheet sheet, List<CssToken> tokens)
    {
        foreach (var t in tokens)
        {
            if (t.Type == CssTokenType.Number || t.Type == CssTokenType.Dimension)
            {
                var singleList = new List<CssToken> { t };
                var width = ParseFloatFromLength(singleList);
                if (width.HasValue) sheet.BorderWidth = width.Value;
            }
            else if (t.Type == CssTokenType.Hash)
            {
                if (CssColorNames.TryParseHex(t.Value, out var c))
                    sheet.BorderColor = c;
            }
            else if (t.Type == CssTokenType.Ident)
            {
                if (CssColorNames.TryParse(t.Value, out var c))
                    sheet.BorderColor = c;
                // skip style keywords like "solid", "none" etc.
            }
        }
    }

    // side: 0=top, 1=right, 2=bottom, 3=left
    private static Edges SetEdgeSide(Edges? existing, Length? val, int side)
    {
        if (!val.HasValue) return existing ?? new Edges();
        var e = existing ?? new Edges();
        switch (side)
        {
            case 0: e.Top = val.Value; break;
            case 1: e.Right = val.Value; break;
            case 2: e.Bottom = val.Value; break;
            case 3: e.Left = val.Value; break;
        }
        return e;
    }
}
