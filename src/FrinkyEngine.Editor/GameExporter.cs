using System.Diagnostics;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Rendering;

namespace FrinkyEngine.Editor;

public class ExportConfig
{
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectDirectory { get; set; } = string.Empty;
    public string AssetsPath { get; set; } = string.Empty;
    public string DefaultScene { get; set; } = string.Empty;
    public string? GameCsprojPath { get; set; }
    public string? GameAssemblyDll { get; set; }
    public string RuntimeCsprojPath { get; set; } = string.Empty;
    public string? RuntimeTemplateDirectory { get; set; }
    public string OutputDirectory { get; set; } = string.Empty;
    public ProjectSettings? ProjectSettings { get; set; }
}

public static class GameExporter
{
    public static bool IsExporting { get; private set; }

    public static async Task<bool> ExportAsync(ExportConfig config)
    {
        if (IsExporting)
            return false;

        IsExporting = true;
        var tempDir = Path.Combine(Path.GetTempPath(), $"FrinkyExport_{Guid.NewGuid():N}");
        string? builtGameAssemblyPath = null;

        try
        {
            FrinkyLog.Info("Starting game export...");

            // Step 1: Build game scripts (Release) if there's a game project
            if (!string.IsNullOrEmpty(config.GameCsprojPath) && File.Exists(config.GameCsprojPath))
            {
                FrinkyLog.Info("Building game scripts (Release)...");
                var gameBuildOutputDir = Path.Combine(tempDir, "gamebuild");
                if (!await RunDotnetAsync($"build \"{config.GameCsprojPath}\" -c Release -o \"{gameBuildOutputDir}\""))
                {
                    FrinkyLog.Error("Game script build failed. Export aborted.");
                    return false;
                }

                builtGameAssemblyPath = ResolveBuiltGameAssemblyPath(config, gameBuildOutputDir);
                if (builtGameAssemblyPath != null)
                    FrinkyLog.Info($"Using built game assembly: {Path.GetFileName(builtGameAssemblyPath)}");
            }

            // Step 2: Prepare Runtime (publish from source or use bundled template)
            var publishDir = Path.Combine(tempDir, "publish");
            FrinkyLog.Info("Preparing runtime...");
            if (!await PrepareRuntimeAsync(config, publishDir))
            {
                FrinkyLog.Error("Runtime preparation failed. Export aborted.");
                return false;
            }

            // Step 3: Collect archive entries
            FrinkyLog.Info("Collecting assets...");
            var entries = new List<FAssetEntry>();
            var projectSettings = config.ProjectSettings?.Clone() ?? ProjectSettings.GetDefault(config.ProjectName);
            projectSettings.Normalize(config.ProjectName);
            var outputName = SanitizeFileName(projectSettings.Build.OutputName, config.ProjectName);
            var defaultSceneRelative = projectSettings.ResolveStartupScene(config.DefaultScene);

            // 3a: Assets directory
            if (Directory.Exists(config.AssetsPath))
            {
                foreach (var file in Directory.GetFiles(config.AssetsPath, "*", SearchOption.AllDirectories))
                {
                    var relativePath = "Assets/" + Path.GetRelativePath(config.AssetsPath, file).Replace('\\', '/');
                    entries.Add(new FAssetEntry { RelativePath = relativePath, SourcePath = file });
                }
            }

            // 3b: Shaders from published output
            var publishedShadersDir = Path.Combine(publishDir, "Shaders");
            if (Directory.Exists(publishedShadersDir))
            {
                foreach (var file in Directory.GetFiles(publishedShadersDir, "*", SearchOption.AllDirectories))
                {
                    var relativePath = "Shaders/" + Path.GetRelativePath(publishedShadersDir, file).Replace('\\', '/');
                    entries.Add(new FAssetEntry { RelativePath = relativePath, SourcePath = file });
                }
            }

            // 3c: Game assembly + dependencies
            string? gameAssemblyRelPath = null;
            var gameDllFullPath = ResolveGameAssemblyPath(config, builtGameAssemblyPath);
            if (!string.IsNullOrEmpty(gameDllFullPath) && File.Exists(gameDllFullPath))
            {
                var gameDllDir = Path.GetDirectoryName(gameDllFullPath)!;
                var runtimeDlls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var f in Directory.GetFiles(publishDir, "*.dll"))
                    runtimeDlls.Add(Path.GetFileName(f));

                foreach (var file in Directory.GetFiles(gameDllDir, "*.dll"))
                {
                    var fileName = Path.GetFileName(file);
                    if (runtimeDlls.Contains(fileName))
                        continue;

                    var relativePath = "GameAssembly/" + fileName;
                    entries.Add(new FAssetEntry { RelativePath = relativePath, SourcePath = file });
                }

                gameAssemblyRelPath = "GameAssembly/" + Path.GetFileName(gameDllFullPath);
            }
            else if (!string.IsNullOrWhiteSpace(config.GameAssemblyDll))
            {
                FrinkyLog.Warning($"Configured game assembly was not found for export: {config.GameAssemblyDll}");
            }
            else
            {
                FrinkyLog.Warning("No game assembly configured. Export will run without scripts.");
            }

            // 3d: Manifest
            var manifest = new ExportManifest
            {
                ProjectName = config.ProjectName,
                ProductName = outputName,
                BuildVersion = projectSettings.Build.BuildVersion,
                DefaultScene = "Assets/" + defaultSceneRelative,
                GameAssembly = gameAssemblyRelPath,
                TargetFps = projectSettings.Runtime.TargetFps,
                VSync = projectSettings.Runtime.VSync,
                WindowTitle = projectSettings.Runtime.WindowTitle,
                WindowWidth = projectSettings.Runtime.WindowWidth,
                WindowHeight = projectSettings.Runtime.WindowHeight,
                Resizable = projectSettings.Runtime.Resizable,
                Fullscreen = projectSettings.Runtime.Fullscreen,
                StartMaximized = projectSettings.Runtime.StartMaximized,
                ForwardPlusTileSize = projectSettings.Runtime.ForwardPlusTileSize,
                ForwardPlusMaxLights = projectSettings.Runtime.ForwardPlusMaxLights,
                ForwardPlusMaxLightsPerTile = projectSettings.Runtime.ForwardPlusMaxLightsPerTile,
                PhysicsFixedTimestep = projectSettings.Runtime.PhysicsFixedTimestep,
                PhysicsMaxSubstepsPerFrame = projectSettings.Runtime.PhysicsMaxSubstepsPerFrame,
                PhysicsSolverVelocityIterations = projectSettings.Runtime.PhysicsSolverVelocityIterations,
                PhysicsSolverSubsteps = projectSettings.Runtime.PhysicsSolverSubsteps,
                PhysicsContactSpringFrequency = projectSettings.Runtime.PhysicsContactSpringFrequency,
                PhysicsContactDampingRatio = projectSettings.Runtime.PhysicsContactDampingRatio,
                PhysicsMaximumRecoveryVelocity = projectSettings.Runtime.PhysicsMaximumRecoveryVelocity,
                PhysicsDefaultFriction = projectSettings.Runtime.PhysicsDefaultFriction,
                PhysicsDefaultRestitution = projectSettings.Runtime.PhysicsDefaultRestitution
            };

            var manifestPath = Path.Combine(tempDir, "manifest.json");
            File.WriteAllText(manifestPath, manifest.ToJson());
            entries.Add(new FAssetEntry { RelativePath = "manifest.json", SourcePath = manifestPath });

            // Step 4: Write .fasset
            FrinkyLog.Info($"Packing {entries.Count} files into archive...");
            var fassetPath = Path.Combine(tempDir, $"{outputName}.fasset");
            FAssetArchive.Write(fassetPath, entries);

            // Step 5: Copy published Runtime to output, rename exe
            Directory.CreateDirectory(config.OutputDirectory);

            // Find the runtime exe in publish output
            var runtimeExe = Path.Combine(publishDir, "FrinkyEngine.Runtime.exe");
            if (!File.Exists(runtimeExe))
            {
                // Fallback: try without .exe for Linux-style builds
                runtimeExe = Path.Combine(publishDir, "FrinkyEngine.Runtime");
                if (!File.Exists(runtimeExe))
                {
                    FrinkyLog.Error("Published runtime executable not found.");
                    return false;
                }
            }

            var outputExe = Path.Combine(config.OutputDirectory, $"{outputName}.exe");
            File.Copy(runtimeExe, outputExe, overwrite: true);

            // Copy all other runtime files (DLLs, native libs, etc.) except the exe itself and Shaders
            foreach (var file in Directory.GetFiles(publishDir, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(publishDir, file);

                // Skip the original exe (already copied/renamed)
                if (relativePath.Equals("FrinkyEngine.Runtime.exe", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Skip shaders (packed into .fasset)
                if (relativePath.StartsWith("Shaders", StringComparison.OrdinalIgnoreCase))
                    continue;

                var destPath = Path.Combine(config.OutputDirectory, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                File.Copy(file, destPath, overwrite: true);
            }

            // Step 6: Move .fasset next to the exe
            var outputFasset = Path.Combine(config.OutputDirectory, $"{outputName}.fasset");
            File.Copy(fassetPath, outputFasset, overwrite: true);

            FrinkyLog.Info($"Export complete: {config.OutputDirectory}");
            return true;
        }
        catch (Exception ex)
        {
            FrinkyLog.Error($"Export error: {ex.Message}");
            return false;
        }
        finally
        {
            // Step 7: Clean up temp
            try
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, recursive: true);
            }
            catch
            {
                // Best effort cleanup
            }

            IsExporting = false;
        }
    }

    public static string? FindRuntimeCsproj()
    {
        // Walk up from AppContext.BaseDirectory to find FrinkyEngine.sln
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            var slnPath = Path.Combine(dir, "FrinkyEngine.sln");
            if (File.Exists(slnPath))
            {
                var runtimeCsproj = Path.Combine(dir, "src", "FrinkyEngine.Runtime", "FrinkyEngine.Runtime.csproj");
                return File.Exists(runtimeCsproj) ? runtimeCsproj : null;
            }
            dir = Path.GetDirectoryName(dir);
        }
        return null;
    }

