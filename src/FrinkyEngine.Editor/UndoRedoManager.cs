using FrinkyEngine.Core.Serialization;

namespace FrinkyEngine.Editor;

public record UndoSnapshot(string SceneJson, List<Guid> SelectedEntityIds);

public class UndoRedoManager
{
    private const int MaxHistory = 50;

    private readonly List<UndoSnapshot> _undoStack = new();
    private readonly List<UndoSnapshot> _redoStack = new();

    private string? _currentSnapshot;
    private List<Guid> _currentSelectedEntityIds = new();

    // Batch state for continuous edits (gizmo drags, slider drags)
    private bool _isBatching;
    private string? _batchStartSnapshot;
    private List<Guid>? _batchStartSelectedEntityIds;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public void SetBaseline(Core.Scene.Scene? scene, IReadOnlyList<Guid> selectedEntityIds)
    {
        if (scene == null) return;
        _currentSnapshot = SceneSerializer.SerializeToString(scene);
        _currentSelectedEntityIds = selectedEntityIds.ToList();
    }

    public void RecordUndo(IReadOnlyList<Guid>? currentSelectedEntityIds = null)
    {
        if (_currentSnapshot == null) return;

        var selectedIds = currentSelectedEntityIds?.ToList() ?? _currentSelectedEntityIds.ToList();
        _undoStack.Add(new UndoSnapshot(_currentSnapshot, selectedIds));
        if (_undoStack.Count > MaxHistory)
            _undoStack.RemoveAt(0);

        _redoStack.Clear();
    }

    public void RefreshBaseline(Core.Scene.Scene? scene, IReadOnlyList<Guid> selectedEntityIds)
    {
        if (scene == null) return;
        _currentSnapshot = SceneSerializer.SerializeToString(scene);
        _currentSelectedEntityIds = selectedEntityIds.ToList();
    }

    public void BeginBatch()
    {
        if (_isBatching) return;
        _isBatching = true;
        _batchStartSnapshot = _currentSnapshot;
        _batchStartSelectedEntityIds = _currentSelectedEntityIds.ToList();
    }

    public void EndBatch(Core.Scene.Scene? scene, IReadOnlyList<Guid> selectedEntityIds)
    {
        if (!_isBatching) return;
        _isBatching = false;

        if (_batchStartSnapshot == null) return;

        // Push the pre-batch state as the undo point
        _undoStack.Add(new UndoSnapshot(_batchStartSnapshot, _batchStartSelectedEntityIds?.ToList() ?? new List<Guid>()));
        if (_undoStack.Count > MaxHistory)
            _undoStack.RemoveAt(0);

        _redoStack.Clear();
        _batchStartSnapshot = null;
        _batchStartSelectedEntityIds = null;

        // Refresh baseline to current state
        RefreshBaseline(scene, selectedEntityIds);
    }

    public void Undo(EditorApplication app)
    {
        if (!CanUndo || app.CurrentScene == null) return;

        // Push current state onto redo stack
        if (_currentSnapshot != null)
            _redoStack.Add(new UndoSnapshot(_currentSnapshot, app.GetSelectedEntityIds()));

        var snapshot = _undoStack[^1];
        _undoStack.RemoveAt(_undoStack.Count - 1);

        RestoreSnapshot(app, snapshot);
        NotificationManager.Instance.Post("Undo", NotificationType.Info, 1.5f);
    }

    public void Redo(EditorApplication app)
    {
        if (!CanRedo || app.CurrentScene == null) return;

        // Push current state onto undo stack
        if (_currentSnapshot != null)
            _undoStack.Add(new UndoSnapshot(_currentSnapshot, app.GetSelectedEntityIds()));

        var snapshot = _redoStack[^1];
        _redoStack.RemoveAt(_redoStack.Count - 1);

        RestoreSnapshot(app, snapshot);
        NotificationManager.Instance.Post("Redo", NotificationType.Info, 1.5f);
    }

    private void RestoreSnapshot(EditorApplication app, UndoSnapshot snapshot)
    {
        var restored = SceneSerializer.DeserializeFromString(snapshot.SceneJson);
        if (restored == null) return;

        // Preserve scene metadata
        if (app.CurrentScene != null)
        {
            restored.Name = app.CurrentScene.Name;
            restored.FilePath = app.CurrentScene.FilePath;
        }

        app.CurrentScene = restored;
        Core.Scene.SceneManager.Instance.SetActiveScene(restored);

        var selectedEntities = new List<Core.ECS.Entity>();
        foreach (var selectedId in snapshot.SelectedEntityIds)
        {
            var selectedEntity = FindEntityById(restored, selectedId);
            if (selectedEntity != null)
                selectedEntities.Add(selectedEntity);
        }
        app.SetSelection(selectedEntities);

        // Update baseline
        _currentSnapshot = snapshot.SceneJson;
        _currentSelectedEntityIds = snapshot.SelectedEntityIds.ToList();
    }

    private static Core.ECS.Entity? FindEntityById(Core.Scene.Scene scene, Guid id)
    {
        foreach (var entity in scene.Entities)
        {
            if (entity.Id == id) return entity;
            var found = FindEntityByIdRecursive(entity, id);
            if (found != null) return found;
        }
        return null;
    }

    private static Core.ECS.Entity? FindEntityByIdRecursive(Core.ECS.Entity entity, Guid id)
    {
        foreach (var child in entity.Transform.Children)
        {
            if (child.Entity.Id == id) return child.Entity;
            var found = FindEntityByIdRecursive(child.Entity, id);
            if (found != null) return found;
        }
        return null;
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        _currentSnapshot = null;
        _currentSelectedEntityIds.Clear();
        _isBatching = false;
        _batchStartSnapshot = null;
        _batchStartSelectedEntityIds = null;
    }
}
