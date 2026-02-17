using System.Globalization;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.CanvasUI.Styles.Css;
using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Authoring;

internal static class CanvasValueConverter
{
    public static bool TryParseBindingExpression(string raw, out string propertyName)
    {
        propertyName = string.Empty;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        var trimmed = raw.Trim();
        if (!trimmed.StartsWith('{') || !trimmed.EndsWith('}') || trimmed.Length < 3)
            return false;

        propertyName = trimmed[1..^1].Trim();
        return propertyName.Length > 0;
    }

    public static bool TryConvertValue(object? sourceValue, Type targetType, out object? converted)
    {
        converted = null;

        var nullableUnderlying = Nullable.GetUnderlyingType(targetType);
        var effectiveTarget = nullableUnderlying ?? targetType;

        if (sourceValue == null)
        {
            if (!targetType.IsValueType || nullableUnderlying != null)
                return true;

            converted = Activator.CreateInstance(targetType);
            return true;
        }

        var sourceType = sourceValue.GetType();
        if (effectiveTarget.IsAssignableFrom(sourceType))
        {
            converted = sourceValue;
            return true;
        }

        try
        {
            if (effectiveTarget == typeof(string))
            {
                converted = sourceValue.ToString();
                return true;
            }

            if (effectiveTarget == typeof(bool))
            {
                converted = Convert.ToBoolean(sourceValue, CultureInfo.InvariantCulture);
                return true;
            }

            if (effectiveTarget == typeof(int))
            {
                converted = Convert.ToInt32(sourceValue, CultureInfo.InvariantCulture);
                return true;
            }

            if (effectiveTarget == typeof(float))
            {
                converted = Convert.ToSingle(sourceValue, CultureInfo.InvariantCulture);
                return true;
            }

            if (effectiveTarget == typeof(double))
            {
                converted = Convert.ToDouble(sourceValue, CultureInfo.InvariantCulture);
                return true;
            }

            if (effectiveTarget.IsEnum)
            {
                if (sourceValue is string s &&
                    Enum.TryParse(effectiveTarget, NormalizeCssIdent(s), true, out object? enumValue))
                {
                    converted = enumValue;
                    return true;
                }
                return false;
            }

            if (effectiveTarget == typeof(Color))
            {
                if (sourceValue is string colorText && TryParseColor(colorText, out var color))
                {
                    converted = color;
                    return true;
                }
                return false;
            }

            if (effectiveTarget == typeof(Texture2D))
            {
                if (sourceValue is string texturePath)
                {
                    converted = AssetManager.Instance.LoadTexture(texturePath);
                    return true;
                }
                return false;
            }

            converted = Convert.ChangeType(sourceValue, effectiveTarget, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryParseColor(string text, out Color color)
    {
        text = text.Trim();

        if (text.StartsWith('#'))
            return CssColorNames.TryParseHex(text[1..], out color);

        if (CssColorNames.TryParse(text, out color))
            return true;

        if (text.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase) ||
            text.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase))
        {
            int open = text.IndexOf('(');
            int close = text.LastIndexOf(')');
            if (open < 0 || close <= open)
                return false;
            string fn = text[..open];
            var args = text[(open + 1)..close]
                .Split(',')
                .Select(x => x.Trim())
                .ToList();
            return CssColorNames.TryParseRgbFunction(fn, args, out color);
        }

        return false;
    }

    public static string NormalizeCssIdent(string cssIdent)
    {
        var parts = cssIdent.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return cssIdent;
        return string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
    }
}
