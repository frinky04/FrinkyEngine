using System.Numerics;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Core.Scene;
using ImGuiNET;
using Raylib_cs;

namespace FrinkyEngine.Editor.Panels;

public class AssetBrowserPanel
{
    private readonly EditorApplication _app;
    private string _currentDir = "";
    private string _searchQuery = "";
    private bool _flatView = true;
    private int _filterIndex; // 0=All, 1=Model, 2=Scene, 3=Texture, 4=Script
    private static readonly string[] FilterNames = { "All", "Models", "Scenes", "Textures", "Scripts" };
    private static readonly AssetType?[] FilterTypes = { null, AssetType.Model, AssetType.Scene, AssetType.Texture, AssetType.Script };

    public AssetBrowserPanel(EditorApplication app)
    {
        _app = app;
    }

    public void Draw()
    {
        if (!ImGui.Begin("Assets"))
        {
            ImGui.End();
            return;
        }

        DrawToolbar();
        if (!_flatView)
            DrawBreadcrumb();
        ImGui.Separator();

        var filter = FilterTypes[_filterIndex];
        var isSearching = !string.IsNullOrWhiteSpace(_searchQuery);

        if (isSearching || _flatView)
            DrawSearchResults(filter);
        else
            DrawDirectoryContents(filter);

        ImGui.End();
    }

    private void DrawToolbar()
    {
        if (ImGui.Button("Refresh"))
            AssetDatabase.Instance.Refresh();

        ImGui.SameLine();
        ImGui.Checkbox("Flat", ref _flatView);

        ImGui.SameLine();
        ImGui.SetNextItemWidth(80);
        float iconScale = EditorIcons.IconScale;
        if (ImGui.SliderFloat("##IconSize", ref iconScale, 0.5f, 3.0f, "%.1fx"))
            EditorIcons.IconScale = iconScale;
        if (ImGui.IsItemDeactivatedAfterEdit())
            EditorPreferences.Instance.SaveConfig();

        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        ImGui.Combo("##Filter", ref _filterIndex, FilterNames, FilterNames.Length);

        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##Search", "Search assets...", ref _searchQuery, 256);
    }

    private void DrawBreadcrumb()
    {
        if (ImGui.SmallButton("Assets"))
        {
            _currentDir = "";
        }

        if (!string.IsNullOrEmpty(_currentDir))
        {
            var parts = _currentDir.Split('/');
            var path = "";
            for (int i = 0; i < parts.Length; i++)
            {
                ImGui.SameLine();
                ImGui.TextDisabled("/");
                ImGui.SameLine();

                path = i == 0 ? parts[i] : path + "/" + parts[i];
                var thisPath = path; // capture for closure
                if (ImGui.SmallButton(parts[i] + "##bc" + i))
                {
                    _currentDir = thisPath;
                }
            }
        }
    }

    private void DrawDirectoryContents(AssetType? filter)
    {
        var db = AssetDatabase.Instance;

        // Subdirectories
        foreach (var dir in db.GetSubdirectories(_currentDir))
        {
            var label = "[DIR] " + dir;
            if (ImGui.Selectable(label, false, ImGuiSelectableFlags.AllowDoubleClick))
            {
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    _currentDir = string.IsNullOrEmpty(_currentDir) ? dir : _currentDir + "/" + dir;
                }
            }
        }

        // Files
        foreach (var asset in db.GetAssetsInDirectory(_currentDir, filter))
        {
            ImGui.PushID(asset.RelativePath);
            DrawAssetRow(asset, asset.FileName);
            ImGui.PopID();
        }
    }

    private void DrawSearchResults(AssetType? filter)
    {
        var db = AssetDatabase.Instance;
        var query = _searchQuery.Trim();

        foreach (var asset in db.GetAssets(filter))
        {
            if (!asset.RelativePath.Contains(query, StringComparison.OrdinalIgnoreCase)
                && !asset.FileName.Contains(query, StringComparison.OrdinalIgnoreCase))
                continue;

            ImGui.PushID(asset.RelativePath);
            DrawAssetRow(asset, asset.RelativePath);
            ImGui.PopID();
        }
    }

    private void HandleDoubleClick(AssetEntry asset)
    {
        switch (asset.Type)
        {
            case AssetType.Scene:
                OpenSceneAsset(asset);
                break;

            case AssetType.Model:
                AssignModelToSelected(asset);
                break;
        }
    }

    private void AssignModelToSelected(AssetEntry asset)
    {
        var entity = _app.SelectedEntity;
        if (entity == null) return;

        var mr = entity.GetComponent<MeshRendererComponent>();
        if (mr == null) return;

        mr.ModelPath = asset.RelativePath;
        FrinkyLog.Info($"Assigned model '{asset.FileName}' to {entity.Name}");
    }

    private void DrawContextMenu(AssetEntry asset)
    {
        if (!ImGui.BeginPopupContextItem(asset.RelativePath))
            return;

        if (asset.Type == AssetType.Scene && ImGui.MenuItem("Open Scene"))
        {
            OpenSceneAsset(asset);
        }

        if (asset.Type == AssetType.Model && ImGui.MenuItem("Assign to MeshRenderer"))
        {
            AssignModelToSelected(asset);
        }

        if (ImGui.MenuItem("Copy Path"))
        {
            ImGui.SetClipboardText(asset.RelativePath);
        }

        ImGui.EndPopup();
    }

    private void OpenSceneAsset(AssetEntry asset)
    {
        var fullPath = AssetManager.Instance.ResolvePath(asset.RelativePath);
        SceneManager.Instance.LoadScene(fullPath);
        _app.CurrentScene = SceneManager.Instance.ActiveScene;
        _app.ClearSelection();
        _app.RestoreEditorCameraFromScene();
        _app.UpdateWindowTitle();
        _app.UndoRedo.Clear();
        _app.UndoRedo.SetBaseline(_app.CurrentScene, _app.GetSelectedEntityIds());
        FrinkyLog.Info($"Opened scene: {asset.RelativePath}");
    }

    private void DrawAssetRow(AssetEntry asset, string displayLabel)
    {
        float iconSize = DrawInlineIcon(asset.Type);

        var textPos = ImGui.GetCursorPos();
        bool clicked = ImGui.Selectable("##sel", false, ImGuiSelectableFlags.AllowDoubleClick, new Vector2(0, iconSize));

        if (clicked && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            HandleDoubleClick(asset);

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(asset.RelativePath);

        DrawContextMenu(asset);

        // Overlay vertically centered text
        var afterPos = ImGui.GetCursorPos();
        float textH = ImGui.GetTextLineHeight();
        ImGui.SetCursorPos(new Vector2(textPos.X, textPos.Y + Math.Max(0f, (iconSize - textH) * 0.5f)));
        ImGui.TextUnformatted(displayLabel);
        ImGui.SetCursorPos(afterPos);
        ImGui.Dummy(Vector2.Zero);
    }

    private static float DrawInlineIcon(AssetType type)
    {
        float size = EditorIcons.GetIconSize();
        var icon = EditorIcons.GetIcon(type);
        if (icon is Texture2D tex)
        {
            ImGui.Image((nint)tex.Id, new Vector2(size, size));
            ImGui.SameLine(0, 4);
        }
        return size;
    }
}
