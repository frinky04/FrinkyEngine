using System.Text.Json;

namespace FrinkyEngine.Core.Assets;

/// <summary>
/// Metadata embedded in an exported game package, describing the project and its runtime settings.
/// </summary>
public class ExportManifest
{
    /// <summary>
    /// Name of the project.
    /// </summary>
    public string ProjectName { get; set; } = "Untitled";

    /// <summary>
    /// Asset-relative path to the default scene.
    /// </summary>
    public string DefaultScene { get; set; } = string.Empty;

    /// <summary>
    /// File name of the game assembly DLL, if any.
    /// </summary>
    public string? GameAssembly { get; set; }

    /// <summary>
    /// Product name for the exported game.
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// Build version string.
    /// </summary>
    public string? BuildVersion { get; set; }

    /// <summary>
    /// Target frames per second.
    /// </summary>
    public int? TargetFps { get; set; }

    /// <summary>
    /// Whether vertical sync is enabled.
    /// </summary>
    public bool? VSync { get; set; }

    /// <summary>
    /// Window title bar text.
    /// </summary>
    public string? WindowTitle { get; set; }

    /// <summary>
    /// Initial window width in pixels.
    /// </summary>
    public int? WindowWidth { get; set; }

    /// <summary>
    /// Initial window height in pixels.
    /// </summary>
    public int? WindowHeight { get; set; }

    /// <summary>
    /// Whether the window is resizable.
    /// </summary>
    public bool? Resizable { get; set; }

    /// <summary>
    /// Whether the game starts in fullscreen.
    /// </summary>
    public bool? Fullscreen { get; set; }

    /// <summary>
    /// Whether the game window starts maximized.
    /// </summary>
    public bool? StartMaximized { get; set; }

    /// <summary>
    /// Forward+ tile size in pixels.
    /// </summary>
    public int? ForwardPlusTileSize { get; set; }

    /// <summary>
    /// Maximum total lights for the Forward+ renderer.
    /// </summary>
    public int? ForwardPlusMaxLights { get; set; }

    /// <summary>
    /// Maximum lights per tile for the Forward+ renderer.
    /// </summary>
    public int? ForwardPlusMaxLightsPerTile { get; set; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes this manifest to a JSON string.
    /// </summary>
    /// <returns>The JSON representation.</returns>
    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    /// <summary>
    /// Deserializes an export manifest from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>The deserialized manifest.</returns>
    public static ExportManifest FromJson(string json) =>
        JsonSerializer.Deserialize<ExportManifest>(json, JsonOptions)
        ?? throw new InvalidOperationException("Failed to deserialize export manifest.");
}
