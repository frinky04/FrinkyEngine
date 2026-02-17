using System.Globalization;
using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Styles.Css;

internal static class CssColorNames
{
    private static readonly Dictionary<string, Color> NamedColors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["transparent"] = new Color(0, 0, 0, 0),
        ["black"] = new Color(0, 0, 0, 255),
        ["white"] = new Color(255, 255, 255, 255),
        ["red"] = new Color(255, 0, 0, 255),
        ["green"] = new Color(0, 128, 0, 255),
        ["blue"] = new Color(0, 0, 255, 255),
        ["yellow"] = new Color(255, 255, 0, 255),
        ["cyan"] = new Color(0, 255, 255, 255),
        ["magenta"] = new Color(255, 0, 255, 255),
        ["orange"] = new Color(255, 165, 0, 255),
        ["purple"] = new Color(128, 0, 128, 255),
        ["pink"] = new Color(255, 192, 203, 255),
        ["gray"] = new Color(128, 128, 128, 255),
        ["grey"] = new Color(128, 128, 128, 255),
        ["silver"] = new Color(192, 192, 192, 255),
        ["navy"] = new Color(0, 0, 128, 255),
        ["teal"] = new Color(0, 128, 128, 255),
        ["maroon"] = new Color(128, 0, 0, 255),
        ["olive"] = new Color(128, 128, 0, 255),
        ["lime"] = new Color(0, 255, 0, 255),
        ["aqua"] = new Color(0, 255, 255, 255),
        ["fuchsia"] = new Color(255, 0, 255, 255),
        ["darkgray"] = new Color(169, 169, 169, 255),
        ["darkgrey"] = new Color(169, 169, 169, 255),
        ["lightgray"] = new Color(211, 211, 211, 255),
        ["lightgrey"] = new Color(211, 211, 211, 255),
        ["darkred"] = new Color(139, 0, 0, 255),
        ["darkgreen"] = new Color(0, 100, 0, 255),
        ["darkblue"] = new Color(0, 0, 139, 255),
        ["cornflowerblue"] = new Color(100, 149, 237, 255),
        ["dodgerblue"] = new Color(30, 144, 255, 255),
        ["tomato"] = new Color(255, 99, 71, 255),
        ["coral"] = new Color(255, 127, 80, 255),
        ["gold"] = new Color(255, 215, 0, 255),
        ["indianred"] = new Color(205, 92, 92, 255),
        ["khaki"] = new Color(240, 230, 140, 255),
        ["slategray"] = new Color(112, 128, 144, 255),
        ["slategrey"] = new Color(112, 128, 144, 255),
        ["steelblue"] = new Color(70, 130, 180, 255),
        ["whitesmoke"] = new Color(245, 245, 245, 255),
    };

    public static bool TryParse(string value, out Color color)
        => NamedColors.TryGetValue(value, out color);

    public static bool TryParseHex(string hex, out Color color)
    {
        color = default;

        switch (hex.Length)
        {
            case 3: // #rgb
                if (byte.TryParse(new string(hex[0], 2), NumberStyles.HexNumber, null, out byte r3) &&
                    byte.TryParse(new string(hex[1], 2), NumberStyles.HexNumber, null, out byte g3) &&
                    byte.TryParse(new string(hex[2], 2), NumberStyles.HexNumber, null, out byte b3))
                {
                    color = new Color(r3, g3, b3, (byte)255);
                    return true;
                }
                return false;

            case 4: // #rgba
                if (byte.TryParse(new string(hex[0], 2), NumberStyles.HexNumber, null, out byte r4) &&
                    byte.TryParse(new string(hex[1], 2), NumberStyles.HexNumber, null, out byte g4) &&
                    byte.TryParse(new string(hex[2], 2), NumberStyles.HexNumber, null, out byte b4) &&
                    byte.TryParse(new string(hex[3], 2), NumberStyles.HexNumber, null, out byte a4))
                {
                    color = new Color(r4, g4, b4, a4);
                    return true;
                }
                return false;

            case 6: // #rrggbb
                if (byte.TryParse(hex[..2], NumberStyles.HexNumber, null, out byte r6) &&
                    byte.TryParse(hex[2..4], NumberStyles.HexNumber, null, out byte g6) &&
                    byte.TryParse(hex[4..6], NumberStyles.HexNumber, null, out byte b6))
                {
                    color = new Color(r6, g6, b6, (byte)255);
                    return true;
                }
                return false;

            case 8: // #rrggbbaa
                if (byte.TryParse(hex[..2], NumberStyles.HexNumber, null, out byte r8) &&
                    byte.TryParse(hex[2..4], NumberStyles.HexNumber, null, out byte g8) &&
                    byte.TryParse(hex[4..6], NumberStyles.HexNumber, null, out byte b8) &&
                    byte.TryParse(hex[6..8], NumberStyles.HexNumber, null, out byte a8))
                {
                    color = new Color(r8, g8, b8, a8);
                    return true;
                }
                return false;
        }

        return false;
    }

    public static bool TryParseRgbFunction(string funcName, List<string> args, out Color color)
    {
        color = default;
        bool hasAlpha = funcName.Equals("rgba", StringComparison.OrdinalIgnoreCase);

        if (args.Count < 3) return false;

        if (!byte.TryParse(args[0].Trim(), out byte r)) return false;
        if (!byte.TryParse(args[1].Trim(), out byte g)) return false;
        if (!byte.TryParse(args[2].Trim(), out byte b)) return false;

        byte a = 255;
        if (hasAlpha && args.Count >= 4)
        {
            string alphaStr = args[3].Trim();
            if (float.TryParse(alphaStr, CultureInfo.InvariantCulture, out float af))
            {
                a = (byte)(Math.Clamp(af, 0f, 1f) * 255);
            }
        }

        color = new Color(r, g, b, a);
        return true;
    }
}
