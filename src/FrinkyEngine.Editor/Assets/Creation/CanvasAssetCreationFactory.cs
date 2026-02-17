using FrinkyEngine.Core.Assets;
using Hexa.NET.ImGui;

namespace FrinkyEngine.Editor.Assets.Creation;

public sealed class CanvasAssetCreationFactory : IAssetCreationFactory
{
    private const string DefaultCanvasTemplate = """
        <Panel class="root" style="width: 100%; height: 100%; padding: 16px;">
            <Label text="New Canvas UI" />
        </Panel>
        """;

    public string Id => "canvas";
    public string DisplayName => "Canvas";
    public string NameHint => "Canvas Name";
    public string Extension => ".canvas";
    public AssetType AssetType => AssetType.Canvas;

    public void Reset(EditorApplication app)
    {
    }

    public void DrawOptions(EditorApplication app)
    {
        ImGui.TextDisabled("Creates a CanvasUI markup asset (.canvas).");
    }

    public bool TryValidateName(string name, out string? validationMessage)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            validationMessage = "Name is required.";
            return false;
        }

        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.Contains('/') || name.Contains('\\'))
        {
            validationMessage = "Name contains invalid file characters.";
            return false;
        }

        validationMessage = null;
        return true;
    }

    public string BuildRelativePath(string name)
    {
        return $"{name}{Extension}";
    }

    public bool TryCreate(EditorApplication app, string name, out string createdRelativePath, out string? errorMessage)
    {
        createdRelativePath = BuildRelativePath(name);
        errorMessage = null;

        var assetsPath = AssetManager.Instance.AssetsPath;
        if (string.IsNullOrWhiteSpace(assetsPath))
        {
            errorMessage = "Open a project first.";
            return false;
        }

        var fullPath = Path.Combine(assetsPath, createdRelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            errorMessage = "File already exists.";
            return false;
        }

        try
        {
            Directory.CreateDirectory(assetsPath);
            File.WriteAllText(fullPath, DefaultCanvasTemplate);
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to create canvas file: {ex.Message}";
            return false;
        }
    }
}
