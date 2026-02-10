using System.Diagnostics;
using FrinkyEngine.Core.Rendering;

namespace FrinkyEngine.Editor;

public static class ScriptBuilder
{
    public static bool IsBuilding { get; private set; }

    public static async Task<bool> BuildAsync(string csprojPath)
    {
        if (IsBuilding)
            return false;

        IsBuilding = true;
        try
        {
            FrinkyLog.Info($"Building scripts: {Path.GetFileName(csprojPath)}...");

            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{csprojPath}\" --configuration Debug",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                FrinkyLog.Error("Failed to start dotnet build process.");
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

            var success = process.ExitCode == 0;
            FrinkyLog.Info(success ? "Build succeeded." : "Build failed.");
            return success;
        }
        catch (Exception ex)
        {
            FrinkyLog.Error($"Build error: {ex.Message}");
            return false;
        }
        finally
        {
            IsBuilding = false;
        }
    }
}
