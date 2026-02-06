using System.Text.Json;

namespace FrinkyEngine.Core.Assets;

public class ExportManifest
{
    public string ProjectName { get; set; } = "Untitled";
    public string DefaultScene { get; set; } = string.Empty;
    public string? GameAssembly { get; set; }
    public string? ProductName { get; set; }
    public string? BuildVersion { get; set; }
    public int? TargetFps { get; set; }
    public bool? VSync { get; set; }
    public string? WindowTitle { get; set; }
    public int? WindowWidth { get; set; }
    public int? WindowHeight { get; set; }
    public bool? Resizable { get; set; }
    public bool? Fullscreen { get; set; }
    public bool? StartMaximized { get; set; }
    public int? ForwardPlusTileSize { get; set; }
    public int? ForwardPlusMaxLights { get; set; }
    public int? ForwardPlusMaxLightsPerTile { get; set; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    public static ExportManifest FromJson(string json) =>
        JsonSerializer.Deserialize<ExportManifest>(json, JsonOptions)
        ?? throw new InvalidOperationException("Failed to deserialize export manifest.");
}
