using System.Diagnostics;
using System.Globalization;
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
    public const string AssetDragPayload = "FRINKY_ASSET_BROWSER";
    private readonly record struct BrowserItem(
        string Id,
        string Label,
        string Tooltip,
        AssetEntry Asset);

    private readonly EditorApplication _app;
    private string _searchQuery = string.Empty;
    private int _filterIndex; // 0=All, 1=Model, 2=Scene, 3=Texture, 4=Script, 5=Prefab
    private static readonly string[] FilterNames = { "All", "Models", "Scenes", "Textures", "Scripts", "Prefabs" };
    private static readonly AssetType?[] FilterTypes = { null, AssetType.Model, AssetType.Scene, AssetType.Texture, AssetType.Script, AssetType.Prefab };

    // Multi-selection state
    private readonly HashSet<string> _selectedAssets = new(StringComparer.OrdinalIgnoreCase);
    private string? _selectionAnchor;

    // Tag filter state
    private readonly HashSet<string> _activeTagFilters = new(StringComparer.OrdinalIgnoreCase);

    // Tag manager state
    private string _newTagName = string.Empty;
    private Vector3 _newTagColor = new(0.3f, 0.6f, 1.0f);
    private bool _openTagManager;
    private string? _confirmDeleteTag;

    // Cached item list for range selection
    private List<BrowserItem> _lastItems = new();

    public AssetBrowserPanel(EditorApplication app)
    {
        _app = app;
    }

    public unsafe void Draw()
    {
        // Clear dragged asset path when no drag is active
        var activePayload = ImGui.GetDragDropPayload();
        if (activePayload.NativePtr == null)
            _app.DraggedAssetPath = null;

        if (!ImGui.Begin("Assets"))
        {
            ImGui.End();
            return;
        }

        DrawToolbar();
        DrawTagFilterBar();
        ImGui.Separator();

        var items = BuildFilteredItems();
        _lastItems = items;

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

        // Click empty area to clear selection
        if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows) &&
            !ImGui.IsAnyItemHovered() &&
            ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            _selectedAssets.Clear();
            _selectionAnchor = null;
        }

        DrawTagManagerModal();
        ImGui.End();
    }

    private void DrawToolbar()
    {
        if (ImGui.Button("Refresh"))
            AssetDatabase.Instance.Refresh();

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
        if (ImGui.Button("Tag Manager"))
            _openTagManager = true;

        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##Search", "Search assets...", ref _searchQuery, 256);
    }

    private void DrawTagFilterBar()
    {
        var tagDb = _app.TagDatabase;
        if (tagDb == null)
            return;

        // Active filter chips
        var toRemove = new List<string>();
        foreach (var tagName in _activeTagFilters)
        {
            var tag = tagDb.GetAllTags().FirstOrDefault(t => string.Equals(t.Name, tagName, StringComparison.OrdinalIgnoreCase));
            if (tag == null)
            {
                toRemove.Add(tagName);
                continue;
            }

            var color = ParseHexColor(tag.Color);
            ImGui.PushStyleColor(ImGuiCol.Button, color);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, color * new Vector4(1.2f, 1.2f, 1.2f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, color * new Vector4(0.8f, 0.8f, 0.8f, 1f));

            // Use contrasting text color
            float luminance = color.X * 0.299f + color.Y * 0.587f + color.Z * 0.114f;
            var textColor = luminance > 0.5f ? new Vector4(0, 0, 0, 1) : new Vector4(1, 1, 1, 1);
            ImGui.PushStyleColor(ImGuiCol.Text, textColor);

            if (ImGui.SmallButton(tag.Name + " x##tagfilter_" + tag.Name))
                toRemove.Add(tagName);

            ImGui.PopStyleColor(4);
            ImGui.SameLine();
        }

        foreach (var name in toRemove)
            _activeTagFilters.Remove(name);

        // "+" button to add tag filters
        if (ImGui.SmallButton("+##addtagfilter"))
            ImGui.OpenPopup("AddTagFilterPopup");

        if (ImGui.BeginPopup("AddTagFilterPopup"))
        {
            var allTags = tagDb.GetAllTags();
            if (allTags.Count == 0)
            {
                ImGui.TextDisabled("No tags defined.");
            }
            else
            {
                foreach (var tag in allTags)
                {
                    if (_activeTagFilters.Contains(tag.Name))
                        continue;

                    var color = ParseHexColor(tag.Color);
                    var drawList = ImGui.GetWindowDrawList();
                    var cursor = ImGui.GetCursorScreenPos();
                    drawList.AddRectFilled(cursor, cursor + new Vector2(12, 12), ImGuiColorToU32(color), 2f);
                    ImGui.Dummy(new Vector2(12, 12));
                    ImGui.SameLine();

                    if (ImGui.MenuItem(tag.Name))
                        _activeTagFilters.Add(tag.Name);
                }
            }

            ImGui.EndPopup();
        }

        if (_activeTagFilters.Count > 0)
        {
            ImGui.SameLine();
            if (ImGui.SmallButton("Clear"))
                _activeTagFilters.Clear();
        }
    }

    private static void SetGridViewMode(bool gridView)
    {
        if (EditorPreferences.Instance.AssetBrowserGridView == gridView)
            return;

        EditorPreferences.Instance.AssetBrowserGridView = gridView;
        EditorPreferences.Instance.SaveConfig();
    }

    private List<BrowserItem> BuildFilteredItems()
    {
        var db = AssetDatabase.Instance;
        var tagDb = _app.TagDatabase;
        var filter = FilterTypes[_filterIndex];
        var query = _searchQuery.Trim();
        var hasQuery = !string.IsNullOrEmpty(query);
        var items = new List<BrowserItem>();

        // Build set of assets that pass tag filter
        HashSet<string>? tagFilteredAssets = null;
        if (_activeTagFilters.Count > 0 && tagDb != null)
        {
            tagFilteredAssets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var tagName in _activeTagFilters)
            {
                foreach (var assetPath in tagDb.GetAssetsWithTag(tagName))
                    tagFilteredAssets.Add(assetPath);
            }
        }

        foreach (var asset in db.GetAssets(filter))
        {
            // Search filter — matches path, filename, or any tag name
            if (hasQuery
                && !asset.RelativePath.Contains(query, StringComparison.OrdinalIgnoreCase)
                && !asset.FileName.Contains(query, StringComparison.OrdinalIgnoreCase)
                && !AssetMatchesTagSearch(tagDb, asset.RelativePath, query))
            {
                continue;
            }

            // Tag filter (OR logic)
            if (tagFilteredAssets != null && !tagFilteredAssets.Contains(asset.RelativePath))
                continue;

            items.Add(new BrowserItem(
                Id: asset.RelativePath,
                Label: asset.FileName,
                Tooltip: asset.RelativePath,
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
        float ribbonHeight = 2f;
        var tags = _app.TagDatabase?.GetTagsForAsset(item.Asset.RelativePath);
        bool hasTags = tags != null && tags.Count > 0;
        float rowHeight = Math.Max(iconSize, ImGui.GetTextLineHeight()) + 4f;

        bool isSelected = _selectedAssets.Contains(item.Id);
        bool clicked = ImGui.Selectable("##list", isSelected, ImGuiSelectableFlags.AllowDoubleClick, new Vector2(0, rowHeight));

        if (clicked)
        {
            HandleItemClick(item);
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                HandleAssetDoubleClick(item.Asset);
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(item.Tooltip);

        DrawAssetDragDropSource(item);
        DrawItemContextMenu(item);

        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        float iconY = min.Y + Math.Max(0f, (rowHeight - iconSize) * 0.5f);
        DrawItemIcon(item, new Vector2(min.X + 3f, iconY), iconSize);

        var drawList = ImGui.GetWindowDrawList();
        float textY = min.Y + Math.Max(0f, (rowHeight - ImGui.GetTextLineHeight()) * 0.5f);
        drawList.AddText(new Vector2(min.X + iconSize + 9f, textY), ImGui.GetColorU32(ImGuiCol.Text), item.Label);

        // Tag color ribbon at bottom of row
        if (hasTags)
        {
            float ribbonY = max.Y - ribbonHeight;
            float totalWidth = max.X - min.X;
            DrawTagRibbon(drawList, new Vector2(min.X, ribbonY), totalWidth, ribbonHeight, tags!);
        }
    }

    private void DrawItemsGrid(IReadOnlyList<BrowserItem> items)
    {
        float baseIconSize = EditorIcons.GetIconSize();
        float iconSize = Math.Max(24f, baseIconSize * 2.2f);
        float tileWidth = Math.Max(110f, iconSize + 32f);
        float ribbonHeight = 2f;
        float tileHeight = iconSize + ImGui.GetTextLineHeightWithSpacing() * 2f + 14f + ribbonHeight + 4f;
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
            DrawGridItem(item, tileWidth, tileHeight, iconSize, ribbonHeight);
            ImGui.PopID();
        }

        ImGui.EndTable();
    }

    private void DrawGridItem(BrowserItem item, float tileWidth, float tileHeight, float iconSize, float ribbonHeight)
    {
        bool isSelected = _selectedAssets.Contains(item.Id);
        bool clicked = ImGui.Selectable("##tile", isSelected, ImGuiSelectableFlags.AllowDoubleClick, new Vector2(tileWidth, tileHeight));

        if (clicked)
        {
            HandleItemClick(item);
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                HandleAssetDoubleClick(item.Asset);
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(item.Tooltip);

        DrawAssetDragDropSource(item);
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

        // Tag ribbon just above label text
        var tags = _app.TagDatabase?.GetTagsForAsset(item.Asset.RelativePath);
        if (tags != null && tags.Count > 0)
        {
            float ribbonWidth = Math.Min(tileWidth - 16f, textSize.X + 12f);
            float ribbonX = min.X + (tileWidth - ribbonWidth) * 0.5f;
            float ribbonY = textY - ribbonHeight - 2f;
            DrawTagRibbon(drawList, new Vector2(ribbonX, ribbonY), ribbonWidth, ribbonHeight, tags);
        }
    }

    private static void DrawTagRibbon(ImDrawListPtr drawList, Vector2 pos, float totalWidth, float height, List<AssetTag> tags)
    {
        if (tags.Count == 0)
            return;

        float segWidth = totalWidth / tags.Count;
        for (int i = 0; i < tags.Count; i++)
        {
            var color = ParseHexColor(tags[i].Color);
            float x = pos.X + segWidth * i;
            drawList.AddRectFilled(
                new Vector2(x, pos.Y),
                new Vector2(x + segWidth, pos.Y + height),
                ImGuiColorToU32(color));
        }
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

    private void HandleItemClick(BrowserItem item)
    {
        var io = ImGui.GetIO();

        if (io.KeyShift && _selectionAnchor != null)
        {
            // Range select
            int anchorIndex = _lastItems.FindIndex(i => string.Equals(i.Id, _selectionAnchor, StringComparison.OrdinalIgnoreCase));
            int currentIndex = _lastItems.FindIndex(i => string.Equals(i.Id, item.Id, StringComparison.OrdinalIgnoreCase));

            if (anchorIndex >= 0 && currentIndex >= 0)
            {
                int start = Math.Min(anchorIndex, currentIndex);
                int end = Math.Max(anchorIndex, currentIndex);

                if (!io.KeyCtrl)
                    _selectedAssets.Clear();

                for (int i = start; i <= end; i++)
                    _selectedAssets.Add(_lastItems[i].Id);
            }
            else
            {
                _selectedAssets.Clear();
                _selectedAssets.Add(item.Id);
                _selectionAnchor = item.Id;
            }
        }
        else if (io.KeyCtrl)
        {
            // Toggle select
            if (!_selectedAssets.Remove(item.Id))
                _selectedAssets.Add(item.Id);
            _selectionAnchor = item.Id;
        }
        else
        {
            // Single select
            _selectedAssets.Clear();
            _selectedAssets.Add(item.Id);
            _selectionAnchor = item.Id;
        }
    }

    private void HandleAssetDoubleClick(AssetEntry asset)
    {
        switch (asset.Type)
        {
            case AssetType.Scene:
                OpenSceneAsset(asset);
                break;
            case AssetType.Prefab:
                InstantiatePrefabAsset(asset);
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

    private void DrawAssetDragDropSource(BrowserItem item)
    {
        if (!ImGui.BeginDragDropSource(ImGuiDragDropFlags.SourceAllowNullID))
            return;

        _app.DraggedAssetPath = item.Asset.RelativePath;
        ImGui.SetDragDropPayload(AssetDragPayload, nint.Zero, 0);
        ImGui.TextUnformatted($"[{item.Asset.Type}] {item.Label}");
        ImGui.EndDragDropSource();
    }

    private void DrawItemContextMenu(BrowserItem item)
    {
        if (!ImGui.BeginPopupContextItem())
            return;

        // Ensure clicked item is in selection
        if (!_selectedAssets.Contains(item.Id))
        {
            _selectedAssets.Clear();
            _selectedAssets.Add(item.Id);
            _selectionAnchor = item.Id;
        }

        DrawAssetContextMenu(item.Asset);

        ImGui.EndPopup();
    }

    private void DrawAssetContextMenu(AssetEntry asset)
    {
        if (asset.Type == AssetType.Scene && ImGui.MenuItem("Open Scene"))
            OpenSceneAsset(asset);

        if (asset.Type == AssetType.Prefab && ImGui.MenuItem("Instantiate Prefab"))
            InstantiatePrefabAsset(asset);

        if (asset.Type == AssetType.Script && ImGui.MenuItem("Open in VS Code"))
            OpenScriptAsset(asset);

        if (asset.Type == AssetType.Model && ImGui.MenuItem("Assign to MeshRenderer"))
            AssignModelToSelected(asset);

        if (ImGui.MenuItem("Open in Default Program"))
            OpenAssetExternally(asset);

        if (ImGui.MenuItem("Copy Path"))
            ImGui.SetClipboardText(asset.RelativePath);

        // Tags submenu
        var tagDb = _app.TagDatabase;
        if (tagDb != null && ImGui.BeginMenu("Tags"))
        {
            var allTags = tagDb.GetAllTags();
            var targetPaths = _selectedAssets.Count > 0 ? _selectedAssets.ToList() : new List<string> { asset.RelativePath };

            foreach (var tag in allTags)
            {
                // Check if all selected assets have this tag
                bool allHave = targetPaths.All(p => tagDb.AssetHasTag(p, tag.Name));

                var color = ParseHexColor(tag.Color);
                var drawList = ImGui.GetWindowDrawList();
                var cursor = ImGui.GetCursorScreenPos();
                drawList.AddRectFilled(cursor, cursor + new Vector2(12, 12), ImGuiColorToU32(color), 2f);
                ImGui.Dummy(new Vector2(12, 12));
                ImGui.SameLine();

                if (ImGui.MenuItem(tag.Name, null, allHave))
                {
                    if (allHave)
                        tagDb.RemoveTagFromAssets(targetPaths, tag.Name);
                    else
                        tagDb.AddTagToAssets(targetPaths, tag.Name);
                    _app.SaveTagDatabase();
                }
            }

            if (allTags.Count > 0)
                ImGui.Separator();

            if (ImGui.MenuItem("Manage Tags..."))
                _openTagManager = true;

            ImGui.EndMenu();
        }
    }

    private void DrawTagManagerModal()
    {
        if (_openTagManager)
        {
            ImGui.OpenPopup("Tag Manager");
            _openTagManager = false;
        }

        var center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(420, 400), ImGuiCond.Appearing);

        if (!ImGui.BeginPopupModal("Tag Manager", ImGuiWindowFlags.None))
            return;

        var tagDb = _app.TagDatabase;
        if (tagDb == null)
        {
            ImGui.TextDisabled("No project open.");
            if (ImGui.Button("Close"))
                ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        var allTags = tagDb.GetAllTags();
        bool changed = false;

        if (ImGui.BeginChild("TagList", new Vector2(0, -ImGui.GetFrameHeightWithSpacing() * 2 - 4), ImGuiChildFlags.Borders))
        {
            if (allTags.Count == 0)
            {
                ImGui.TextDisabled("No tags defined yet.");
            }
            else if (ImGui.BeginTable("##TagTable", 4, ImGuiTableFlags.NoPadOuterX))
            {
                ImGui.TableSetupColumn("Color", ImGuiTableColumnFlags.WidthFixed, 30f);
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Count", ImGuiTableColumnFlags.WidthFixed, 40f);
                ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 60f);

                for (int i = 0; i < allTags.Count; i++)
                {
                    var tag = allTags[i];
                    ImGui.PushID(i);
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    var color = ParseHexColorVec3(tag.Color);
                    if (ImGui.ColorEdit3("##color", ref color, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel))
                    {
                        tag.Color = ToHexColor(color);
                        changed = true;
                    }

                    ImGui.TableSetColumnIndex(1);
                    string name = tag.Name;
                    ImGui.SetNextItemWidth(-1);
                    if (ImGui.InputText("##name", ref name, 128, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        if (!string.IsNullOrWhiteSpace(name) && !string.Equals(name.Trim(), tag.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            tagDb.RenameTag(tag.Name, name.Trim());
                            changed = true;
                        }
                    }

                    ImGui.TableSetColumnIndex(2);
                    var assetsWithTag = tagDb.GetAssetsWithTag(tag.Name);
                    ImGui.TextDisabled($"{assetsWithTag.Count}");

                    ImGui.TableSetColumnIndex(3);
                    if (ImGui.Button("Delete"))
                    {
                        if (assetsWithTag.Count > 0)
                            _confirmDeleteTag = tag.Name;
                        else
                        {
                            tagDb.DeleteTag(tag.Name);
                            changed = true;
                        }
                    }

                    ImGui.PopID();
                }

                ImGui.EndTable();
            }
        }
        ImGui.EndChild();

        // Delete confirmation
        if (_confirmDeleteTag != null)
        {
            ImGui.OpenPopup("ConfirmDeleteTag");
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        }

        if (ImGui.BeginPopupModal("ConfirmDeleteTag", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
        {
            ImGui.Text($"Tag '{_confirmDeleteTag}' is used by assets. Delete anyway?");
            if (ImGui.Button("Delete", new Vector2(120, 0)))
            {
                tagDb.DeleteTag(_confirmDeleteTag!);
                changed = true;
                _confirmDeleteTag = null;
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(120, 0)))
            {
                _confirmDeleteTag = null;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        // New tag row + Close
        ImGui.ColorEdit3("##newcolor", ref _newTagColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 160f);
        bool submitted = ImGui.InputText("##newtag", ref _newTagName, 128, ImGuiInputTextFlags.EnterReturnsTrue);
        ImGui.SameLine();
        if ((ImGui.Button("New Tag") || submitted) && !string.IsNullOrWhiteSpace(_newTagName))
        {
            tagDb.CreateTag(_newTagName.Trim(), ToHexColor(_newTagColor));
            _newTagName = string.Empty;
            changed = true;
        }
        float closeWidth = ImGui.CalcTextSize("Close").X + ImGui.GetStyle().FramePadding.X * 2f;
        ImGui.SameLine(ImGui.GetWindowWidth() - closeWidth - ImGui.GetStyle().WindowPadding.X);
        if (ImGui.Button("Close"))
            ImGui.CloseCurrentPopup();

        if (changed)
            _app.SaveTagDatabase();

        ImGui.EndPopup();
    }

    private void InstantiatePrefabAsset(AssetEntry asset)
    {
        if (asset.Type != AssetType.Prefab)
            return;

        _app.InstantiatePrefabAsset(asset.RelativePath);
    }

    private void OpenSceneAsset(AssetEntry asset)
    {
        var fullPath = AssetManager.Instance.ResolvePath(asset.RelativePath);
        SceneManager.Instance.LoadScene(fullPath);
        _app.CurrentScene = SceneManager.Instance.ActiveScene;
        _app.Prefabs.RecalculateOverridesForScene();
        _app.ClearSelection();
        _app.RestoreEditorCameraFromScene();
        _app.UpdateWindowTitle();
        _app.UndoRedo.Clear();
        _app.UndoRedo.SetBaseline(_app.CurrentScene, _app.GetSelectedEntityIds(), _app.SerializeCurrentHierarchyState());
        FrinkyLog.Info($"Opened scene: {asset.RelativePath}");
    }

    private static void DrawItemIcon(BrowserItem item, Vector2 min, float size)
    {
        Texture2D? icon = EditorIcons.GetIcon(item.Asset.Type);

        if (icon is Texture2D tex)
        {
            uint tint = EditorIcons.GetIconTint(item.Asset.Type);
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddImage((nint)tex.Id, min, new Vector2(min.X + size, min.Y + size),
                Vector2.Zero, Vector2.One, tint);
        }
    }

    private static Vector4 ParseHexColor(string hex)
    {
        var v = ParseHexColorVec3(hex);
        return new Vector4(v.X, v.Y, v.Z, 1.0f);
    }

    private static Vector3 ParseHexColorVec3(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return new Vector3(1, 1, 1);

        hex = hex.TrimStart('#');
        if (hex.Length < 6)
            return new Vector3(1, 1, 1);

        if (int.TryParse(hex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int r) &&
            int.TryParse(hex[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int g) &&
            int.TryParse(hex[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int b))
        {
            return new Vector3(r / 255f, g / 255f, b / 255f);
        }

        return new Vector3(1, 1, 1);
    }

    private static string ToHexColor(Vector3 color)
    {
        int r = Math.Clamp((int)(color.X * 255f), 0, 255);
        int g = Math.Clamp((int)(color.Y * 255f), 0, 255);
        int b = Math.Clamp((int)(color.Z * 255f), 0, 255);
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private static bool AssetMatchesTagSearch(AssetTagDatabase? tagDb, string relativePath, string query)
    {
        if (tagDb == null)
            return false;

        var tags = tagDb.GetTagsForAsset(relativePath);
        foreach (var tag in tags)
        {
            if (tag.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static uint ImGuiColorToU32(Vector4 color)
    {
        return ImGui.ColorConvertFloat4ToU32(color);
    }
}
