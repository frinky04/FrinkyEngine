using System.Text.Json;

namespace FrinkyEngine.Core.Assets;

public class ProjectSettings
{
    public const string FileName = "project_settings.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ProjectMetadataSettings Project { get; set; } = new();
    public RuntimeProjectSettings Runtime { get; set; } = new();
    public BuildProjectSettings Build { get; set; } = new();

    public static string GetPath(string projectDirectory)
    {
        return Path.Combine(projectDirectory, FileName);
    }

    public static ProjectSettings GetDefault(string projectName)
    {
        var normalizedProjectName = string.IsNullOrWhiteSpace(projectName) ? "Untitled" : projectName.Trim();

        return new ProjectSettings
        {
            Project = new ProjectMetadataSettings
            {
                Version = "0.1.0",
                Author = string.Empty,
                Company = string.Empty,
                Description = string.Empty
            },
            Runtime = new RuntimeProjectSettings
            {
                TargetFps = 120,
                VSync = true,
                WindowTitle = normalizedProjectName,
                WindowWidth = 1280,
                WindowHeight = 720,
                Resizable = true,
                Fullscreen = false,
                StartMaximized = false,
                StartupSceneOverride = string.Empty,
                ForwardPlusTileSize = 16,
                ForwardPlusMaxLights = 256,
                ForwardPlusMaxLightsPerTile = 64
            },
            Build = new BuildProjectSettings
            {
                OutputName = normalizedProjectName,
                BuildVersion = "0.1.0"
            }
        };
    }

    public static ProjectSettings LoadOrCreate(string projectDirectory, string? projectName = null)
    {
        var path = GetPath(projectDirectory);
        var fallbackProjectName = ResolveProjectName(projectName, projectDirectory);

        if (File.Exists(path))
            return Load(path, fallbackProjectName);

        var defaults = GetDefault(fallbackProjectName);
        defaults.Normalize(fallbackProjectName);
        defaults.Save(path);
        return defaults;
    }

    public static ProjectSettings Load(string path, string? projectName = null)
    {
        var defaultProjectName = ResolveProjectName(projectName, Path.GetDirectoryName(path));

        try
        {
            var json = File.ReadAllText(path);
            var settings = JsonSerializer.Deserialize<ProjectSettings>(json, JsonOptions)
                           ?? GetDefault(defaultProjectName);
            settings.Normalize(defaultProjectName);
            return settings;
        }
        catch
        {
            var fallback = GetDefault(defaultProjectName);
            fallback.Normalize(defaultProjectName);
            return fallback;
        }
    }

    public void Save(string path)
    {
        var defaultProjectName = ResolveProjectName(null, Path.GetDirectoryName(path));
        Normalize(defaultProjectName);
        var json = JsonSerializer.Serialize(this, JsonOptions);
        File.WriteAllText(path, json);
    }

    public ProjectSettings Clone()
    {
        return new ProjectSettings
        {
            Project = new ProjectMetadataSettings
            {
                Version = Project.Version,
                Author = Project.Author,
                Company = Project.Company,
                Description = Project.Description
            },
            Runtime = new RuntimeProjectSettings
            {
                TargetFps = Runtime.TargetFps,
                VSync = Runtime.VSync,
                WindowTitle = Runtime.WindowTitle,
                WindowWidth = Runtime.WindowWidth,
                WindowHeight = Runtime.WindowHeight,
                Resizable = Runtime.Resizable,
                Fullscreen = Runtime.Fullscreen,
                StartMaximized = Runtime.StartMaximized,
                StartupSceneOverride = Runtime.StartupSceneOverride,
                ForwardPlusTileSize = Runtime.ForwardPlusTileSize,
                ForwardPlusMaxLights = Runtime.ForwardPlusMaxLights,
                ForwardPlusMaxLightsPerTile = Runtime.ForwardPlusMaxLightsPerTile
            },
            Build = new BuildProjectSettings
            {
                OutputName = Build.OutputName,
                BuildVersion = Build.BuildVersion
            }
        };
    }

