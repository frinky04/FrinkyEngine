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
            case "font-family": sheet.FontFamily = ParseString(tokens); break;
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
            case "text-align": sheet.TextAlign = ParseEnum<TextAlign>(tokens); break;

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

    private static string? ParseString(List<CssToken> tokens)
    {
        if (tokens.Count == 0) return null;
        var v = tokens[0].Value;
        // Strip surrounding quotes if present
        if (v.Length >= 2 &&
            ((v[0] == '"' && v[^1] == '"') || (v[0] == '\'' && v[^1] == '\'')))
            v = v[1..^1];
        return string.IsNullOrWhiteSpace(v) ? null : v;
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

    private static readonly Dictionary<Type, Dictionary<string, object>> EnumLookupCache = new();

    private static T? ParseEnum<T>(List<CssToken> tokens) where T : struct, Enum
    {
        if (tokens.Count == 0) return null;
        if (tokens[0].Type != CssTokenType.Ident) return null;

        var lookup = GetOrBuildEnumLookup(typeof(T));
        if (lookup.TryGetValue(tokens[0].Value, out var boxed))
            return (T)boxed;
        return null;
    }

    private static Dictionary<string, object> GetOrBuildEnumLookup(Type enumType)
    {
        if (EnumLookupCache.TryGetValue(enumType, out var existing))
            return existing;

        var map = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var val in Enum.GetValues(enumType))
        {
            var name = val.ToString()!;
            map[name] = val;
            // Also add kebab-case key: "FlexStart" → "flex-start"
            map[PascalToKebab(name)] = val;
        }

        EnumLookupCache[enumType] = map;
        return map;
    }

    private static string PascalToKebab(string pascal)
    {
        var sb = new System.Text.StringBuilder(pascal.Length + 4);
        for (int i = 0; i < pascal.Length; i++)
        {
            char c = pascal[i];
            if (char.IsUpper(c) && i > 0)
                sb.Append('-');
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
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
