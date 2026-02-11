using System.Diagnostics;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Serialization;
using Hexa.NET.ImGui;

namespace FrinkyEngine.Editor.Panels;

public class HierarchyPanel
{
    private const string RootFolderKey = "__root__";
    private const string EntityDragPayload = "FRINKY_HIERARCHY_ENTITY";
    private const string FolderDragPayload = "FRINKY_HIERARCHY_FOLDER";

    private readonly EditorApplication _app;

    private Guid? _rangeAnchorId;
    private bool _isWindowFocused;
    private bool _focusSearchRequested;

    private Guid? _focusedEntityId;
    private string? _focusedFolderId;

    private Guid? _renamingEntityId;
    private string? _renamingFolderId;
    private string _renameBuffer = string.Empty;
    private bool _focusRenameInput;

    private Guid? _draggedEntityId;
    private string? _draggedFolderId;
    private bool _openCreateFolderPopup;
    private string? _createFolderParentId;
    private string _createFolderName = "New Folder";
    private bool _focusCreateFolderInput;

    private List<Entity> _lastVisibleEntities = new();

    public HierarchyPanel(EditorApplication app)
    {
        _app = app;
    }

    public bool IsWindowFocused => _isWindowFocused;

    public void FocusSearch()
    {
        _focusSearchRequested = true;
    }

    public void BeginRenameSelected()
    {
        if (!string.IsNullOrWhiteSpace(_focusedFolderId))
        {
            BeginRenameFolder(_focusedFolderId);
            return;
        }

        if (_app.SelectedEntity != null)
        {
            BeginRenameEntity(_app.SelectedEntity);
            return;
        }
    }

    public void SelectAllVisibleEntities()
    {
        if (ShouldSuppressHierarchyHotkeys())
            return;

        if (_lastVisibleEntities.Count == 0)
            return;

        _app.SetSelection(_lastVisibleEntities);
        _rangeAnchorId = _lastVisibleEntities[^1].Id;
    }

    public void ExpandSelection()
    {
        if (ShouldSuppressHierarchyHotkeys() || _app.CurrentScene == null)
            return;

        var state = _app.GetOrCreateHierarchySceneState();
        var expandedEntities = new HashSet<string>(state.ExpandedEntityIds, StringComparer.OrdinalIgnoreCase);
        var expandedFolders = new HashSet<string>(state.ExpandedFolderIds, StringComparer.OrdinalIgnoreCase);

        bool changed = false;
        foreach (var entity in _app.SelectedEntities)
        {
            if (entity.Scene != _app.CurrentScene || entity.Transform.Children.Count == 0)
                continue;

            changed |= expandedEntities.Add(entity.Id.ToString("N"));
        }

        if (!string.IsNullOrWhiteSpace(_focusedFolderId))
            changed |= expandedFolders.Add(_focusedFolderId);

        if (!changed)
            return;

        state.ExpandedEntityIds = expandedEntities.ToList();
        state.ExpandedFolderIds = expandedFolders.ToList();
        _app.MarkHierarchyStateDirty();
    }

    public void CollapseSelection()
    {
        if (ShouldSuppressHierarchyHotkeys() || _app.CurrentScene == null)
            return;

        var state = _app.GetOrCreateHierarchySceneState();
        var expandedEntities = new HashSet<string>(state.ExpandedEntityIds, StringComparer.OrdinalIgnoreCase);
        var expandedFolders = new HashSet<string>(state.ExpandedFolderIds, StringComparer.OrdinalIgnoreCase);

        bool changed = false;
        foreach (var entity in _app.SelectedEntities)
            changed |= expandedEntities.Remove(entity.Id.ToString("N"));

        if (!string.IsNullOrWhiteSpace(_focusedFolderId))
            changed |= expandedFolders.Remove(_focusedFolderId);

        if (!changed)
            return;

        state.ExpandedEntityIds = expandedEntities.ToList();
        state.ExpandedFolderIds = expandedFolders.ToList();
        _app.MarkHierarchyStateDirty();
    }

    public unsafe void Draw()
    {
        if (!ImGui.Begin("Hierarchy"))
        {
            _isWindowFocused = false;
            ImGui.End();
            return;
        }

        _isWindowFocused = ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);

        if (_app.CurrentScene == null)
        {
            ImGui.TextDisabled("No scene loaded.");
            ImGui.End();
            return;
        }

        _app.CleanupHierarchyStateForCurrentScene();
        var state = _app.GetOrCreateHierarchySceneState();
        bool canEditScene = _app.CanEditScene;

        if (!canEditScene)
            ImGui.TextDisabled("Editing is disabled in Play mode.");

        ImGui.BeginDisabled(!canEditScene);
        DrawToolbar(state);
        DrawCreateFolderPopup(state);
        ImGui.Separator();

        var context = BuildRenderContext(_app.CurrentScene, state);
        var expandedFolders = new HashSet<string>(state.ExpandedFolderIds, StringComparer.OrdinalIgnoreCase);
        var expandedEntities = new HashSet<string>(state.ExpandedEntityIds, StringComparer.OrdinalIgnoreCase);
        var visibleEntities = new List<Entity>();
        bool expansionChanged = false;

        DrawRootContent(_app.CurrentScene, state, context, expandedFolders, expandedEntities, visibleEntities, ref expansionChanged);
        DrawBackgroundContextMenu(state);
        DrawRootDropZone(state);

        if (expansionChanged)
        {
            state.ExpandedFolderIds = expandedFolders.ToList();
            state.ExpandedEntityIds = expandedEntities.ToList();
            _app.MarkHierarchyStateDirty();
        }

        if ((ImGuiPayload*)ImGui.GetDragDropPayload().Handle == null)
        {
            _draggedEntityId = null;
            _draggedFolderId = null;
            _app.DraggedEntityId = null;
        }