    public void Normalize(string defaultProjectName)
    {
        var safeProjectName = string.IsNullOrWhiteSpace(defaultProjectName) ? "Untitled" : defaultProjectName.Trim();

        Project ??= new ProjectMetadataSettings();
        Project.Version = Coalesce(Project.Version, "0.1.0");
        Project.Author = Coalesce(Project.Author, string.Empty);
        Project.Company = Coalesce(Project.Company, string.Empty);
        Project.Description = CoalesceSingleLine(Project.Description, string.Empty);

        Runtime ??= new RuntimeProjectSettings();
        Runtime.TargetFps = Clamp(Runtime.TargetFps, 30, 500, 120);
        Runtime.WindowTitle = Coalesce(Runtime.WindowTitle, safeProjectName);
        Runtime.WindowWidth = Clamp(Runtime.WindowWidth, 320, 10000, 1280);
        Runtime.WindowHeight = Clamp(Runtime.WindowHeight, 200, 10000, 720);
        Runtime.StartupSceneOverride = NormalizeScenePath(Runtime.StartupSceneOverride);
        Runtime.ForwardPlusTileSize = Clamp(Runtime.ForwardPlusTileSize, 8, 64, 16);
        Runtime.ForwardPlusMaxLights = Clamp(Runtime.ForwardPlusMaxLights, 16, 2048, 256);
        Runtime.ForwardPlusMaxLightsPerTile = Clamp(Runtime.ForwardPlusMaxLightsPerTile, 8, 256, 64);

        Build ??= new BuildProjectSettings();
        Build.OutputName = Coalesce(Build.OutputName, safeProjectName);
        Build.BuildVersion = Coalesce(Build.BuildVersion, Project.Version);
    }

    public string ResolveStartupScene(string defaultScene)
    {
        var scene = string.IsNullOrWhiteSpace(Runtime.StartupSceneOverride)
            ? defaultScene
            : Runtime.StartupSceneOverride;
        return NormalizeScenePath(scene);
    }

    private static string ResolveProjectName(string? projectName, string? directoryPath)
    {
        if (!string.IsNullOrWhiteSpace(projectName))
            return projectName.Trim();

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            var folderName = Path.GetFileName(directoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (!string.IsNullOrWhiteSpace(folderName))
                return folderName;
        }

        return "Untitled";
    }

    private static int Clamp(int value, int min, int max, int fallback)
    {
        if (value < min || value > max)
            return fallback;
        return value;
    }

    private static string Coalesce(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static string CoalesceSingleLine(string? value, string fallback)
    {
        var text = Coalesce(value, fallback);
        return text.Replace("\r", " ").Replace("\n", " ");
    }

    private static string NormalizeScenePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var scenePath = value.Trim().Replace('\\', '/');
        if (scenePath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            scenePath = scenePath["Assets/".Length..];

        return scenePath;
    }
}

public class ProjectMetadataSettings
{
    public string Version { get; set; } = "0.1.0";
    public string Author { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class RuntimeProjectSettings
{
    public int TargetFps { get; set; } = 120;
    public bool VSync { get; set; } = true;
    public string WindowTitle { get; set; } = "Untitled";
    public int WindowWidth { get; set; } = 1280;
    public int WindowHeight { get; set; } = 720;
    public bool Resizable { get; set; } = true;
    public bool Fullscreen { get; set; }
    public bool StartMaximized { get; set; }
    public string StartupSceneOverride { get; set; } = string.Empty;
    public int ForwardPlusTileSize { get; set; } = 16;
    public int ForwardPlusMaxLights { get; set; } = 256;
    public int ForwardPlusMaxLightsPerTile { get; set; } = 64;
}

public class BuildProjectSettings
{
    public string OutputName { get; set; } = "Untitled";
    public string BuildVersion { get; set; } = "0.1.0";
}
