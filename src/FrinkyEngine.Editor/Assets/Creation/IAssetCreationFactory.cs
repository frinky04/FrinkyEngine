using FrinkyEngine.Core.Assets;

namespace FrinkyEngine.Editor.Assets.Creation;

public interface IAssetCreationFactory
{
    string Id { get; }
    string DisplayName { get; }
    string NameHint { get; }
    string Extension { get; }
    AssetType AssetType { get; }

    void Reset(EditorApplication app);
    void DrawOptions(EditorApplication app);
    bool TryValidateName(string name, out string? validationMessage);
    string BuildRelativePath(string name);
    bool TryCreate(EditorApplication app, string name, out string createdRelativePath, out string? errorMessage);
}
