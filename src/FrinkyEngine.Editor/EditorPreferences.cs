using System.Text.Json;
using FrinkyEngine.Core.Rendering;

namespace FrinkyEngine.Editor;

public class EditorPreferences
{
    public static EditorPreferences Instance { get; } = new();

    private string? _configPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void LoadConfig(string? projectDirectory)
    {
        if (projectDirectory == null) return;

        var configDir = Path.Combine(projectDirectory, ".frinky");
        _configPath = Path.Combine(configDir, "editor_preferences.json");

        if (File.Exists(_configPath))
        {
            try
            {
                var json = File.ReadAllText(_configPath);
                var data = JsonSerializer.Deserialize<EditorPreferencesData>(json, JsonOptions);
                if (data != null)
                    Apply(data);
            }
            catch (Exception ex)
            {
                FrinkyLog.Warning($"Failed to load editor preferences: {ex.Message}");
            }
        }
        else
        {
            Directory.CreateDirectory(configDir);
            SaveConfig();
        }
    }

    public void SaveConfig()
    {
        if (_configPath == null) return;

        try
        {
            var data = new EditorPreferencesData
            {
                IconScale = EditorIcons.IconScale
            };
            var json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"Failed to save editor preferences: {ex.Message}");
        }
    }

    private static void Apply(EditorPreferencesData data)
    {
        EditorIcons.IconScale = Math.Clamp(data.IconScale, 0.5f, 3.0f);
    }

    private class EditorPreferencesData
    {
        public float IconScale { get; set; } = 1.0f;
    }
}
