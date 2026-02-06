using FrinkyEngine.Core.Serialization;

namespace FrinkyEngine.Editor;

public record UndoSnapshot(string SceneJson, Guid? SelectedEntityId);

public class UndoRedoManager
{
    private const int MaxHistory = 50;

    private readonly List<UndoSnapshot> _undoStack = new();
    private readonly List<UndoSnapshot> _redoStack = new();

    private string? _currentSnapshot;
    private Guid? _currentSelectedEntityId;

    // Batch state for continuous edits (gizmo drags, slider drags)
    private bool _isBatching;
    private string? _batchStartSnapshot;
    private Guid? _batchStartSelectedEntityId;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public void SetBaseline(Core.Scene.Scene? scene, Guid? selectedEntityId)
    {
        if (scene == null) return;
        _currentSnapshot = SceneSerializer.SerializeToString(scene);
        _currentSelectedEntityId = selectedEntityId;
    }

    public void RecordUndo()
    {
        if (_currentSnapshot == null) return;

        _undoStack.Add(new UndoSnapshot(_currentSnapshot, _currentSelectedEntityId));
        if (_undoStack.Count > MaxHistory)
            _undoStack.RemoveAt(0);

        _redoStack.Clear();
    }

    public void RefreshBaseline(Core.Scene.Scene? scene, Guid? selectedEntityId)
    {
        if (scene == null) return;
        _currentSnapshot = SceneSerializer.SerializeToString(scene);
        _currentSelectedEntityId = selectedEntityId;
    }

    public void BeginBatch()
    {
        if (_isBatching) return;
        _isBatching = true;
        _batchStartSnapshot = _currentSnapshot;
        _batchStartSelectedEntityId = _currentSelectedEntityId;
    }

    public void EndBatch(Core.Scene.Scene? scene, Guid? selectedEntityId)
    {
        if (!_isBatching) return;
        _isBatching = false;

        if (_batchStartSnapshot == null) return;

        // Push the pre-batch state as the undo point
        _undoStack.Add(new UndoSnapshot(_batchStartSnapshot, _batchStartSelectedEntityId));
        if (_undoStack.Count > MaxHistory)
            _undoStack.RemoveAt(0);

        _redoStack.Clear();
        _batchStartSnapshot = null;
        _batchStartSelectedEntityId = null;

        // Refresh baseline to current state
        RefreshBaseline(scene, selectedEntityId);
    }

    public void Undo(EditorApplication app)
    {
        if (!CanUndo || app.CurrentScene == null) return;

        // Push current state onto redo stack
        if (_currentSnapshot != null)
            _redoStack.Add(new UndoSnapshot(_currentSnapshot, _currentSelectedEntityId));

        var snapshot = _undoStack[^1];
        _undoStack.RemoveAt(_undoStack.Count - 1);

        RestoreSnapshot(app, snapshot);
    }

    public void Redo(EditorApplication app)
    {
        if (!CanRedo || app.CurrentScene == null) return;

        // Push current state onto undo stack
        if (_currentSnapshot != null)
            _undoStack.Add(new UndoSnapshot(_currentSnapshot, _currentSelectedEntityId));

        var snapshot = _redoStack[^1];
        _redoStack.RemoveAt(_redoStack.Count - 1);

        RestoreSnapshot(app, snapshot);
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

        // Restore selection by entity Id
        app.SelectedEntity = null;
        if (snapshot.SelectedEntityId.HasValue)
        {
            app.SelectedEntity = FindEntityById(restored, snapshot.SelectedEntityId.Value);
        }

        // Update baseline
        _currentSnapshot = snapshot.SceneJson;
        _currentSelectedEntityId = snapshot.SelectedEntityId;
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
        _currentSelectedEntityId = null;
        _isBatching = false;
        _batchStartSnapshot = null;
        _batchStartSelectedEntityId = null;
    }
}
