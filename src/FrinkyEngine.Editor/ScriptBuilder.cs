using System.Diagnostics;
using System.Xml.Linq;
using FrinkyEngine.Core.ECS;
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
            EnsureCoreReference(csprojPath);
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

    private static void EnsureCoreReference(string csprojPath)
    {
        if (!File.Exists(csprojPath))
            return;

        var projectDir = Path.GetDirectoryName(Path.GetFullPath(csprojPath));
        if (string.IsNullOrWhiteSpace(projectDir))
            return;

        XDocument doc;
        try
        {
            doc = XDocument.Load(csprojPath, LoadOptions.PreserveWhitespace);
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"Failed to inspect project file for engine reference: {ex.Message}");
            return;
        }

        var root = doc.Root;
        if (root == null)
            return;

        var ns = root.Name.Namespace;
        bool IsElement(XElement e, string name) => e.Name.LocalName.Equals(name, StringComparison.Ordinal);

        bool changed = false;
        bool hasValidProjectReference = false;
        var referencesToRemove = new List<XElement>();

        foreach (var projectReference in root.Descendants().Where(e => IsElement(e, "ProjectReference")))
        {
            var includeAttr = projectReference.Attribute("Include")?.Value;
            if (string.IsNullOrWhiteSpace(includeAttr))
                continue;
            if (!includeAttr.Contains("FrinkyEngine.Core.csproj", StringComparison.OrdinalIgnoreCase))
                continue;

            var absoluteReference = Path.GetFullPath(Path.Combine(projectDir, includeAttr));
            if (File.Exists(absoluteReference))
            {
                hasValidProjectReference = true;
            }
            else
            {
                referencesToRemove.Add(projectReference);
                FrinkyLog.Warning($"Removed missing FrinkyEngine.Core project reference: {includeAttr}");
            }
        }

        foreach (var reference in referencesToRemove)
        {
            reference.Remove();
            changed = true;
        }

        bool hasValidAssemblyReference = false;
        foreach (var assemblyReference in root.Descendants().Where(e => IsElement(e, "Reference")))
        {
            var includeAttr = assemblyReference.Attribute("Include")?.Value;
            if (!string.Equals(includeAttr, "FrinkyEngine.Core", StringComparison.OrdinalIgnoreCase))
                continue;

            var hintPath = assemblyReference.Elements().FirstOrDefault(e => IsElement(e, "HintPath"))?.Value;
            if (string.IsNullOrWhiteSpace(hintPath))
                continue;

            var absoluteHintPath = Path.GetFullPath(Path.Combine(projectDir, hintPath));
            if (File.Exists(absoluteHintPath))
            {
                hasValidAssemblyReference = true;
                break;
            }
        }

        if (!hasValidProjectReference && !hasValidAssemblyReference)
        {
            var localEngineDir = Path.Combine(projectDir, ".frinky", "engine");
            if (ProjectScaffolder.CopyCoreAssemblyFiles(localEngineDir))
            {
                var localCoreAssembly = Path.Combine(localEngineDir, "FrinkyEngine.Core.dll");
                var relativeHintPath = Path.GetRelativePath(projectDir, localCoreAssembly).Replace('\\', '/');
                EnsureAssemblyReference(root, ns, relativeHintPath);
                changed = true;
                FrinkyLog.Info("Added local FrinkyEngine.Core assembly reference for script build.");
            }
            else
            {
                FrinkyLog.Warning("FrinkyEngine.Core.dll not found; could not auto-fix script project reference.");
            }
        }

        if (changed)
        {
            doc.Save(csprojPath);
        }
    }

    private static void EnsureAssemblyReference(XElement root, XNamespace ns, string hintPath)
    {
        bool IsElement(XElement e, string name) => e.Name.LocalName.Equals(name, StringComparison.Ordinal);

        var existing = root.Descendants()
            .FirstOrDefault(e =>
                IsElement(e, "Reference") &&
                string.Equals(e.Attribute("Include")?.Value, "FrinkyEngine.Core", StringComparison.OrdinalIgnoreCase));

        if (existing == null)
        {
            var itemGroup = root.Elements().FirstOrDefault(e => IsElement(e, "ItemGroup")) ?? new XElement(ns + "ItemGroup");
            if (itemGroup.Parent == null)
                root.Add(itemGroup);

            existing = new XElement(ns + "Reference", new XAttribute("Include", "FrinkyEngine.Core"));
            itemGroup.Add(existing);
        }

        var hint = existing.Elements().FirstOrDefault(e => IsElement(e, "HintPath"));
        if (hint == null)
        {
            hint = new XElement(ns + "HintPath");
            existing.Add(hint);
        }
        hint.Value = hintPath;

        var privateNode = existing.Elements().FirstOrDefault(e => IsElement(e, "Private"));
        if (privateNode == null)
        {
            privateNode = new XElement(ns + "Private");
            existing.Add(privateNode);
        }
        privateNode.Value = "false";
    }
}
