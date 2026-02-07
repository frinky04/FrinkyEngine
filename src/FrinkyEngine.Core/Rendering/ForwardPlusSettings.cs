namespace FrinkyEngine.Core.Rendering;

/// <summary>
/// Configuration for the Forward+ tiled light culling system.
/// </summary>
/// <param name="TileSize">Screen-space tile size in pixels (8–64, default 16).</param>
/// <param name="MaxLights">Maximum number of lights processed per frame (16–2048, default 256).</param>
/// <param name="MaxLightsPerTile">Maximum lights assigned to any single tile (8–256, default 64).</param>
public readonly record struct ForwardPlusSettings(int TileSize, int MaxLights, int MaxLightsPerTile)
{
    /// <summary>
    /// Default tile size (16 pixels).
    /// </summary>
    public const int DefaultTileSize = 16;

    /// <summary>
    /// Default maximum lights per frame (256).
    /// </summary>
    public const int DefaultMaxLights = 256;

    /// <summary>
    /// Default maximum lights per tile (64).
    /// </summary>
    public const int DefaultMaxLightsPerTile = 64;

    /// <summary>
    /// Gets the default Forward+ settings.
    /// </summary>
    public static ForwardPlusSettings Default => new(DefaultTileSize, DefaultMaxLights, DefaultMaxLightsPerTile);

    /// <summary>
    /// Returns a copy with all values clamped to valid ranges, substituting defaults for out-of-range values.
    /// </summary>
    /// <returns>A normalized copy of these settings.</returns>
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
