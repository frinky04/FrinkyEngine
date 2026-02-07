using System.Diagnostics;
using System.Numerics;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Core.Serialization;

namespace FrinkyEngine.Editor;

public static class ProjectScaffolder
{
    /// <summary>
    /// Creates a new game project on disk and returns the path to the .fproject file.
    /// </summary>
    public static string CreateProject(string parentDirectory, string projectName)
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
        var coreProjectPath = FindCoreProjectPath(projectDir);
        var csprojContent = coreProjectPath != null
            ? GenerateCsprojWithProjectReference(coreProjectPath)
            : GenerateCsprojWithAssemblyReference(PrepareLocalCoreAssemblyReference(projectDir));
        File.WriteAllText(csprojPath, csprojContent);

        // 3. Create default scene (camera + light, same as NewScene)
        var scenesDir = Path.Combine(projectDir, "Assets", "Scenes");
        Directory.CreateDirectory(scenesDir);
        var scenePath = Path.Combine(scenesDir, "MainScene.fscene");
        var scene = BuildDefaultScene();
        SceneSerializer.Save(scene, scenePath);

        // 4. Create example RotatorComponent script
        var scriptsDir = Path.Combine(projectDir, "Assets", "Scripts");
        Directory.CreateDirectory(scriptsDir);
        var scriptPath = Path.Combine(scriptsDir, "RotatorComponent.cs");
        File.WriteAllText(scriptPath, GenerateRotatorComponent(projectName));

        // 5. Write .gitignore and initialize git repo
        var gitignorePath = Path.Combine(projectDir, ".gitignore");
        File.WriteAllText(gitignorePath, GenerateGitignore());
        InitializeGitRepo(projectDir);

        return fprojectPath;
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

    private static string PrepareLocalCoreAssemblyReference(string projectDir)
    {
        var sourceCoreAssembly = typeof(Component).Assembly.Location;
        if (!File.Exists(sourceCoreAssembly))
            throw new FileNotFoundException("Could not locate FrinkyEngine.Core.dll for project scaffolding.", sourceCoreAssembly);

        var engineDir = Path.Combine(projectDir, ".frinky", "engine");
        Directory.CreateDirectory(engineDir);

        var targetCoreAssembly = Path.Combine(engineDir, "FrinkyEngine.Core.dll");
        File.Copy(sourceCoreAssembly, targetCoreAssembly, overwrite: true);

        var sourcePdb = Path.ChangeExtension(sourceCoreAssembly, ".pdb");
        if (File.Exists(sourcePdb))
        {
            var targetPdb = Path.Combine(engineDir, "FrinkyEngine.Core.pdb");
            File.Copy(sourcePdb, targetPdb, overwrite: true);
        }

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

    private static Core.Scene.Scene BuildDefaultScene()
    {
        var scene = new Core.Scene.Scene { Name = "MainScene" };

        var cameraEntity = scene.CreateEntity("Main Camera");
        cameraEntity.Transform.LocalPosition = new Vector3(0, 5, 10);
        cameraEntity.Transform.EulerAngles = new Vector3(-20, 0, 0);
        cameraEntity.AddComponent<CameraComponent>();

        var lightEntity = scene.CreateEntity("Directional Light");
        lightEntity.Transform.LocalPosition = new Vector3(2, 10, 2);
        lightEntity.AddComponent<LightComponent>();

        return scene;
    }

    private static string GenerateRotatorComponent(string projectName)
    {
        return $$"""
            using System.Numerics;
            using FrinkyEngine.Core.ECS;

            namespace {{projectName}}.Scripts;

            public class RotatorComponent : Component
            {
                public float Speed { get; set; } = 45f;
                public Vector3 Axis { get; set; } = Vector3.UnitY;

                public override void Update(float dt)
                {
                    var euler = Entity.Transform.EulerAngles;
                    euler += Axis * Speed * dt;
                    Entity.Transform.EulerAngles = euler;
                }
            }
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