    public static string? FindRuntimeTemplateDirectory()
    {
        return ResolveRuntimeTemplateDirectory(null);
    }

    private static async Task<bool> PrepareRuntimeAsync(ExportConfig config, string publishDir)
    {
        if (!string.IsNullOrWhiteSpace(config.RuntimeCsprojPath) && File.Exists(config.RuntimeCsprojPath))
        {
            FrinkyLog.Info("Publishing runtime from source project...");
            return await RunDotnetAsync(
                $"publish \"{config.RuntimeCsprojPath}\" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:FrinkyExport=true -o \"{publishDir}\"");
        }

        var runtimeTemplateDir = ResolveRuntimeTemplateDirectory(config.RuntimeTemplateDirectory);
        if (!string.IsNullOrWhiteSpace(runtimeTemplateDir))
        {
            FrinkyLog.Info("Using bundled runtime template for export.");
            CopyDirectory(runtimeTemplateDir, publishDir);
            return true;
        }

        FrinkyLog.Error("Runtime source project and bundled runtime template were not found.");
        return false;
    }

    private static string? ResolveBuiltGameAssemblyPath(ExportConfig config, string buildOutputDir)
    {
        if (!Directory.Exists(buildOutputDir))
            return null;

        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(config.GameAssemblyDll))
            candidates.Add(Path.Combine(buildOutputDir, Path.GetFileName(config.GameAssemblyDll)));
        if (!string.IsNullOrWhiteSpace(config.GameCsprojPath))
        {
            var fromProjectName = Path.Combine(
                buildOutputDir,
                Path.GetFileNameWithoutExtension(config.GameCsprojPath) + ".dll");
            candidates.Add(fromProjectName);
        }

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
                return candidate;
        }

        var knownEngineAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "FrinkyEngine.Core.dll",
            "FrinkyEngine.Runtime.dll",
            "FrinkyEngine.Editor.dll"
        };

        var fallback = Directory.GetFiles(buildOutputDir, "*.dll", SearchOption.TopDirectoryOnly)
            .FirstOrDefault(path => !knownEngineAssemblies.Contains(Path.GetFileName(path)));

        return fallback;
    }

    private static string? ResolveGameAssemblyPath(ExportConfig config, string? builtGameAssemblyPath)
    {
        if (!string.IsNullOrWhiteSpace(builtGameAssemblyPath) && File.Exists(builtGameAssemblyPath))
            return builtGameAssemblyPath;

        if (!string.IsNullOrWhiteSpace(config.GameAssemblyDll))
        {
            var configuredPath = Path.Combine(config.ProjectDirectory, config.GameAssemblyDll);
            if (File.Exists(configuredPath))
                return configuredPath;
        }

        return null;
    }

    private static string? ResolveRuntimeTemplateDirectory(string? configuredPath)
    {
        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(configuredPath))
            candidates.Add(configuredPath);

        candidates.Add(Path.Combine(AppContext.BaseDirectory, "RuntimeTemplate"));
        candidates.Add(Path.Combine(Environment.CurrentDirectory, "RuntimeTemplate"));

        foreach (var candidate in candidates)
        {
            if (!Directory.Exists(candidate))
                continue;
            if (!ContainsRuntimeExecutable(candidate))
                continue;
            return candidate;
        }

        return null;
    }

    private static bool ContainsRuntimeExecutable(string directory)
    {
        var exePath = Path.Combine(directory, "FrinkyEngine.Runtime.exe");
        if (File.Exists(exePath))
            return true;

        var noExtPath = Path.Combine(directory, "FrinkyEngine.Runtime");
        return File.Exists(noExtPath);
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);

        foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDir, file);
            var destPath = Path.Combine(destinationDir, relative);
            var parent = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrWhiteSpace(parent))
                Directory.CreateDirectory(parent);
            File.Copy(file, destPath, overwrite: true);
        }
    }

    private static async Task<bool> RunDotnetAsync(string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null)
        {
            FrinkyLog.Error("Failed to start dotnet process.");
            return false;
        }

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (!string.IsNullOrWhiteSpace(stdout))
        {
            foreach (var line in stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                FrinkyLog.Info(line.TrimEnd());
        }

        if (!string.IsNullOrWhiteSpace(stderr))
        {
            foreach (var line in stderr.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                FrinkyLog.Error(line.TrimEnd());
        }

        return process.ExitCode == 0;
    }

    private static string SanitizeFileName(string value, string fallback)
    {
        var selected = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(selected.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? fallback : sanitized;
    }
}
