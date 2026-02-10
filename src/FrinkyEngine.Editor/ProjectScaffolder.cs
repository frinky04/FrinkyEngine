using System.Diagnostics;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Rendering;

namespace FrinkyEngine.Editor;

public static class ProjectScaffolder
{
    private static readonly HashSet<string> TextFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs", ".json", ".fscene", ".fprefab", ".txt", ".md", ".gitignore"
    };

    /// <summary>
    /// Creates a new game project on disk and returns the path to the .fproject file.
    /// </summary>
    public static string CreateProject(string parentDirectory, string projectName, ProjectTemplate template)
    {
        var projectDir = Path.Combine(parentDirectory, projectName);
        Directory.CreateDirectory(projectDir);

        // 1. Create .fproject
        var fprojectPath = Path.Combine(projectDir, $"{projectName}.fproject");
        var projectFile = new ProjectFile
        {
            ProjectName = projectName,
            DefaultScene = "Scenes/MainScene.fscene",
            AssetsPath = "Assets",
            GameProject = $"{projectName}.csproj",
            GameAssembly = $"bin/Debug/net8.0/{projectName}.dll"
        };
        projectFile.Save(fprojectPath);

        var settingsPath = ProjectSettings.GetPath(projectDir);
        var settings = ProjectSettings.GetDefault(projectName);
        settings.Save(settingsPath);

        var editorSettings = EditorProjectSettings.GetDefault();
        editorSettings.Save(projectDir);

        // 2. Create .csproj with FrinkyEngine.Core reference
        var csprojPath = Path.Combine(projectDir, $"{projectName}.csproj");
        var coreRelativePath = FindCoreProjectPath(projectDir);
        var csprojContent = coreRelativePath != null
            ? GenerateCsprojWithProjectReference(coreRelativePath)
            : GenerateCsprojWithAssemblyReference(PrepareLocalCoreAssemblyReference(projectDir));
        File.WriteAllText(csprojPath, csprojContent);

        // 3. Create .sln and restore NuGet packages
        CreateSolutionAndRestore(projectDir, projectName);

        // 4. Copy template content (scenes, scripts, etc.)
        CopyTemplateContent(template.ContentDirectory, projectDir, template.SourceName, projectName);

        // 5. Write .gitignore and initialize git repo
        var gitignorePath = Path.Combine(projectDir, ".gitignore");
        File.WriteAllText(gitignorePath, GenerateGitignore());
        InitializeGitRepo(projectDir);

        return fprojectPath;
    }

    /// <summary>
    /// Creates a new game project using the default template (3d-starter).
    /// </summary>
    public static string CreateProject(string parentDirectory, string projectName)
    {
        var template = ProjectTemplateRegistry.GetById("3d-starter")
            ?? ProjectTemplateRegistry.Templates.FirstOrDefault()
            ?? throw new InvalidOperationException("No project templates found. Ensure ProjectTemplateRegistry.Discover() has been called.");
        return CreateProject(parentDirectory, projectName, template);
    }

    private static void CopyTemplateContent(string contentDir, string projectDir, string sourceName, string projectName)
    {
        foreach (var sourceFile in Directory.EnumerateFiles(contentDir, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(contentDir, sourceFile);

            // Skip .template.config directories (dotnet new metadata)
            if (relativePath.StartsWith(".template.config", StringComparison.OrdinalIgnoreCase))
                continue;

            // Skip root-level files that the scaffolder generates dynamically
            if (!relativePath.Contains(Path.DirectorySeparatorChar) && !relativePath.Contains(Path.AltDirectorySeparatorChar))
            {
                var ext = Path.GetExtension(relativePath);
                if (ext.Equals(".csproj", StringComparison.OrdinalIgnoreCase) ||
                    ext.Equals(".fproject", StringComparison.OrdinalIgnoreCase) ||
                    Path.GetFileName(relativePath).Equals(".gitignore", StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            var targetPath = Path.Combine(projectDir, relativePath);
            var targetDirectory = Path.GetDirectoryName(targetPath)!;
            Directory.CreateDirectory(targetDirectory);

            var extension = Path.GetExtension(sourceFile);
            if (TextFileExtensions.Contains(extension))
            {
                // Perform sourceName → projectName replacement in text files
                var content = File.ReadAllText(sourceFile);
                content = content.Replace(sourceName, projectName);
                File.WriteAllText(targetPath, content);
            }
            else
            {
                File.Copy(sourceFile, targetPath, overwrite: true);
            }
        }

        FrinkyLog.Info($"Scaffold: copied template content from {contentDir}");
    }

    private static string? FindCoreProjectPath(string projectDir)
    {
        // Walk up from the editor's base directory looking for FrinkyEngine.sln
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "FrinkyEngine.sln")))
            {
                var coreAbsolute = Path.Combine(dir, "src", "FrinkyEngine.Core", "FrinkyEngine.Core.csproj");
                if (File.Exists(coreAbsolute))
                {
                    return Path.GetRelativePath(projectDir, coreAbsolute);
                }
            }
            dir = Path.GetDirectoryName(dir);
        }

        return null;
    }

    /// <summary>
    /// Copies FrinkyEngine.Core DLL (and PDB/XML if present) into the given engine directory.
    /// Skips copy if the target DLL is already up to date.
    /// Returns true if files were copied, false if source doesn't exist or target is already current.
    /// </summary>
    internal static bool CopyCoreAssemblyFiles(string engineDir)
    {
        var sourceDll = typeof(Component).Assembly.Location;
        if (string.IsNullOrEmpty(sourceDll) || !File.Exists(sourceDll))
            return false;

        Directory.CreateDirectory(engineDir);

        var targetDll = Path.Combine(engineDir, "FrinkyEngine.Core.dll");
        if (File.Exists(targetDll) &&
            File.GetLastWriteTimeUtc(targetDll) >= File.GetLastWriteTimeUtc(sourceDll))
        {
            return false;
        }

        File.Copy(sourceDll, targetDll, overwrite: true);

        var sourcePdb = Path.ChangeExtension(sourceDll, ".pdb");
        if (File.Exists(sourcePdb))
            File.Copy(sourcePdb, Path.Combine(engineDir, "FrinkyEngine.Core.pdb"), overwrite: true);

        var sourceXml = Path.ChangeExtension(sourceDll, ".xml");
        if (File.Exists(sourceXml))
            File.Copy(sourceXml, Path.Combine(engineDir, "FrinkyEngine.Core.xml"), overwrite: true);

        return true;
    }

    /// <summary>
    /// Updates .frinky/engine/ assemblies if the editor ships a newer Core DLL.
    /// Safe to call on any project — returns silently if the directory doesn't exist
    /// (i.e. the project uses a ProjectReference to engine source).
    /// </summary>
    public static void UpdateCoreAssemblyIfNeeded(string projectDir)
    {
        var engineDir = Path.Combine(projectDir, ".frinky", "engine");
        if (!Directory.Exists(engineDir))
            return;

        try
        {
            if (CopyCoreAssemblyFiles(engineDir))
                FrinkyLog.Info("Updated .frinky/engine/ assemblies to match current editor.");
            else
                FrinkyLog.Info(".frinky/engine/ assemblies are up to date.");
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"Could not update .frinky/engine/ assemblies: {ex.Message}");
        }
    }

    private static string PrepareLocalCoreAssemblyReference(string projectDir)
    {
        var engineDir = Path.Combine(projectDir, ".frinky", "engine");
        CopyCoreAssemblyFiles(engineDir);

        var targetCoreAssembly = Path.Combine(engineDir, "FrinkyEngine.Core.dll");
        if (!File.Exists(targetCoreAssembly))
            throw new FileNotFoundException("Could not locate FrinkyEngine.Core.dll for project scaffolding.", targetCoreAssembly);

        FrinkyLog.Info("Scaffold: using local FrinkyEngine.Core assembly reference.");
        return Path.GetRelativePath(projectDir, targetCoreAssembly).Replace('\\', '/');
    }

    private static string GenerateCsprojWithProjectReference(string coreRelativePath)
    {
        // Normalize to forward slashes for cross-platform .csproj compatibility
        var normalizedPath = coreRelativePath.Replace('\\', '/');
        return $"""
            <Project Sdk="Microsoft.NET.Sdk">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <Nullable>enable</Nullable>
                <LangVersion>12</LangVersion>
                <ImplicitUsings>enable</ImplicitUsings>
                <OutputType>Library</OutputType>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Raylib-cs" Version="7.0.2" />
                <ProjectReference Include="{normalizedPath}" />
              </ItemGroup>

            </Project>
            """;
    }

    private static string GenerateCsprojWithAssemblyReference(string coreHintPath)
    {
        return $"""
            <Project Sdk="Microsoft.NET.Sdk">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <Nullable>enable</Nullable>
                <LangVersion>12</LangVersion>
                <ImplicitUsings>enable</ImplicitUsings>
                <OutputType>Library</OutputType>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Raylib-cs" Version="7.0.2" />
                <Reference Include="FrinkyEngine.Core">
                  <HintPath>{coreHintPath}</HintPath>
                  <Private>false</Private>
                </Reference>
              </ItemGroup>

            </Project>
            """;
    }

    private static string GenerateGitignore()
    {
        return """
            ## .NET
            bin/
            obj/
            *.user
            *.suo
            *.cache

            ## IDE
            .vs/
            .idea/
            *.swp
            *~

            ## Build
            publish/
            out/

            ## OS
            Thumbs.db
            .DS_Store

            ## Engine
            .frinky/
            *.fproject.user
            imgui.ini
            """;
    }

    private static void CreateSolutionAndRestore(string projectDir, string projectName)
    {
        try
        {
            RunDotnet(projectDir, $"new sln -n \"{projectName}\"");
            RunDotnet(projectDir, $"sln \"{projectName}.sln\" add \"{projectName}.csproj\"");
            RunDotnet(projectDir, "restore");
            FrinkyLog.Info("Scaffold: created solution and restored packages.");
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"Scaffold: could not create solution or restore packages (dotnet SDK may not be installed): {ex.Message}");
        }
    }

    private static void InitializeGitRepo(string projectDir)
    {
        try
        {
            RunGit(projectDir, "init");
            RunGit(projectDir, "add .");
            RunGit(projectDir, "commit -m \"Initial commit\"");
            FrinkyLog.Info("Scaffold: initialized git repository.");
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"Scaffold: could not initialize git repo (git may not be installed): {ex.Message}");
        }
    }

    private static void RunDotnet(string workingDirectory, string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        process.WaitForExit(60_000);

        if (process.ExitCode != 0)
        {
            var stderr = process.StandardError.ReadToEnd();
            throw new InvalidOperationException($"dotnet {arguments} failed: {stderr}");
        }
    }

    private static void RunGit(string workingDirectory, string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        process.WaitForExit(10_000);

        if (process.ExitCode != 0)
        {
            var stderr = process.StandardError.ReadToEnd();
            throw new InvalidOperationException($"git {arguments} failed: {stderr}");
        }
    }
}
