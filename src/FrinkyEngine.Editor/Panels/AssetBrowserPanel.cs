using System.Diagnostics;
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
    private readonly record struct BrowserItem(
        string Id,
        string Label,
        string Tooltip,
        bool IsDirectory,
        string? RelativeDirectory,
        AssetEntry? Asset);

    private readonly EditorApplication _app;
    private string _currentDir = string.Empty;
    private string _searchQuery = string.Empty;
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
        var items = (isSearching || _flatView)
            ? BuildSearchItems(filter)
            : BuildDirectoryItems(filter);

        if (items.Count == 0)
        {
            ImGui.TextDisabled("No assets found.");
        }
        else if (EditorPreferences.Instance.AssetBrowserGridView)
        {
            DrawItemsGrid(items);
        }
        else
        {
            DrawItemsList(items);
        }

        ImGui.End();
    }

    private void DrawToolbar()
    {
        if (ImGui.Button("Refresh"))
            AssetDatabase.Instance.Refresh();

        ImGui.SameLine();
        ImGui.Checkbox("Flat", ref _flatView);

        ImGui.SameLine();
        bool isGrid = EditorPreferences.Instance.AssetBrowserGridView;
        if (ImGui.RadioButton("Grid", isGrid))
            SetGridViewMode(true);

        ImGui.SameLine();
        if (ImGui.RadioButton("List", !isGrid))
            SetGridViewMode(false);

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

    private static void SetGridViewMode(bool gridView)
    {
        if (EditorPreferences.Instance.AssetBrowserGridView == gridView)
            return;

        EditorPreferences.Instance.AssetBrowserGridView = gridView;
        EditorPreferences.Instance.SaveConfig();
    }

    private void DrawBreadcrumb()
    {
        if (ImGui.SmallButton("Assets"))
            _currentDir = string.Empty;

        if (string.IsNullOrEmpty(_currentDir))
            return;

        var parts = _currentDir.Split('/');
        var path = string.Empty;
        for (int i = 0; i < parts.Length; i++)
        {
            ImGui.SameLine();
            ImGui.TextDisabled("/");
            ImGui.SameLine();

            path = i == 0 ? parts[i] : path + "/" + parts[i];
            var thisPath = path;
            if (ImGui.SmallButton(parts[i] + "##bc" + i))
                _currentDir = thisPath;
        }
    }

    private List<BrowserItem> BuildDirectoryItems(AssetType? filter)
    {
        var db = AssetDatabase.Instance;
        var items = new List<BrowserItem>();

        foreach (var dir in db.GetSubdirectories(_currentDir))
        {
            var relativeDir = string.IsNullOrEmpty(_currentDir) ? dir : _currentDir + "/" + dir;
            items.Add(new BrowserItem(
                Id: "dir:" + relativeDir,
                Label: dir,
                Tooltip: relativeDir,
                IsDirectory: true,
                RelativeDirectory: relativeDir,
                Asset: null));
        }

        foreach (var asset in db.GetAssetsInDirectory(_currentDir, filter))
        {
            items.Add(new BrowserItem(
                Id: "asset:" + asset.RelativePath,
                Label: asset.FileName,
                Tooltip: asset.RelativePath,
                IsDirectory: false,
                RelativeDirectory: null,
                Asset: asset));
        }

        return items;
    }

    private List<BrowserItem> BuildSearchItems(AssetType? filter)
    {
        var db = AssetDatabase.Instance;
        var query = _searchQuery.Trim();
        var hasQuery = !string.IsNullOrEmpty(query);
        var items = new List<BrowserItem>();

        foreach (var asset in db.GetAssets(filter))
        {
            if (hasQuery
                && !asset.RelativePath.Contains(query, StringComparison.OrdinalIgnoreCase)
                && !asset.FileName.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            items.Add(new BrowserItem(
                Id: "asset:" + asset.RelativePath,
                Label: asset.FileName,
                Tooltip: asset.RelativePath,
                IsDirectory: false,
                RelativeDirectory: null,
                Asset: asset));
        }

        return items;
    }

    private void DrawItemsList(IReadOnlyList<BrowserItem> items)
    {
        foreach (var item in items)
        {
            ImGui.PushID(item.Id);
            DrawListItem(item);
            ImGui.PopID();
        }
    }

    private void DrawListItem(BrowserItem item)
    {
        float iconSize = EditorIcons.GetIconSize();
        float rowHeight = Math.Max(iconSize, ImGui.GetTextLineHeight()) + 4f;

        bool clicked = ImGui.Selectable("##list", false, ImGuiSelectableFlags.AllowDoubleClick, new Vector2(0, rowHeight));
        if (clicked && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            HandleItemDoubleClick(item);

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(item.Tooltip);

        DrawItemContextMenu(item);

        var min = ImGui.GetItemRectMin();
        float iconY = min.Y + Math.Max(0f, (rowHeight - iconSize) * 0.5f);
        DrawItemIcon(item, new Vector2(min.X + 3f, iconY), iconSize);

        var drawList = ImGui.GetWindowDrawList();
        float textY = min.Y + Math.Max(0f, (rowHeight - ImGui.GetTextLineHeight()) * 0.5f);
        drawList.AddText(new Vector2(min.X + iconSize + 9f, textY), ImGui.GetColorU32(ImGuiCol.Text), item.Label);
    }

    private void DrawItemsGrid(IReadOnlyList<BrowserItem> items)
    {
        float baseIconSize = EditorIcons.GetIconSize();
        float iconSize = Math.Max(24f, baseIconSize * 2.2f);
        float tileWidth = Math.Max(110f, iconSize + 32f);
        float tileHeight = iconSize + ImGui.GetTextLineHeightWithSpacing() * 2f + 14f;
        float spacing = ImGui.GetStyle().ItemSpacing.X;
        float available = Math.Max(tileWidth, ImGui.GetContentRegionAvail().X);
        int columns = Math.Max(1, (int)(available / (tileWidth + spacing)));

        if (!ImGui.BeginTable("##AssetGrid", columns, ImGuiTableFlags.SizingFixedFit))
            return;

        for (int i = 0; i < items.Count; i++)
        {
            if (i % columns == 0)
                ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(i % columns);

            var item = items[i];
            ImGui.PushID(item.Id);
            DrawGridItem(item, tileWidth, tileHeight, iconSize);
            ImGui.PopID();
        }

        ImGui.EndTable();
    }

    private void DrawGridItem(BrowserItem item, float tileWidth, float tileHeight, float iconSize)
    {
        bool clicked = ImGui.Selectable("##tile", false, ImGuiSelectableFlags.AllowDoubleClick, new Vector2(tileWidth, tileHeight));
        if (clicked && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            HandleItemDoubleClick(item);

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(item.Tooltip);

        DrawItemContextMenu(item);

        var min = ImGui.GetItemRectMin();
        var drawList = ImGui.GetWindowDrawList();

        float iconX = min.X + (tileWidth - iconSize) * 0.5f;
        float iconY = min.Y + 8f;
        DrawItemIcon(item, new Vector2(iconX, iconY), iconSize);

        string clipped = ClipLabel(item.Label, tileWidth - 10f);
        var textSize = ImGui.CalcTextSize(clipped);
        float textX = min.X + Math.Max(5f, (tileWidth - textSize.X) * 0.5f);
        float textY = min.Y + tileHeight - ImGui.GetTextLineHeightWithSpacing() - 4f;
        drawList.AddText(new Vector2(textX, textY), ImGui.GetColorU32(ImGuiCol.Text), clipped);
    }

    private static string ClipLabel(string label, float maxWidth)
    {
        if (ImGui.CalcTextSize(label).X <= maxWidth)
            return label;

        const string ellipsis = "...";
        var clipped = label;
        while (clipped.Length > 1 && ImGui.CalcTextSize(clipped + ellipsis).X > maxWidth)
            clipped = clipped[..^1];

        return clipped + ellipsis;
    }

    private void HandleItemDoubleClick(BrowserItem item)
    {
        if (item.IsDirectory && item.RelativeDirectory != null)
        {
            OpenDirectoryExternally(item.RelativeDirectory);
            return;
        }

        if (item.Asset != null)
            HandleAssetDoubleClick(item.Asset);
    }

    private void HandleAssetDoubleClick(AssetEntry asset)
    {
        switch (asset.Type)
        {
            case AssetType.Scene:
                OpenSceneAsset(asset);
                break;
            case AssetType.Script:
                OpenScriptAsset(asset);
                break;
            default:
                OpenAssetExternally(asset);
                break;
        }
    }

    private void OpenScriptAsset(AssetEntry asset)
    {
        var fullPath = AssetManager.Instance.ResolvePath(asset.RelativePath);
        _app.OpenFileInVSCode(fullPath);
    }

    private void OpenAssetExternally(AssetEntry asset)
    {
        var fullPath = AssetManager.Instance.ResolvePath(asset.RelativePath);
        OpenPathWithDefaultProgram(fullPath, $"asset '{asset.RelativePath}'");
    }

    private void OpenDirectoryExternally(string relativeDir)
    {
        var fullPath = AssetManager.Instance.AssetsPath;
        if (!string.IsNullOrEmpty(relativeDir))
            fullPath = Path.Combine(fullPath, relativeDir.Replace('/', Path.DirectorySeparatorChar));

        OpenPathWithDefaultProgram(fullPath, $"folder '{relativeDir}'");
    }

    private static void OpenPathWithDefaultProgram(string absolutePath, string description)
    {
        try
        {
            if (!File.Exists(absolutePath) && !Directory.Exists(absolutePath))
            {
                FrinkyLog.Warning($"Cannot open {description}: path not found ({absolutePath})");
                NotificationManager.Instance.Post("Path not found.", NotificationType.Warning);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = absolutePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"Failed to open {description}: {ex.Message}");
            NotificationManager.Instance.Post("Failed to open path.", NotificationType.Error);
        }
    }

    private void AssignModelToSelected(AssetEntry asset)
    {
        var entity = _app.SelectedEntity;
        if (entity == null)
            return;

        var mr = entity.GetComponent<MeshRendererComponent>();
        if (mr == null)
            return;

        mr.ModelPath = asset.RelativePath;
        FrinkyLog.Info($"Assigned model '{asset.FileName}' to {entity.Name}");
    }

    private void DrawItemContextMenu(BrowserItem item)
    {
        if (!ImGui.BeginPopupContextItem())
            return;

        if (item.IsDirectory && item.RelativeDirectory != null)
        {
            if (ImGui.MenuItem("Browse Folder"))
                _currentDir = item.RelativeDirectory;

            if (ImGui.MenuItem("Open in File Explorer"))
                OpenDirectoryExternally(item.RelativeDirectory);

            if (ImGui.MenuItem("Copy Path"))
                ImGui.SetClipboardText(item.RelativeDirectory);
        }
        else if (item.Asset != null)
        {
            DrawAssetContextMenu(item.Asset);
        }

        ImGui.EndPopup();
    }

    private void DrawAssetContextMenu(AssetEntry asset)
    {
        if (asset.Type == AssetType.Scene && ImGui.MenuItem("Open Scene"))
            OpenSceneAsset(asset);

        if (asset.Type == AssetType.Script && ImGui.MenuItem("Open in VS Code"))
            OpenScriptAsset(asset);

        if (asset.Type == AssetType.Model && ImGui.MenuItem("Assign to MeshRenderer"))
            AssignModelToSelected(asset);

        if (ImGui.MenuItem("Open in Default Program"))
            OpenAssetExternally(asset);

        if (ImGui.MenuItem("Copy Path"))
            ImGui.SetClipboardText(asset.RelativePath);
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

    private static void DrawItemIcon(BrowserItem item, Vector2 min, float size)
    {
        Texture2D? icon = item.IsDirectory
            ? EditorIcons.GetFolderIcon() ?? EditorIcons.GetIcon(AssetType.Unknown)
            : item.Asset != null ? EditorIcons.GetIcon(item.Asset.Type) : EditorIcons.GetIcon(AssetType.Unknown);

        if (icon is Texture2D tex)
        {
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddImage((nint)tex.Id, min, new Vector2(min.X + size, min.Y + size));
        }
    }
}
