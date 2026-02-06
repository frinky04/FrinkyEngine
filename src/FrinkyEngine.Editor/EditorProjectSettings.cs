using System.Text.Json;
using FrinkyEngine.Core.Rendering;

namespace FrinkyEngine.Editor;

public class EditorProjectSettings
{
    public const string FileName = "editor_settings.json";

    public int TargetFps { get; set; } = 120;
    public bool VSync { get; set; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static EditorProjectSettings LoadOrCreate(string? projectDirectory)
    {
        var defaults = GetDefault();
        if (projectDirectory == null)
            return defaults;

        var path = GetPath(projectDirectory);
        if (!File.Exists(path))
        {
            defaults.Save(projectDirectory);
            return defaults;
        }

        try
        {
            var json = File.ReadAllText(path);
            var settings = JsonSerializer.Deserialize<EditorProjectSettings>(json, JsonOptions) ?? GetDefault();
            settings.Normalize();
            return settings;
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"Failed to load editor project settings: {ex.Message}");
            return defaults;
        }
    }

    public void Save(string? projectDirectory)
    {
        if (projectDirectory == null)
            return;

        try
        {
            Normalize();
            var path = GetPath(projectDirectory);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"Failed to save editor project settings: {ex.Message}");
        }
    }

    public EditorProjectSettings Clone()
    {
        return new EditorProjectSettings
        {
            TargetFps = TargetFps,
            VSync = VSync
        };
    }

    public static string GetPath(string projectDirectory)
    {
        return Path.Combine(projectDirectory, ".frinky", FileName);
    }

    public static EditorProjectSettings GetDefault()
    {
        return new EditorProjectSettings
        {
            TargetFps = 120,
            VSync = false
        };
    }

    public void Normalize()
    {
        if (TargetFps < 30 || TargetFps > 500)
            TargetFps = 120;
    }
}
