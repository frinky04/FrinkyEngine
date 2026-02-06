namespace FrinkyEngine.Core.Rendering;

public readonly record struct ForwardPlusSettings(int TileSize, int MaxLights, int MaxLightsPerTile)
{
    public const int DefaultTileSize = 16;
    public const int DefaultMaxLights = 256;
    public const int DefaultMaxLightsPerTile = 64;

    public static ForwardPlusSettings Default => new(DefaultTileSize, DefaultMaxLights, DefaultMaxLightsPerTile);

    public ForwardPlusSettings Normalize()
    {
        var tileSize = Clamp(TileSize, 8, 64, DefaultTileSize);
        var maxLights = Clamp(MaxLights, 16, 2048, DefaultMaxLights);
        var maxLightsPerTile = Clamp(MaxLightsPerTile, 8, 256, DefaultMaxLightsPerTile);
        return new ForwardPlusSettings(tileSize, maxLights, maxLightsPerTile);
    }

    private static int Clamp(int value, int min, int max, int fallback)
    {
        if (value < min || value > max)
            return fallback;
        return value;
    }
}
