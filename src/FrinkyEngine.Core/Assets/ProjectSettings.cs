namespace FrinkyEngine.Core.Assets;

public class ProjectSettings
{
    public const string FileName = "ProjectSettings.ini";

    public ProjectMetadataSettings Project { get; set; } = new();
    public EditorProjectSettings Editor { get; set; } = new();
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
            Editor = new EditorProjectSettings
            {
                TargetFps = 60
            },
            Runtime = new RuntimeProjectSettings
            {
                TargetFps = 60,
                VSync = false,
                WindowTitle = normalizedProjectName,
                WindowWidth = 1280,
                WindowHeight = 720,
                Resizable = true,
                Fullscreen = false,
                StartMaximized = false,
                StartupSceneOverride = string.Empty
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

        if (!File.Exists(path))
        {
            var defaults = GetDefault(fallbackProjectName);
            defaults.Normalize(fallbackProjectName);
            defaults.Save(path);
            return defaults;
        }

        return Load(path, fallbackProjectName);
    }

    public static ProjectSettings Load(string path, string? projectName = null)
    {
        var defaultProjectName = ResolveProjectName(projectName, Path.GetDirectoryName(path));
        var settings = GetDefault(defaultProjectName);

        var data = ParseIni(path);

        settings.Project.Version = GetString(data, "Project", "Version", settings.Project.Version);
        settings.Project.Author = GetString(data, "Project", "Author", settings.Project.Author);
        settings.Project.Company = GetString(data, "Project", "Company", settings.Project.Company);
        settings.Project.Description = GetString(data, "Project", "Description", settings.Project.Description);

        settings.Editor.TargetFps = GetInt(data, "Editor", "TargetFps", settings.Editor.TargetFps);

        settings.Runtime.TargetFps = GetInt(data, "Runtime", "TargetFps", settings.Runtime.TargetFps);
        settings.Runtime.VSync = GetBool(data, "Runtime", "VSync", settings.Runtime.VSync);
        settings.Runtime.WindowTitle = GetString(data, "Runtime", "WindowTitle", settings.Runtime.WindowTitle);
        settings.Runtime.WindowWidth = GetInt(data, "Runtime", "WindowWidth", settings.Runtime.WindowWidth);
        settings.Runtime.WindowHeight = GetInt(data, "Runtime", "WindowHeight", settings.Runtime.WindowHeight);
        settings.Runtime.Resizable = GetBool(data, "Runtime", "Resizable", settings.Runtime.Resizable);
        settings.Runtime.Fullscreen = GetBool(data, "Runtime", "Fullscreen", settings.Runtime.Fullscreen);
        settings.Runtime.StartMaximized = GetBool(data, "Runtime", "StartMaximized", settings.Runtime.StartMaximized);
        settings.Runtime.StartupSceneOverride = GetString(data, "Runtime", "StartupSceneOverride", settings.Runtime.StartupSceneOverride);

        settings.Build.OutputName = GetString(data, "Build", "OutputName", settings.Build.OutputName);
        settings.Build.BuildVersion = GetString(data, "Build", "BuildVersion", settings.Build.BuildVersion);

        settings.Normalize(defaultProjectName);
        return settings;
    }

