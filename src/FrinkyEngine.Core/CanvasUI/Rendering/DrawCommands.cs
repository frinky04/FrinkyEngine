using System.Numerics;
using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Rendering;

internal static class DrawCommands
{
    private const int MaxMeasureCacheEntries = 4096;
    private static readonly Dictionary<MeasureTextCacheKey, Vector2> MeasureCache = new();

    public static void RoundedRect(float x, float y, float w, float h, float radius, Color color)
    {
        if (color.A == 0 || w <= 0 || h <= 0) return;
        if (radius <= 0)
        {
            Raylib.DrawRectangleRec(new Rectangle(x, y, w, h), color);
            return;
        }
        var rect = new Rectangle(x, y, w, h);
        float roundness = MathF.Min(radius / MathF.Min(w, h) * 2f, 1f);
        Raylib.DrawRectangleRounded(rect, roundness, 8, color);
    }

    public static void RectBorder(float x, float y, float w, float h, float radius, float thickness, Color color)
    {
        if (color.A == 0 || thickness <= 0 || w <= 0 || h <= 0) return;
        var rect = new Rectangle(x, y, w, h);
        if (radius <= 0)
        {
            Raylib.DrawRectangleLinesEx(rect, thickness, color);
            return;
        }
        float roundness = MathF.Min(radius / MathF.Min(w, h) * 2f, 1f);
        Raylib.DrawRectangleRoundedLinesEx(rect, roundness, 8, thickness, color);
    }

    public static void Text(string text, float x, float y, float fontSize, Color color, Font font)
    {
        if (string.IsNullOrEmpty(text) || color.A == 0) return;
        Raylib.DrawTextEx(font, text, new Vector2(x, y), fontSize, 1f, color);
    }

    public static void TexturedRect(Texture2D texture, float x, float y, float w, float h, Color tint)
    {
        if (w <= 0 || h <= 0) return;
        var source = new Rectangle(0, 0, texture.Width, texture.Height);
        var dest = new Rectangle(x, y, w, h);
        Raylib.DrawTexturePro(texture, source, dest, Vector2.Zero, 0f, tint);
    }

    public static Vector2 MeasureText(string text, float fontSize, Font font)
    {
        if (string.IsNullOrEmpty(text)) return Vector2.Zero;

        int quantizedSize = (int)MathF.Round(fontSize * 1000f);
        var key = new MeasureTextCacheKey(font.Texture.Id, font.BaseSize, quantizedSize, text);
        if (MeasureCache.TryGetValue(key, out var size))
            return size;

        size = Raylib.MeasureTextEx(font, text, fontSize, 1f);
        if (MeasureCache.Count >= MaxMeasureCacheEntries)
            MeasureCache.Clear();
        MeasureCache[key] = size;
        return size;
    }

    public static void ClearMeasureCache()
    {
        MeasureCache.Clear();
    }

    private readonly record struct MeasureTextCacheKey(
        uint FontTextureId,
        int FontBaseSize,
        int QuantizedFontSize,
        string Text);
}
