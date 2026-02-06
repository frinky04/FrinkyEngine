using System.Text.Json;

namespace FrinkyEngine.Core.Assets;

public class ProjectFile
{
    public string ProjectName { get; set; } = "Untitled";
    public string DefaultScene { get; set; } = "Scenes/MainScene.fscene";
    public string AssetsPath { get; set; } = "Assets";
    public string GameAssembly { get; set; } = string.Empty;
    public string GameProject { get; set; } = string.Empty;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static ProjectFile Load(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ProjectFile>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize project file.");
    }

    public void Save(string path)
    {
        var json = JsonSerializer.Serialize(this, JsonOptions);
        File.WriteAllText(path, json);
    }

    public string GetAbsoluteAssetsPath(string projectDir)
    {
        return Path.GetFullPath(Path.Combine(projectDir, AssetsPath));
    }

    public string GetAbsoluteScenePath(string projectDir)
    {
        return Path.GetFullPath(Path.Combine(projectDir, AssetsPath, DefaultScene));
    }
}