    public void Save(string path)
    {
        var defaultProjectName = ResolveProjectName(null, Path.GetDirectoryName(path));
        Normalize(defaultProjectName);

        var lines = new List<string>
        {
            "[Project]",
            $"Version={Project.Version}",
            $"Author={Project.Author}",
            $"Company={Project.Company}",
            $"Description={Project.Description}",
            string.Empty,
            "[Editor]",
            $"TargetFps={Editor.TargetFps}",
            string.Empty,
            "[Runtime]",
            $"TargetFps={Runtime.TargetFps}",
            $"VSync={Runtime.VSync.ToString().ToLowerInvariant()}",
            $"WindowTitle={Runtime.WindowTitle}",
            $"WindowWidth={Runtime.WindowWidth}",
            $"WindowHeight={Runtime.WindowHeight}",
            $"Resizable={Runtime.Resizable.ToString().ToLowerInvariant()}",
            $"Fullscreen={Runtime.Fullscreen.ToString().ToLowerInvariant()}",
            $"StartMaximized={Runtime.StartMaximized.ToString().ToLowerInvariant()}",
            $"StartupSceneOverride={Runtime.StartupSceneOverride}",
            string.Empty,
            "[Build]",
            $"OutputName={Build.OutputName}",
            $"BuildVersion={Build.BuildVersion}"
        };

        File.WriteAllText(path, string.Join(Environment.NewLine, lines) + Environment.NewLine);
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
            Editor = new EditorProjectSettings
            {
                TargetFps = Editor.TargetFps
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
                StartupSceneOverride = Runtime.StartupSceneOverride
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

        Editor ??= new EditorProjectSettings();
        Editor.TargetFps = Clamp(Editor.TargetFps, 30, 500, 60);

        Runtime ??= new RuntimeProjectSettings();
        Runtime.TargetFps = Clamp(Runtime.TargetFps, 30, 500, 60);
        Runtime.WindowTitle = Coalesce(Runtime.WindowTitle, safeProjectName);
        Runtime.WindowWidth = Clamp(Runtime.WindowWidth, 320, 10000, 1280);
        Runtime.WindowHeight = Clamp(Runtime.WindowHeight, 200, 10000, 720);
        Runtime.StartupSceneOverride = NormalizeScenePath(Runtime.StartupSceneOverride);

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

    private static Dictionary<string, Dictionary<string, string>> ParseIni(string path)
    {
        var sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        string currentSection = string.Empty;

        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith(';') || line.StartsWith('#'))
                continue;

            if (line.StartsWith('[') && line.EndsWith(']') && line.Length >= 3)
            {
                currentSection = line[1..^1].Trim();
                if (!sections.ContainsKey(currentSection))
                    sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                continue;
            }

            var eqIndex = line.IndexOf('=');
            if (eqIndex <= 0)
                continue;

            var key = line[..eqIndex].Trim();
            var value = line[(eqIndex + 1)..].Trim();
            if (string.IsNullOrEmpty(key))
                continue;

            if (!sections.TryGetValue(currentSection, out var map))
            {
                map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                sections[currentSection] = map;
            }

            map[key] = value;
        }

        return sections;
    }

    private static string GetString(
        IReadOnlyDictionary<string, Dictionary<string, string>> data,
        string section,
        string key,
        string fallback)
    {
        if (data.TryGetValue(section, out var map) && map.TryGetValue(key, out var value))
            return value;
        return fallback;
    }

    private static int GetInt(
        IReadOnlyDictionary<string, Dictionary<string, string>> data,
        string section,
        string key,
        int fallback)
    {
        if (data.TryGetValue(section, out var map) && map.TryGetValue(key, out var raw) && int.TryParse(raw, out var value))
            return value;
        return fallback;
    }

    private static bool GetBool(
        IReadOnlyDictionary<string, Dictionary<string, string>> data,
        string section,
        string key,
        bool fallback)
    {
        if (data.TryGetValue(section, out var map) && map.TryGetValue(key, out var raw))
        {
            if (bool.TryParse(raw, out var parsedBool))
                return parsedBool;
            if (raw == "1")
                return true;
            if (raw == "0")
                return false;
        }

        return fallback;
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

public class EditorProjectSettings
{
    public int TargetFps { get; set; } = 60;
}

public class RuntimeProjectSettings
{
    public int TargetFps { get; set; } = 60;
    public bool VSync { get; set; }
    public string WindowTitle { get; set; } = "Untitled";
    public int WindowWidth { get; set; } = 1280;
    public int WindowHeight { get; set; } = 720;
    public bool Resizable { get; set; } = true;
    public bool Fullscreen { get; set; }
    public bool StartMaximized { get; set; }
    public string StartupSceneOverride { get; set; } = string.Empty;
}

public class BuildProjectSettings
{
    public string OutputName { get; set; } = "Untitled";
    public string BuildVersion { get; set; } = "0.1.0";
}