        HandleKeyboardNavigation(visibleEntities);
        _lastVisibleEntities = visibleEntities;
        ImGui.EndDisabled();
        ImGui.End();
    }

    private void DrawToolbar(HierarchySceneState state)
    {
        bool stateChanged = false;
        bool openFiltersPopup = false;
        bool openCreatePopup = false;

        if (ImGui.BeginTable(
                "##HierarchyToolbar",
                3,
                ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.NoPadOuterX | ImGuiTableFlags.NoPadInnerX))
        {
            ImGui.TableSetupColumn("Search", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Filters", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Create", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            if (_focusSearchRequested)
            {
                ImGui.SetKeyboardFocusHere();
                _focusSearchRequested = false;
            }

            string search = state.SearchQuery;
            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputTextWithHint("##HierarchySearch", "Search", ref search, 256))
            {
                state.SearchQuery = search;
                stateChanged = true;
            }

            ImGui.TableSetColumnIndex(1);
            if (ImGui.Button("Filters"))
                openFiltersPopup = true;

            ImGui.TableSetColumnIndex(2);
            if (ImGui.Button("Create"))
                openCreatePopup = true;

            ImGui.EndTable();
        }

        if (openFiltersPopup)
            ImGui.OpenPopup("HierarchyFilters");
        if (openCreatePopup)
            ImGui.OpenPopup("HierarchyCreate");

        if (ImGui.BeginPopup("HierarchyFilters"))
        {
            bool activeOnly = state.FilterActiveOnly;
            if (ImGui.Checkbox("Active Only", ref activeOnly))
            {
                state.FilterActiveOnly = activeOnly;
                if (activeOnly)
                    state.FilterInactiveOnly = false;
                stateChanged = true;
            }

            bool inactiveOnly = state.FilterInactiveOnly;
            if (ImGui.Checkbox("Inactive Only", ref inactiveOnly))
            {
                state.FilterInactiveOnly = inactiveOnly;
                if (inactiveOnly)
                    state.FilterActiveOnly = false;
                stateChanged = true;
            }

            var prefabFilter = state.PrefabFilter;
            var prefabPreview = GetPrefabFilterLabel(prefabFilter);
            if (ImGui.BeginCombo("Prefab", prefabPreview))
            {
                stateChanged |= DrawPrefabFilterOption(state, HierarchyPrefabFilter.Any);
                stateChanged |= DrawPrefabFilterOption(state, HierarchyPrefabFilter.PrefabInstances);
                stateChanged |= DrawPrefabFilterOption(state, HierarchyPrefabFilter.PrefabRoots);
                stateChanged |= DrawPrefabFilterOption(state, HierarchyPrefabFilter.NonPrefabs);
                ImGui.EndCombo();
            }

            ImGui.Separator();

            var componentTypes = ComponentTypeResolver.GetAllComponentTypes()
                .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var currentType = string.IsNullOrWhiteSpace(state.RequiredComponentType)
                ? null
                : ComponentTypeResolver.Resolve(state.RequiredComponentType);

            var preview = currentType?.Name ?? "Any Component";
            if (ImGui.BeginCombo("Has Component", preview))
            {
                bool anySelected = string.IsNullOrWhiteSpace(state.RequiredComponentType);
                if (ImGui.Selectable("Any Component", anySelected))
                {
                    state.RequiredComponentType = string.Empty;
                    stateChanged = true;
                }

                foreach (var type in componentTypes)
                {
                    var typeName = type.FullName ?? type.Name;
                    bool selected = string.Equals(typeName, state.RequiredComponentType, StringComparison.Ordinal);
                    var source = ComponentTypeResolver.GetAssemblySource(type);
                    if (ImGui.Selectable($"{type.Name}  [{source}]", selected))
                    {
                        state.RequiredComponentType = typeName;
                        stateChanged = true;
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.Separator();

            bool showOnlyMatches = state.ShowOnlyMatches;
            if (ImGui.Checkbox("Show Only Matches", ref showOnlyMatches))
            {
                state.ShowOnlyMatches = showOnlyMatches;
                stateChanged = true;
            }

            bool autoExpand = state.AutoExpandMatches;
            if (ImGui.Checkbox("Auto Expand Matches", ref autoExpand))
            {
                state.AutoExpandMatches = autoExpand;
                stateChanged = true;
            }

            ImGui.EndPopup();
        }

        if (ImGui.BeginPopup("HierarchyCreate"))
        {
            DrawCreateEntityMenuItems(parent: null);
            ImGui.Separator();
            if (ImGui.MenuItem("Folder"))
                RequestCreateFolder(parentFolderId: null);
            ImGui.EndPopup();
        }

        if (stateChanged)
            _app.MarkHierarchyStateDirty();
    }

    private void DrawRootContent(
        Core.Scene.Scene scene,
        HierarchySceneState state,
        HierarchyRenderContext context,
        HashSet<string> expandedFolders,
        HashSet<string> expandedEntities,
        List<Entity> visibleEntities,
        ref bool expansionChanged)
    {
        foreach (var folder in context.RootFolders)
            DrawFolderNode(folder, scene, state, context, expandedFolders, expandedEntities, visibleEntities, ref expansionChanged);

        foreach (var entity in context.UnassignedRootEntities)
            DrawEntityNode(entity, state, context, expandedEntities, visibleEntities, ref expansionChanged);
    }

    private void DrawFolderNode(
        HierarchyFolderState folder,
        Core.Scene.Scene scene,
        HierarchySceneState state,
        HierarchyRenderContext context,
        HashSet<string> expandedFolders,
        HashSet<string> expandedEntities,
        List<Entity> visibleEntities,
        ref bool expansionChanged)
    {
        if (!context.FolderVisible.GetValueOrDefault(folder.Id, true))
            return;

        var childFolders = context.GetOrderedChildFolders(folder.Id)
            .Where(child => context.FolderVisible.GetValueOrDefault(child.Id, true))
            .ToList();
        var folderEntities = context.GetFolderEntities(folder.Id)
            .Where(entity => context.EntityVisible.GetValueOrDefault(entity.Id, true))
            .ToList();

        bool hasChildren = childFolders.Count > 0 || folderEntities.Count > 0;
        bool isFocused = string.Equals(_focusedFolderId, folder.Id, StringComparison.OrdinalIgnoreCase);

        var flags = ImGuiTreeNodeFlags.OpenOnArrow
                    | ImGuiTreeNodeFlags.SpanAvailWidth
                    | ImGuiTreeNodeFlags.FramePadding;
        if (!hasChildren)
            flags |= ImGuiTreeNodeFlags.Leaf;
        if (isFocused)
            flags |= ImGuiTreeNodeFlags.Selected;

        bool forceOpen = context.HasSearch
                         && state.AutoExpandMatches
                         && context.FolderSubtreeMatch.GetValueOrDefault(folder.Id);

        bool isOpen = expandedFolders.Contains(folder.Id) || forceOpen;
        ImGui.SetNextItemOpen(isOpen, ImGuiCond.Always);

        bool isRenamingFolder = string.Equals(_renamingFolderId, folder.Id, StringComparison.OrdinalIgnoreCase);
        bool opened = ImGui.TreeNodeEx($"##folder_{folder.Id}", flags);

        if (isRenamingFolder)
            DrawFolderRenameInput(folder);
        else
            DrawFolderRowLabel(folder.Name);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            _focusedFolderId = folder.Id;
            _focusedEntityId = null;
            _renamingEntityId = null;
        }

        if (ImGui.IsItemToggledOpen() && !forceOpen)
        {
            if (opened)
                expandedFolders.Add(folder.Id);
            else
                expandedFolders.Remove(folder.Id);
            expansionChanged = true;
        }

        DrawFolderDragDropSource(folder);
        DrawFolderDragDropTarget(folder, scene, state);
        DrawFolderContextMenu(folder, state);

        if (opened)
        {
            foreach (var childFolder in childFolders)
                DrawFolderNode(childFolder, scene, state, context, expandedFolders, expandedEntities, visibleEntities, ref expansionChanged);

            foreach (var entity in folderEntities)
                DrawEntityNode(entity, state, context, expandedEntities, visibleEntities, ref expansionChanged);

            ImGui.TreePop();
        }
    }

    private void DrawEntityNode(
        Entity entity,
        HierarchySceneState state,
        HierarchyRenderContext context,
        HashSet<string> expandedEntities,
        List<Entity> visibleEntities,
        ref bool expansionChanged)
    {
        if (!context.EntityVisible.GetValueOrDefault(entity.Id, true))
            return;

        var visibleChildren = entity.Transform.Children
            .Select(c => c.Entity)
            .Where(child => context.EntityVisible.GetValueOrDefault(child.Id, true))
            .ToList();

        bool hasChildren = visibleChildren.Count > 0;

        var flags = ImGuiTreeNodeFlags.OpenOnArrow
                    | ImGuiTreeNodeFlags.SpanAvailWidth
                    | ImGuiTreeNodeFlags.FramePadding;
        if (!hasChildren)
            flags |= ImGuiTreeNodeFlags.Leaf;
        if (_app.IsSelected(entity))
            flags |= ImGuiTreeNodeFlags.Selected;

        bool forceOpen = context.HasSearch
                         && state.AutoExpandMatches
                         && context.EntitySubtreeMatch.GetValueOrDefault(entity.Id)
                         && hasChildren;

        string entityKey = entity.Id.ToString("N");
        bool isOpen = expandedEntities.Contains(entityKey) || forceOpen;
        ImGui.SetNextItemOpen(isOpen, ImGuiCond.Always);

        bool isRenaming = _renamingEntityId.HasValue && _renamingEntityId.Value == entity.Id;
        string entityLabel = isRenaming ? string.Empty : entity.Name;
        bool opened = ImGui.TreeNodeEx($"entity_{entityKey}", flags, entityLabel);

        visibleEntities.Add(entity);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            _focusedEntityId = entity.Id;
            _focusedFolderId = null;
            HandleEntitySelection(entity, _lastVisibleEntities.Count > 0 ? _lastVisibleEntities : visibleEntities);
        }

        if (ImGui.IsItemToggledOpen() && !forceOpen)
        {
            if (opened)
                expandedEntities.Add(entityKey);
            else
                expandedEntities.Remove(entityKey);
            expansionChanged = true;
        }

        DrawEntityDragDropSource(entity);
        DrawEntityDragDropTarget(entity);
        DrawAssetBrowserDropTarget(entity);
        DrawEntityContextMenu(entity, state);
        DrawEntityRenameInput(entity, isRenaming);
        DrawEntityStatusInline(entity);

        if (opened)
        {
            foreach (var child in visibleChildren)
                DrawEntityNode(child, state, context, expandedEntities, visibleEntities, ref expansionChanged);

            ImGui.TreePop();
        }
    }

    private void DrawEntityStatusInline(Entity entity)
    {
        int componentCount = Math.Max(0, entity.Components.Count - 1);
        var prefabRoot = _app.Prefabs.GetPrefabRoot(entity);
        bool isPrefab = prefabRoot?.Prefab != null;
        bool isPrefabRoot = isPrefab && prefabRoot!.Id == entity.Id;
        int overrideCount = isPrefabRoot
            ? (prefabRoot!.Prefab!.Overrides?.PropertyOverrides.Count ?? 0)
              + (prefabRoot.Prefab.Overrides?.AddedComponents.Count ?? 0)
              + (prefabRoot.Prefab.Overrides?.RemovedComponents.Count ?? 0)
              + (prefabRoot.Prefab.Overrides?.AddedChildren.Count ?? 0)
              + (prefabRoot.Prefab.Overrides?.RemovedChildren.Count ?? 0)
            : 0;

        string prefabLabel = isPrefab ? (isPrefabRoot && overrideCount > 0 ? "[P*]" : "[P]") : string.Empty;
        string componentLabel = string.IsNullOrEmpty(prefabLabel)
            ? $"[{componentCount}]"
            : $"{prefabLabel} [{componentCount}]";

        float spacing = ImGui.GetStyle().ItemSpacing.X;
        float componentWidth = ImGui.CalcTextSize(componentLabel).X;

        ImGui.SameLine(0f, spacing);
        float targetX = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - componentWidth;
        if (targetX > ImGui.GetCursorPosX())
            ImGui.SetCursorPosX(targetX);

        ImGui.TextDisabled(componentLabel);
    }

    private void DrawEntityRenameInput(Entity entity, bool isRenaming)
    {
        if (!isRenaming)
            return;

        ImGui.SameLine();
        float labelX = ImGui.GetItemRectMin().X + ImGui.GetTreeNodeToLabelSpacing();
        ImGui.SetCursorScreenPos(new System.Numerics.Vector2(labelX, ImGui.GetItemRectMin().Y));
        ImGui.SetNextItemWidth(MathF.Max(140f, ImGui.GetItemRectMax().X - labelX));
        if (_focusRenameInput)
        {
            ImGui.SetKeyboardFocusHere();
            _focusRenameInput = false;
        }

        bool submitted = ImGui.InputText($"##rename_entity_{entity.Id:N}", ref _renameBuffer, 128, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll);
        bool cancel = ImGui.IsItemActive() && ImGui.IsKeyPressed(ImGuiKey.Escape);

        if (cancel)
        {
            CancelRename();
            return;
        }

        if (submitted || ImGui.IsItemDeactivated())
            CommitEntityRename(entity);
    }

    private void DrawFolderRenameInput(HierarchyFolderState folder)
    {
        ImGui.SameLine();
        float labelX = ImGui.GetItemRectMin().X + ImGui.GetTreeNodeToLabelSpacing();
        ImGui.SetCursorScreenPos(new System.Numerics.Vector2(labelX, ImGui.GetItemRectMin().Y));
        ImGui.SetNextItemWidth(MathF.Max(140f, ImGui.GetItemRectMax().X - labelX));
        if (_focusRenameInput)
        {
            ImGui.SetKeyboardFocusHere();
            _focusRenameInput = false;
        }

        bool submitted = ImGui.InputText($"##rename_folder_{folder.Id}", ref _renameBuffer, 128, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll);
        bool cancel = ImGui.IsItemActive() && ImGui.IsKeyPressed(ImGuiKey.Escape);

        if (cancel)
        {
            CancelRename();
            return;
        }

        if (submitted || ImGui.IsItemDeactivated())
            CommitFolderRename(folder);
    }

    private static unsafe void DrawFolderRowLabel(string folderName)
    {
        var min = ImGui.GetItemRectMin();
        float labelOffsetX = ImGui.GetTreeNodeToLabelSpacing();
        float textLineHeight = ImGui.GetTextLineHeight();
        float frameHeight = ImGui.GetFrameHeight();
        float iconSize = MathF.Max(12f, textLineHeight);

        float iconX = min.X + labelOffsetX;
        float iconY = min.Y + MathF.Max(0f, (frameHeight - iconSize) * 0.5f);

        var drawList = ImGui.GetWindowDrawList();
        var textColor = ImGui.GetColorU32(ImGuiCol.Text);

        var folderIcon = EditorIcons.GetFolderIcon();
        if (folderIcon.HasValue)
        {
            var texture = folderIcon.Value;
            drawList.AddImage(
                new ImTextureRef(null, new ImTextureID((ulong)texture.Id)),
                new System.Numerics.Vector2(iconX, iconY),
                new System.Numerics.Vector2(iconX + iconSize, iconY + iconSize));
        }
        else
        {
            drawList.AddText(new System.Numerics.Vector2(iconX, min.Y), textColor, "[ ]");
        }

        float textX = iconX + iconSize + 6f;
        float textY = min.Y + MathF.Max(0f, (frameHeight - textLineHeight) * 0.5f);
        drawList.AddText(new System.Numerics.Vector2(textX, textY), textColor, folderName);
    }

    private void DrawEntityContextMenu(Entity entity, HierarchySceneState state)
    {
        if (!ImGui.BeginPopupContextItem($"EntityContext_{entity.Id:N}"))
            return;

        if (!_app.IsSelected(entity))
            _app.SetSingleSelection(entity);

        if (ImGui.MenuItem("Rename", KeybindManager.Instance.GetShortcutText(EditorAction.RenameEntity)))
            BeginRenameEntity(entity);

        if (ImGui.MenuItem("Duplicate", KeybindManager.Instance.GetShortcutText(EditorAction.DuplicateEntity)))
            _app.DuplicateSelectedEntities();

        if (ImGui.MenuItem("Delete", KeybindManager.Instance.GetShortcutText(EditorAction.DeleteEntity)))
            _app.DeleteSelectedEntities();

        var prefabRoot = _app.Prefabs.GetPrefabRoot(entity);
        if (prefabRoot?.Prefab != null)
        {
            ImGui.Separator();
            DrawPrefabContextMenu(entity, prefabRoot);
        }

        ImGui.Separator();

        if (ImGui.MenuItem("Unparent", (string?)null, false, entity.Transform.Parent != null))
        {
            _app.RecordUndo();
            if (_app.ReparentEntity(entity, null))
                _app.RefreshUndoBaseline();
        }

        if (ImGui.BeginMenu("Move To Folder"))
        {
            var rootEntities = _app.SelectedEntities
                .Where(e => e.Transform.Parent == null).ToList();
            bool canAssign = rootEntities.Count > 0;
            ImGui.BeginDisabled(!canAssign);

            if (ImGui.MenuItem("None", (string?)null, canAssign && rootEntities.All(e => string.IsNullOrWhiteSpace(_app.GetRootEntityFolder(e)))))
                MoveEntitiesToFolder(rootEntities, null);

            foreach (var folder in state.Folders.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase))
            {
                bool selected = canAssign && rootEntities.All(e => string.Equals(_app.GetRootEntityFolder(e), folder.Id, StringComparison.OrdinalIgnoreCase));
                if (ImGui.MenuItem(folder.Name, (string?)null, selected))
                    MoveEntitiesToFolder(rootEntities, folder.Id);
            }

            ImGui.EndDisabled();
            ImGui.EndMenu();
        }

        bool active = entity.Active;
        if (ImGui.MenuItem("Active", (string?)null, active))
        {
            _app.RecordUndo();
            entity.Active = !entity.Active;
            _app.RefreshUndoBaseline();
        }

        if (ImGui.BeginMenu("Create Child"))
        {
            DrawCreateEntityMenuItems(entity);
            ImGui.EndMenu();
        }

        ImGui.EndPopup();
    }

    private void DrawPrefabContextMenu(Entity clickedEntity, Entity prefabRoot)
    {
        if (!ImGui.BeginMenu("Prefab"))
            return;

        if (clickedEntity.Id != prefabRoot.Id && ImGui.MenuItem("Select Prefab Root"))
            _app.SetSingleSelection(prefabRoot);

        if (ImGui.MenuItem("Apply"))
            _app.ApplySelectedPrefab();

        if (ImGui.MenuItem("Revert"))
            _app.RevertSelectedPrefab();

        if (ImGui.MenuItem("Make Unique"))
            _app.MakeUniqueSelectedPrefab();

        if (ImGui.MenuItem("Unpack"))
            _app.UnpackSelectedPrefab();

        ImGui.Separator();

        if (ImGui.MenuItem("Open Prefab in VS Code"))
            OpenPrefabInVSCode(prefabRoot);

        if (ImGui.MenuItem("Show Prefab in Explorer"))
            RevealPrefabInExplorer(prefabRoot);

        ImGui.EndMenu();
    }

    private static string? GetPrefabAbsolutePath(Entity prefabRoot)
    {
        var assetPath = prefabRoot.Prefab?.AssetPath.Path;
        if (string.IsNullOrWhiteSpace(assetPath))
            return null;

        var absolutePath = Path.Combine(AssetManager.Instance.AssetsPath, assetPath.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(absolutePath) ? absolutePath : null;
    }

    private void OpenPrefabInVSCode(Entity prefabRoot)
    {
        var absolutePath = GetPrefabAbsolutePath(prefabRoot);
        if (absolutePath == null)
        {
            NotificationManager.Instance.Post("Prefab asset not found.", NotificationType.Warning);
            return;
        }

        _app.OpenFileInVSCode(absolutePath);
    }

    private static void RevealPrefabInExplorer(Entity prefabRoot)
    {
        var absolutePath = GetPrefabAbsolutePath(prefabRoot);
        if (absolutePath == null)
        {
            NotificationManager.Instance.Post("Prefab asset not found.", NotificationType.Warning);
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{absolutePath}\"",
            UseShellExecute = true
        });
    }

    private void DrawFolderContextMenu(HierarchyFolderState folder, HierarchySceneState state)
    {
        if (!ImGui.BeginPopupContextItem($"FolderContext_{folder.Id}"))
            return;

        if (ImGui.MenuItem("Rename", KeybindManager.Instance.GetShortcutText(EditorAction.RenameEntity)))
            BeginRenameFolder(folder.Id);

        if (ImGui.MenuItem("New Subfolder"))
            RequestCreateFolder(folder.Id);

        ImGui.Separator();

        if (ImGui.MenuItem("Delete Folder"))
            DeleteFolder(folder, state);

        if (ImGui.BeginMenu("Create Entity"))
        {
            DrawCreateEntityMenuItems(parent: null);
            ImGui.EndMenu();
        }

        ImGui.EndPopup();
    }

    private void DrawBackgroundContextMenu(HierarchySceneState state)
    {
        if (!ImGui.BeginPopupContextWindow("HierarchyBackgroundContext", ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems))
            return;

        if (ImGui.BeginMenu("Create"))
        {
            DrawCreateEntityMenuItems(parent: null);
            ImGui.Separator();
            if (ImGui.MenuItem("Folder"))
                RequestCreateFolder(parentFolderId: null);
            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.MenuItem("Expand All"))
        {
            state.ExpandedFolderIds = state.Folders.Select(f => f.Id).ToList();
            state.ExpandedEntityIds = _app.CurrentScene?.Entities.Select(e => e.Id.ToString("N")).ToList() ?? new List<string>();
            _app.MarkHierarchyStateDirty();
        }

        if (ImGui.MenuItem("Collapse All"))
        {
            state.ExpandedFolderIds.Clear();
            state.ExpandedEntityIds.Clear();
            _app.MarkHierarchyStateDirty();
        }

        ImGui.EndPopup();
    }

    private unsafe void DrawRootDropZone(HierarchySceneState state)
    {
        ImGui.Separator();
        const string dropLabel = "Drop Here to Move to Root";
        float width = MathF.Max(1f, ImGui.GetContentRegionAvail().X);
        float height = ImGui.GetFrameHeight();
        var min = ImGui.GetCursorScreenPos();

        ImGui.InvisibleButton("##RootDropZone", new System.Numerics.Vector2(width, height));

        var max = new System.Numerics.Vector2(min.X + width, min.Y + height);
        var drawList = ImGui.GetWindowDrawList();
        uint bg = ImGui.GetColorU32(ImGuiCol.FrameBg);
        uint border = ImGui.GetColorU32(ImGuiCol.Border);
        uint textColor = ImGui.GetColorU32(ImGuiCol.TextDisabled);

        drawList.AddRectFilled(min, max, bg, 2f);
        drawList.AddRect(min, max, border, 2f);

        var textSize = ImGui.CalcTextSize(dropLabel);
        var textPos = new System.Numerics.Vector2(
            min.X + MathF.Max(0f, (width - textSize.X) * 0.5f),
            min.Y + MathF.Max(0f, (height - textSize.Y) * 0.5f));
        drawList.AddText(textPos, textColor, dropLabel);

        if (!ImGui.BeginDragDropTarget())
            return;

        ImGuiPayload* entityPayload = ImGui.AcceptDragDropPayload(EntityDragPayload);
        if (entityPayload != null && entityPayload->Delivery != 0 && _draggedEntityId.HasValue)
        {
            var entity = _app.FindEntityById(_draggedEntityId.Value);
            if (entity != null)
            {
                bool changed = false;
                _app.RecordUndo();

                if (entity.Transform.Parent != null)
                    changed |= _app.ReparentEntity(entity, null);

                changed |= _app.SetRootEntityFolder(entity, null);

                if (changed)
                    _app.RefreshUndoBaseline();
            }
        }

        ImGuiPayload* folderPayload = ImGui.AcceptDragDropPayload(FolderDragPayload);
        if (folderPayload != null && folderPayload->Delivery != 0 && !string.IsNullOrWhiteSpace(_draggedFolderId))
        {
            var folder = state.Folders.FirstOrDefault(f => string.Equals(f.Id, _draggedFolderId, StringComparison.OrdinalIgnoreCase));
            if (folder != null && !string.IsNullOrWhiteSpace(folder.ParentFolderId))
            {
                _app.RecordUndo();
                folder.ParentFolderId = null;
                folder.Order = GetNextFolderOrder(state, null);
                ReindexSiblingFolderOrder(state, null);
                _app.MarkHierarchyStateDirty();
                _app.RefreshUndoBaseline();
            }
        }

        ImGui.EndDragDropTarget();
    }

    private void DrawEntityDragDropSource(Entity entity)
    {
        if (!ImGui.BeginDragDropSource(ImGuiDragDropFlags.SourceAllowNullId))
            return;

        _draggedEntityId = entity.Id;
        _app.DraggedEntityId = entity.Id;
        SetDummyPayload(EntityDragPayload);
        ImGui.TextUnformatted(entity.Name);
        ImGui.EndDragDropSource();
    }

    private void DrawFolderDragDropSource(HierarchyFolderState folder)
    {
        if (!ImGui.BeginDragDropSource(ImGuiDragDropFlags.SourceAllowNullId))
            return;

        _draggedFolderId = folder.Id;
        SetDummyPayload(FolderDragPayload);
        ImGui.TextUnformatted(folder.Name);
        ImGui.EndDragDropSource();
    }

    private unsafe void DrawEntityDragDropTarget(Entity target)
    {
        if (!ImGui.BeginDragDropTarget())
            return;

        ImGuiPayload* payload = ImGui.AcceptDragDropPayload(EntityDragPayload);
        if (payload != null && payload->Delivery != 0 && _draggedEntityId.HasValue)
        {
            var dragged = _app.FindEntityById(_draggedEntityId.Value);
            if (dragged != null && dragged.Id != target.Id)
            {
                _app.RecordUndo();
                if (_app.ReparentEntity(dragged, target))
                    _app.RefreshUndoBaseline();
            }
        }

        ImGui.EndDragDropTarget();
    }

    private unsafe void DrawAssetBrowserDropTarget(Entity entity)
    {
        if (!ImGui.BeginDragDropTarget())
            return;

        ImGuiPayload* payload = ImGui.AcceptDragDropPayload(AssetBrowserPanel.AssetDragPayload);
        if (payload != null && payload->Delivery != 0)
        {
            var assetPath = _app.DraggedAssetPath;
            if (!string.IsNullOrEmpty(assetPath))
            {
                var asset = AssetDatabase.Instance.GetAssets()
                    .FirstOrDefault(a => string.Equals(a.RelativePath, assetPath, StringComparison.OrdinalIgnoreCase));

                if (asset is { Type: AssetType.Script })
                {
                    var typeName = Path.GetFileNameWithoutExtension(asset.FileName);
                    var componentType = ComponentTypeResolver.Resolve(typeName);

                    if (componentType == null)
                    {
                        NotificationManager.Instance.Post("Build scripts first.", NotificationType.Warning);
                    }
                    else if (entity.GetComponent(componentType) != null)
                    {
                        NotificationManager.Instance.Post($"{typeName} already exists on {entity.Name}.", NotificationType.Warning);
                    }
                    else
                    {
                        _app.RecordUndo();
                        entity.AddComponent(componentType);
                        _app.SetSingleSelection(entity);
                        _app.RefreshUndoBaseline();
                        NotificationManager.Instance.Post($"Added {typeName} to {entity.Name}", NotificationType.Info, 1.5f);
                    }
                }
            }
        }

        ImGui.EndDragDropTarget();
    }

    private unsafe void DrawFolderDragDropTarget(HierarchyFolderState targetFolder, Core.Scene.Scene scene, HierarchySceneState state)
    {
        if (!ImGui.BeginDragDropTarget())
            return;

        ImGuiPayload* folderPayload = ImGui.AcceptDragDropPayload(FolderDragPayload);
        if (folderPayload != null && folderPayload->Delivery != 0 && !string.IsNullOrWhiteSpace(_draggedFolderId))
        {
            var draggedFolder = state.Folders.FirstOrDefault(f => string.Equals(f.Id, _draggedFolderId, StringComparison.OrdinalIgnoreCase));
            if (draggedFolder != null && !string.Equals(draggedFolder.Id, targetFolder.Id, StringComparison.OrdinalIgnoreCase))
            {
                bool createsCycle = IsFolderDescendant(state, draggedFolder.Id, targetFolder.Id);
                if (!createsCycle)
                {
                    _app.RecordUndo();
                    draggedFolder.ParentFolderId = targetFolder.Id;
                    draggedFolder.Order = GetNextFolderOrder(state, targetFolder.Id);
                    ReindexSiblingFolderOrder(state, targetFolder.Id);
                    _app.MarkHierarchyStateDirty();
                    _app.RefreshUndoBaseline();
                }
            }
        }

        ImGuiPayload* entityPayload2 = ImGui.AcceptDragDropPayload(EntityDragPayload);
        if (entityPayload2 != null && entityPayload2->Delivery != 0 && _draggedEntityId.HasValue)
        {
            var rootEntities = _app.SelectedEntities
                .Where(e => e.Transform.Parent == null).ToList();
            MoveEntitiesToFolder(rootEntities, targetFolder.Id);
        }

        ImGui.EndDragDropTarget();
    }

    private void MoveEntitiesToFolder(List<Entity> rootEntities, string? folderId)
    {
        if (rootEntities.Count == 0) return;
        _app.RecordUndo();
        bool any = false;
        foreach (var e in rootEntities)
            any |= _app.SetRootEntityFolder(e, folderId);
        if (any) _app.RefreshUndoBaseline();
    }

    private void HandleKeyboardNavigation(List<Entity> visibleEntities)
    {
        if (!_isWindowFocused || ImGui.GetIO().WantTextInput)
            return;

        if (ImGui.IsKeyPressed(ImGuiKey.UpArrow))
            MoveSelectionByOffset(-1, visibleEntities);
        else if (ImGui.IsKeyPressed(ImGuiKey.DownArrow))
            MoveSelectionByOffset(1, visibleEntities);
    }

    private void MoveSelectionByOffset(int offset, List<Entity> visibleEntities)
    {
        if (visibleEntities.Count == 0 || _app.CurrentScene == null)
            return;

        var selected = _app.SelectedEntity;
        int currentIndex = selected == null
            ? (offset > 0 ? -1 : visibleEntities.Count)
            : visibleEntities.FindIndex(e => e.Id == selected.Id);

        if (currentIndex < 0)
            currentIndex = offset > 0 ? -1 : visibleEntities.Count;

        int targetIndex = Math.Clamp(currentIndex + offset, 0, visibleEntities.Count - 1);
        var target = visibleEntities[targetIndex];

        _app.SetSingleSelection(target);
        _rangeAnchorId = target.Id;
        _focusedEntityId = target.Id;
        _focusedFolderId = null;
    }

    private void HandleEntitySelection(Entity entity, List<Entity> hierarchyOrder)
    {
        var io = ImGui.GetIO();
        if (io.KeyShift)
        {
            var anchor = ResolveAnchorEntity(hierarchyOrder);
            if (anchor == null)
            {
                _app.SetSingleSelection(entity);
                _rangeAnchorId = entity.Id;
                return;
            }

            int anchorIndex = hierarchyOrder.FindIndex(e => e.Id == anchor.Id);
            int currentIndex = hierarchyOrder.FindIndex(e => e.Id == entity.Id);
            if (anchorIndex < 0 || currentIndex < 0)
            {
                _app.SetSingleSelection(entity);
                _rangeAnchorId = entity.Id;
                return;
            }

            int start = Math.Min(anchorIndex, currentIndex);
            int end = Math.Max(anchorIndex, currentIndex);
            var range = hierarchyOrder.Skip(start).Take(end - start + 1).ToList();
            _app.SetSelection(range);
            return;
        }

        if (io.KeyCtrl)
        {
            _app.ToggleSelection(entity);
            _rangeAnchorId = entity.Id;
            return;
        }

        _app.SetSingleSelection(entity);
        _rangeAnchorId = entity.Id;
    }

    private Entity? ResolveAnchorEntity(List<Entity> hierarchyOrder)
    {
        if (_rangeAnchorId.HasValue)
            return hierarchyOrder.FirstOrDefault(e => e.Id == _rangeAnchorId.Value);

        return _app.SelectedEntity;
    }

    private void DrawCreateEntityMenuItems(Entity? parent)
    {
        if (_app.CurrentScene == null)
            return;

        if (ImGui.MenuItem("Empty"))
            CreateEntity("Empty", parent);
        if (ImGui.MenuItem("Camera"))
            CreateEntity("Camera", parent, e => e.AddComponent<CameraComponent>());
        if (ImGui.MenuItem("Light"))
            CreateEntity("Light", parent, e => e.AddComponent<LightComponent>());
        if (ImGui.MenuItem("Cube"))
            CreateEntity("Cube", parent, e => e.AddComponent<CubePrimitive>());
        if (ImGui.MenuItem("Sphere"))
            CreateEntity("Sphere", parent, e => e.AddComponent<SpherePrimitive>());
        if (ImGui.MenuItem("Plane"))
            CreateEntity("Plane", parent, e => e.AddComponent<PlanePrimitive>());
        if (ImGui.MenuItem("Cylinder"))
            CreateEntity("Cylinder", parent, e => e.AddComponent<CylinderPrimitive>());
    }

    private void CreateEntity(string name, Entity? parent, Action<Entity>? setup = null)
    {
        if (_app.CurrentScene == null)
            return;

        _app.RecordUndo();
        var entity = _app.CurrentScene.CreateEntity(name);
        setup?.Invoke(entity);

        if (parent != null)
            _app.ReparentEntity(entity, parent);

        _app.SetSingleSelection(entity);
        _rangeAnchorId = entity.Id;
        _focusedEntityId = entity.Id;
        _focusedFolderId = null;
        _app.RefreshUndoBaseline();
    }

    private void RequestCreateFolder(string? parentFolderId)
    {
        _createFolderParentId = parentFolderId;
        _createFolderName = "New Folder";
        _openCreateFolderPopup = true;
        _focusCreateFolderInput = true;
    }

    private void DrawCreateFolderPopup(HierarchySceneState state)
    {
        if (_openCreateFolderPopup)
        {
            ImGui.OpenPopup("HierarchyCreateFolder");
            _openCreateFolderPopup = false;
        }

        if (!ImGui.BeginPopup("HierarchyCreateFolder"))
            return;

        if (_focusCreateFolderInput)
        {
            ImGui.SetKeyboardFocusHere();
            _focusCreateFolderInput = false;
        }

        bool submitted = ImGui.InputTextWithHint(
            "##NewFolderName",
            "Folder name (Enter to create)",
            ref _createFolderName,
            128,
            ImGuiInputTextFlags.EnterReturnsTrue);

        bool cancel = ImGui.IsItemActive() && ImGui.IsKeyPressed(ImGuiKey.Escape);
        if (cancel)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        if (submitted)
        {
            if (CreateFolder(state, _createFolderName, _createFolderParentId))
                ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }

    private bool CreateFolder(HierarchySceneState state, string name, string? parentFolderId)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return false;

        _app.RecordUndo();

        var folder = new HierarchyFolderState
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name,
            ParentFolderId = parentFolderId,
            Order = GetNextFolderOrder(state, parentFolderId)
        };

        state.Folders.Add(folder);
        state.ExpandedFolderIds.Add(folder.Id);
        if (!string.IsNullOrWhiteSpace(parentFolderId))
            state.ExpandedFolderIds.Add(parentFolderId);
        _app.MarkHierarchyStateDirty();

        _focusedFolderId = folder.Id;
        _focusedEntityId = null;
        _app.RefreshUndoBaseline();
        return true;
    }

    private void DeleteFolder(HierarchyFolderState folder, HierarchySceneState state)
    {
        _app.RecordUndo();

        foreach (var child in state.Folders.Where(f => string.Equals(f.ParentFolderId, folder.Id, StringComparison.OrdinalIgnoreCase)).ToList())
            child.ParentFolderId = folder.ParentFolderId;

        var reassignedEntityIds = state.RootEntityFolders
            .Where(kvp => string.Equals(kvp.Value, folder.Id, StringComparison.OrdinalIgnoreCase))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var entityId in reassignedEntityIds)
        {
            if (string.IsNullOrWhiteSpace(folder.ParentFolderId))
                state.RootEntityFolders.Remove(entityId);
            else
                state.RootEntityFolders[entityId] = folder.ParentFolderId;
        }

        state.ExpandedFolderIds.RemoveAll(id => string.Equals(id, folder.Id, StringComparison.OrdinalIgnoreCase));
        state.Folders.RemoveAll(f => string.Equals(f.Id, folder.Id, StringComparison.OrdinalIgnoreCase));

        if (string.Equals(_focusedFolderId, folder.Id, StringComparison.OrdinalIgnoreCase))
            _focusedFolderId = null;

        ReindexSiblingFolderOrder(state, folder.ParentFolderId);
        _app.MarkHierarchyStateDirty();
        _app.RefreshUndoBaseline();
    }

    private void BeginRenameEntity(Entity entity)
    {
        _renamingFolderId = null;
        _renamingEntityId = entity.Id;
        _renameBuffer = entity.Name;
        _focusRenameInput = true;
    }

    private void BeginRenameFolder(string folderId)
    {
        var state = _app.GetOrCreateHierarchySceneState();
        var folder = state.Folders.FirstOrDefault(f => string.Equals(f.Id, folderId, StringComparison.OrdinalIgnoreCase));
        if (folder == null)
            return;

        _renamingEntityId = null;
        _renamingFolderId = folder.Id;
        _renameBuffer = folder.Name;
        _focusRenameInput = true;
    }

    private void CommitEntityRename(Entity entity)
    {
        var name = _renameBuffer.Trim();
        if (!string.IsNullOrWhiteSpace(name) && !string.Equals(name, entity.Name, StringComparison.Ordinal))
        {
            _app.RecordUndo();
            entity.Name = name;
            _app.RefreshUndoBaseline();
        }

        CancelRename();
    }

    private void CommitFolderRename(HierarchyFolderState folder)
    {
        var name = _renameBuffer.Trim();
        if (!string.IsNullOrWhiteSpace(name) && !string.Equals(name, folder.Name, StringComparison.Ordinal))
        {
            _app.RecordUndo();
            folder.Name = name;
            _app.MarkHierarchyStateDirty();
            _app.RefreshUndoBaseline();
        }

        CancelRename();
    }

    private void CancelRename()
    {
        _renamingEntityId = null;
        _renamingFolderId = null;
        _renameBuffer = string.Empty;
        _focusRenameInput = false;
    }

    private static bool IsFolderDescendant(HierarchySceneState state, string ancestorFolderId, string candidateChildFolderId)
    {
        var lookup = state.Folders.ToDictionary(f => f.Id, StringComparer.OrdinalIgnoreCase);
        if (!lookup.TryGetValue(candidateChildFolderId, out var current))
            return false;

        while (!string.IsNullOrWhiteSpace(current.ParentFolderId))
        {
            if (string.Equals(current.ParentFolderId, ancestorFolderId, StringComparison.OrdinalIgnoreCase))
                return true;

            if (!lookup.TryGetValue(current.ParentFolderId, out current!))
                return false;
        }

        return false;
    }

    private static int GetNextFolderOrder(HierarchySceneState state, string? parentFolderId)
    {
        var siblings = state.Folders.Where(f => string.Equals(f.ParentFolderId, parentFolderId, StringComparison.OrdinalIgnoreCase));
        return siblings.Any() ? siblings.Max(f => f.Order) + 1 : 0;
    }

    private static void ReindexSiblingFolderOrder(HierarchySceneState state, string? parentFolderId)
    {
        int order = 0;
        foreach (var folder in state.Folders
                     .Where(f => string.Equals(f.ParentFolderId, parentFolderId, StringComparison.OrdinalIgnoreCase))
                     .OrderBy(f => f.Order)
                     .ThenBy(f => f.Name, StringComparer.OrdinalIgnoreCase))
        {
            folder.Order = order++;
        }
    }

    private bool ShouldSuppressHierarchyHotkeys()
    {
        return !_isWindowFocused || ImGui.GetIO().WantTextInput;
    }

    private static bool MatchesAllTokens(string text, IReadOnlyList<string> tokens)
    {
        if (tokens.Count == 0)
            return true;

        foreach (var token in tokens)
        {
            if (!text.Contains(token, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private static string[] ParseSearchTokens(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<string>();

        return query
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private bool EntityPassesFilters(Entity entity, HierarchySceneState state, Type? requiredComponent)
    {
        if (state.FilterActiveOnly && !entity.Active)
            return false;
        if (state.FilterInactiveOnly && entity.Active)
            return false;
        if (!MatchesPrefabFilter(entity, state.PrefabFilter))
            return false;
        if (requiredComponent != null && entity.GetComponent(requiredComponent) == null)
            return false;

        return true;
    }

    private static bool DrawPrefabFilterOption(HierarchySceneState state, HierarchyPrefabFilter mode)
    {
        bool selected = state.PrefabFilter == mode;
        if (!ImGui.Selectable(GetPrefabFilterLabel(mode), selected))
            return false;

        state.PrefabFilter = mode;
        return true;
    }

    private static string GetPrefabFilterLabel(HierarchyPrefabFilter mode)
    {
        return mode switch
        {
            HierarchyPrefabFilter.Any => "Any",
            HierarchyPrefabFilter.PrefabInstances => "Prefab Instances",
            HierarchyPrefabFilter.PrefabRoots => "Prefab Roots",
            HierarchyPrefabFilter.NonPrefabs => "Non-Prefabs",
            _ => "Any"
        };
    }

    private static bool MatchesPrefabFilter(Entity entity, HierarchyPrefabFilter mode)
    {
        bool isPrefabInstance = entity.Prefab != null && !entity.Prefab.AssetPath.IsEmpty;
        bool isPrefabRoot = isPrefabInstance && entity.Prefab!.IsRoot;

        return mode switch
        {
            HierarchyPrefabFilter.Any => true,
            HierarchyPrefabFilter.PrefabInstances => isPrefabInstance,
            HierarchyPrefabFilter.PrefabRoots => isPrefabRoot,
            HierarchyPrefabFilter.NonPrefabs => !isPrefabInstance,
            _ => true
        };
    }

    private static bool EntityMatchesSearch(Entity entity, IReadOnlyList<string> tokens)
    {
        if (tokens.Count == 0)
            return true;

        string haystack = entity.Name + " " + string.Join(' ', entity.Components.Select(c => c.GetType().Name));
        return MatchesAllTokens(haystack, tokens);
    }

    private bool EvaluateEntityVisibility(Entity entity, HierarchySceneState state, HierarchyRenderContext context)
    {
        if (context.EntityVisible.TryGetValue(entity.Id, out var cached))
            return cached;

        bool passesFilters = EntityPassesFilters(entity, state, context.RequiredComponentType);
        bool searchMatch = EntityMatchesSearch(entity, context.SearchTokens);
        bool selfMatch = passesFilters && searchMatch;

        bool hasVisibleDescendant = false;
        bool hasMatchDescendant = false;

        foreach (var child in entity.Transform.Children)
        {
            var childEntity = child.Entity;
            if (EvaluateEntityVisibility(childEntity, state, context))
                hasVisibleDescendant = true;

            if (context.EntitySubtreeMatch.GetValueOrDefault(childEntity.Id))
                hasMatchDescendant = true;
        }

        bool visible = context.HasSearch && state.ShowOnlyMatches
            ? selfMatch || hasVisibleDescendant
            : passesFilters || hasVisibleDescendant;

        context.EntityVisible[entity.Id] = visible;
        context.EntitySelfMatch[entity.Id] = selfMatch;
        context.EntitySubtreeMatch[entity.Id] = selfMatch || hasMatchDescendant;

        if (context.HasSearch && selfMatch)
            context.SearchMatchedEntities.Add(entity.Id);

        return visible;
    }

    private bool EvaluateFolderVisibility(HierarchyFolderState folder, HierarchySceneState state, HierarchyRenderContext context)
    {
        if (context.FolderVisible.TryGetValue(folder.Id, out var cached))
            return cached;

        bool folderMatch = context.HasSearch && MatchesAllTokens(folder.Name, context.SearchTokens);
        bool hasVisibleDescendants = false;
        bool hasMatchDescendants = false;

        foreach (var child in context.GetOrderedChildFolders(folder.Id))
        {
            if (EvaluateFolderVisibility(child, state, context))
                hasVisibleDescendants = true;

            if (context.FolderSubtreeMatch.GetValueOrDefault(child.Id))
                hasMatchDescendants = true;
        }

        foreach (var entity in context.GetFolderEntities(folder.Id))
        {
            if (EvaluateEntityVisibility(entity, state, context))
                hasVisibleDescendants = true;

            if (context.EntitySubtreeMatch.GetValueOrDefault(entity.Id))
                hasMatchDescendants = true;
        }

        bool visible = !context.HasSearch || !state.ShowOnlyMatches
            ? true
            : folderMatch || hasVisibleDescendants;

        context.FolderVisible[folder.Id] = visible;
        context.FolderSelfMatch[folder.Id] = folderMatch;
        context.FolderSubtreeMatch[folder.Id] = folderMatch || hasMatchDescendants;

        if (folderMatch)
            context.SearchMatchedFolders.Add(folder.Id);

        return visible;
    }

    private HierarchyRenderContext BuildRenderContext(Core.Scene.Scene scene, HierarchySceneState state)
    {
        var context = new HierarchyRenderContext
        {
            SearchTokens = ParseSearchTokens(state.SearchQuery),
            HasSearch = false,
            RequiredComponentType = string.IsNullOrWhiteSpace(state.RequiredComponentType)
                ? null
                : ComponentTypeResolver.Resolve(state.RequiredComponentType),
            FolderChildren = new Dictionary<string, List<HierarchyFolderState>>(StringComparer.OrdinalIgnoreCase),
            FolderRootEntities = new Dictionary<string, List<Entity>>(StringComparer.OrdinalIgnoreCase),
            RootFolders = new List<HierarchyFolderState>(),
            UnassignedRootEntities = new List<Entity>(),
            EntityVisible = new Dictionary<Guid, bool>(),
            EntitySelfMatch = new Dictionary<Guid, bool>(),
            EntitySubtreeMatch = new Dictionary<Guid, bool>(),
            FolderVisible = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase),
            FolderSelfMatch = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase),
            FolderSubtreeMatch = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase),
            SearchMatchedEntities = new HashSet<Guid>(),
            SearchMatchedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        };

        context.HasSearch = context.SearchTokens.Length > 0;

        var foldersById = state.Folders.ToDictionary(f => f.Id, StringComparer.OrdinalIgnoreCase);

        foreach (var folder in state.Folders)
        {
            var parentKey = string.IsNullOrWhiteSpace(folder.ParentFolderId) ? RootFolderKey : folder.ParentFolderId;
            if (!context.FolderChildren.TryGetValue(parentKey, out var children))
            {
                children = new List<HierarchyFolderState>();
                context.FolderChildren[parentKey] = children;
            }
            children.Add(folder);
        }

        if (context.FolderChildren.TryGetValue(RootFolderKey, out var rootFolders))
            context.RootFolders = rootFolders.OrderBy(f => f.Order).ThenBy(f => f.Name, StringComparer.OrdinalIgnoreCase).ToList();

        foreach (var kvp in context.FolderChildren)
            kvp.Value.Sort((a, b) => a.Order != b.Order ? a.Order.CompareTo(b.Order) : StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name));

        var rootEntities = scene.Entities.Where(e => e.Transform.Parent == null).ToList();
        foreach (var entity in rootEntities)
        {
            var folderId = _app.GetRootEntityFolder(entity);
            if (!string.IsNullOrWhiteSpace(folderId) && foldersById.ContainsKey(folderId))
            {
                if (!context.FolderRootEntities.TryGetValue(folderId, out var entities))
                {
                    entities = new List<Entity>();
                    context.FolderRootEntities[folderId] = entities;
                }

                entities.Add(entity);
            }
            else
            {
                context.UnassignedRootEntities.Add(entity);
            }

            EvaluateEntityVisibility(entity, state, context);
        }

        foreach (var rootFolder in context.RootFolders)
            EvaluateFolderVisibility(rootFolder, state, context);

        return context;
    }

    private static unsafe void SetDummyPayload(string payloadType)
    {
        ImGui.SetDragDropPayload(payloadType, (void*)null, 0);
    }

    private sealed class HierarchyRenderContext
    {
        public required string[] SearchTokens { get; init; }
        public required bool HasSearch { get; set; }
        public required Type? RequiredComponentType { get; init; }

        public required Dictionary<string, List<HierarchyFolderState>> FolderChildren { get; init; }
        public required Dictionary<string, List<Entity>> FolderRootEntities { get; init; }
        public required List<HierarchyFolderState> RootFolders { get; set; }
        public required List<Entity> UnassignedRootEntities { get; set; }

        public required Dictionary<Guid, bool> EntityVisible { get; init; }
        public required Dictionary<Guid, bool> EntitySelfMatch { get; init; }
        public required Dictionary<Guid, bool> EntitySubtreeMatch { get; init; }

        public required Dictionary<string, bool> FolderVisible { get; init; }
        public required Dictionary<string, bool> FolderSelfMatch { get; init; }
        public required Dictionary<string, bool> FolderSubtreeMatch { get; init; }

        public required HashSet<Guid> SearchMatchedEntities { get; init; }
        public required HashSet<string> SearchMatchedFolders { get; init; }

        public IReadOnlyList<HierarchyFolderState> GetOrderedChildFolders(string folderId)
        {
            return FolderChildren.TryGetValue(folderId, out var children)
                ? children
                : Array.Empty<HierarchyFolderState>();
        }

        public IReadOnlyList<Entity> GetFolderEntities(string folderId)
        {
            return FolderRootEntities.TryGetValue(folderId, out var entities)
                ? entities
                : Array.Empty<Entity>();
        }
    }
}
