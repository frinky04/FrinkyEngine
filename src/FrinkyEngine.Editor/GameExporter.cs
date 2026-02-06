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
    public string OutputDirectory { get; set; } = string.Empty;
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

        try
        {
            FrinkyLog.Info("Starting game export...");

            // Step 1: Build game scripts (Release) if there's a game project
            if (!string.IsNullOrEmpty(config.GameCsprojPath) && File.Exists(config.GameCsprojPath))
            {
                FrinkyLog.Info("Building game scripts (Release)...");
                if (!await RunDotnetAsync($"build \"{config.GameCsprojPath}\" -c Release"))
                {
                    FrinkyLog.Error("Game script build failed. Export aborted.");
                    return false;
                }
            }

            // Step 2: Publish Runtime
            var publishDir = Path.Combine(tempDir, "publish");
            FrinkyLog.Info("Publishing runtime...");
            if (!await RunDotnetAsync($"publish \"{config.RuntimeCsprojPath}\" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:FrinkyExport=true -o \"{publishDir}\""))
            {
                FrinkyLog.Error("Runtime publish failed. Export aborted.");
                return false;
            }

            // Step 3: Collect archive entries
            FrinkyLog.Info("Collecting assets...");
            var entries = new List<FAssetEntry>();

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
            if (!string.IsNullOrEmpty(config.GameAssemblyDll))
            {
                var gameDllFullPath = Path.Combine(config.ProjectDirectory, config.GameAssemblyDll);
                if (File.Exists(gameDllFullPath))
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
            }

            // 3d: Manifest
            var manifest = new ExportManifest
            {
                ProjectName = config.ProjectName,
                DefaultScene = "Assets/" + config.DefaultScene,
                GameAssembly = gameAssemblyRelPath
            };

            var manifestPath = Path.Combine(tempDir, "manifest.json");
            File.WriteAllText(manifestPath, manifest.ToJson());
            entries.Add(new FAssetEntry { RelativePath = "manifest.json", SourcePath = manifestPath });

            // Step 4: Write .fasset
            FrinkyLog.Info($"Packing {entries.Count} files into archive...");
            var fassetPath = Path.Combine(tempDir, $"{config.ProjectName}.fasset");
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

            var outputExe = Path.Combine(config.OutputDirectory, $"{config.ProjectName}.exe");
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
            var outputFasset = Path.Combine(config.OutputDirectory, $"{config.ProjectName}.fasset");
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
}
