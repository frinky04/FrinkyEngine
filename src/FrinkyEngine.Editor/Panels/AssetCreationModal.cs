using System.Numerics;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Editor.Assets.Creation;
using Hexa.NET.ImGui;

namespace FrinkyEngine.Editor.Panels;

public sealed class AssetCreationModal
{
    private const string PopupTitle = "CreateAsset";

    private readonly EditorApplication _app;
    private bool _openPopup;
    private string? _pendingFactoryId;
    private string? _selectedFactoryId;
    private string _assetName = string.Empty;

    public AssetCreationModal(EditorApplication app)
    {
        _app = app;
    }

    public void Open(string? factoryId = null)
    {
        _assetName = string.Empty;
        _pendingFactoryId = factoryId;
        _openPopup = true;
    }

    public void Draw()
    {
        if (_openPopup)
        {
            ImGui.OpenPopup(PopupTitle);
            _openPopup = false;
        }

        var viewport = ImGui.GetMainViewport();
        var center = new Vector2(viewport.WorkPos.X + viewport.WorkSize.X * 0.5f,
            viewport.WorkPos.Y + viewport.WorkSize.Y * 0.5f);
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(520, 0), ImGuiCond.Appearing);

        if (!ImGui.BeginPopupModal(PopupTitle, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        var factories = AssetCreationRegistry.GetFactories();
        if (factories.Count == 0)
        {
            ImGui.TextDisabled("No asset factories registered.");
            if (ImGui.Button("Close"))
                ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        var selectedFactory = ResolveSelectedFactory(factories);
        if (selectedFactory == null)
        {
            ImGui.EndPopup();
            return;
        }

        DrawFactoryPicker(factories, ref selectedFactory);

        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##assetName", selectedFactory.NameHint, ref _assetName, 256);
        ImGui.Spacing();

        selectedFactory.DrawOptions(_app);

        var normalizedName = NormalizeName(_assetName, selectedFactory.Extension);
        var hasProject = !string.IsNullOrWhiteSpace(_app.ProjectDirectory)
            && !string.IsNullOrWhiteSpace(AssetManager.Instance.AssetsPath);
        var nameValid = selectedFactory.TryValidateName(normalizedName, out string? validationMessage);
        var relativePath = nameValid ? selectedFactory.BuildRelativePath(normalizedName) : string.Empty;
        var projectRelativePreview = string.IsNullOrEmpty(relativePath) ? string.Empty : $"Assets/{relativePath}";
        bool fileExists = nameValid && hasProject && File.Exists(GetFullPath(relativePath));

        ImGui.Spacing();
        if (!string.IsNullOrEmpty(projectRelativePreview))
            ImGui.TextDisabled($"File: {projectRelativePreview}");

        if (!string.IsNullOrWhiteSpace(_assetName) && !nameValid && !string.IsNullOrWhiteSpace(validationMessage))
            DrawValidationMessage(validationMessage);
        else if (fileExists)
            DrawValidationMessage("A file with this name already exists.");

        var canCreate = hasProject
            && nameValid
            && !fileExists
            && !string.IsNullOrWhiteSpace(relativePath);

        ImGui.Spacing();

        var style = ImGui.GetStyle();
        var createBtnWidth = ImGui.CalcTextSize("Create").X + style.FramePadding.X * 2 + 16;
        var cancelBtnWidth = ImGui.CalcTextSize("Cancel").X + style.FramePadding.X * 2 + 16;
        var totalWidth = cancelBtnWidth + style.ItemSpacing.X + createBtnWidth;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - totalWidth);

        if (ImGui.Button("Cancel", new Vector2(cancelBtnWidth, 0)))
            ImGui.CloseCurrentPopup();

        ImGui.SameLine();

        ImGui.BeginDisabled(!canCreate);
        if (ImGui.Button("Create", new Vector2(createBtnWidth, 0)))
        {
            if (selectedFactory.TryCreate(_app, normalizedName, out var createdRelativePath, out string? errorMessage))
            {
                AssetDatabase.Instance.Refresh();
                _app.AssetIcons.OnAssetDatabaseRefreshed(changedRelativePaths: null);

                FrinkyLog.Info($"Created {selectedFactory.DisplayName} asset: {createdRelativePath}");
                NotificationManager.Instance.Post($"Created: {Path.GetFileName(createdRelativePath)}", NotificationType.Success);

                ImGui.CloseCurrentPopup();
            }
            else
            {
                NotificationManager.Instance.Post(errorMessage ?? "Failed to create asset.", NotificationType.Error);
            }
        }
        ImGui.EndDisabled();

        ImGui.EndPopup();
    }

    private IAssetCreationFactory? ResolveSelectedFactory(IReadOnlyList<IAssetCreationFactory> factories)
    {
        if (!string.IsNullOrWhiteSpace(_pendingFactoryId))
        {
            _selectedFactoryId = AssetCreationRegistry.GetFactory(_pendingFactoryId) != null
                ? _pendingFactoryId
                : factories[0].Id;
            _pendingFactoryId = null;
            var resetFactory = AssetCreationRegistry.GetFactory(_selectedFactoryId);
            resetFactory?.Reset(_app);
        }

        if (string.IsNullOrWhiteSpace(_selectedFactoryId) || AssetCreationRegistry.GetFactory(_selectedFactoryId) == null)
        {
            _selectedFactoryId = factories[0].Id;
            factories[0].Reset(_app);
        }

        return AssetCreationRegistry.GetFactory(_selectedFactoryId);
    }

    private void DrawFactoryPicker(IReadOnlyList<IAssetCreationFactory> factories, ref IAssetCreationFactory selectedFactory)
    {
        if (!ImGui.BeginCombo("Type", selectedFactory.DisplayName))
            return;

        foreach (var factory in factories)
        {
            var isSelected = string.Equals(factory.Id, _selectedFactoryId, StringComparison.OrdinalIgnoreCase);
            if (ImGui.Selectable(factory.DisplayName, isSelected))
            {
                _selectedFactoryId = factory.Id;
                selectedFactory = factory;
                selectedFactory.Reset(_app);
            }
        }

        ImGui.EndCombo();
    }

    private static string NormalizeName(string rawName, string extension)
    {
        var normalized = rawName.Trim();
        if (normalized.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            normalized = normalized[..^extension.Length];
        return normalized.Trim();
    }

    private static void DrawValidationMessage(string message)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0.3f, 0.3f, 1));
        ImGui.TextWrapped(message);
        ImGui.PopStyleColor();
    }

    private static string GetFullPath(string relativePath)
    {
        var assetsPath = AssetManager.Instance.AssetsPath ?? string.Empty;
        return Path.Combine(assetsPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
