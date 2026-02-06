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

        // 2. Create .csproj with ProjectReference to FrinkyEngine.Core
        var csprojPath = Path.Combine(projectDir, $"{projectName}.csproj");
        var coreProjectPath = FindCoreProjectPath(projectDir);
        var csprojContent = GenerateCsproj(coreProjectPath);
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

        return fprojectPath;
    }

    private static string FindCoreProjectPath(string projectDir)
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

        // Fallback: assume a relative path from project dir up to the engine
        return Path.GetRelativePath(projectDir,
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..",
                "src", "FrinkyEngine.Core", "FrinkyEngine.Core.csproj"));
    }

    private static string GenerateCsproj(string coreRelativePath)
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
}
