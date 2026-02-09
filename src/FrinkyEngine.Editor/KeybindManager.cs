using System.Text.Json;
using FrinkyEngine.Core.Rendering;
using Hexa.NET.ImGui;

namespace FrinkyEngine.Editor;

public class KeybindManager
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly Dictionary<EditorAction, Keybind> Defaults = BuildDefaults();

    public static KeybindManager Instance { get; } = new();

    private readonly Dictionary<EditorAction, Keybind> _bindings = new();
    private readonly Dictionary<EditorAction, Action> _actions = new();
    private string? _configPath;

    public bool IsCapturingKeybind { get; set; }

    private KeybindManager()
    {
        ResetToDefaults();
    }

    private static Dictionary<EditorAction, Keybind> BuildDefaults()
    {
        return new Dictionary<EditorAction, Keybind>
        {
            [EditorAction.NewScene] = new Keybind(ImGuiKey.N, ctrl: true),
            [EditorAction.OpenScene] = new Keybind(ImGuiKey.O, ctrl: true),
            [EditorAction.SaveScene] = new Keybind(ImGuiKey.S, ctrl: true),
            [EditorAction.SaveSceneAs] = new Keybind(ImGuiKey.S, ctrl: true, shift: true),
            [EditorAction.Undo] = new Keybind(ImGuiKey.Z, ctrl: true),
            [EditorAction.Redo] = new Keybind(ImGuiKey.Y, ctrl: true),
            [EditorAction.BuildScripts] = new Keybind(ImGuiKey.B, ctrl: true),
            [EditorAction.PlayStop] = new Keybind(ImGuiKey.F5),
            [EditorAction.SimulateStop] = new Keybind(ImGuiKey.F5, alt: true),
            [EditorAction.DeleteEntity] = new Keybind(ImGuiKey.Delete),
            [EditorAction.DuplicateEntity] = new Keybind(ImGuiKey.D, ctrl: true),
            [EditorAction.RenameEntity] = new Keybind(ImGuiKey.F2),
            [EditorAction.NewProject] = new Keybind(ImGuiKey.N, ctrl: true, shift: true),
            [EditorAction.GizmoTranslate] = new Keybind(ImGuiKey.Key1),
            [EditorAction.GizmoRotate] = new Keybind(ImGuiKey.Key2),
            [EditorAction.GizmoScale] = new Keybind(ImGuiKey.Key3),
            [EditorAction.GizmoToggleSpace] = new Keybind(ImGuiKey.X),
            [EditorAction.DeselectEntity] = new Keybind(ImGuiKey.Escape),
            [EditorAction.SelectAllEntities] = new Keybind(ImGuiKey.A, ctrl: true),
            [EditorAction.ExpandSelection] = new Keybind(ImGuiKey.RightArrow),
            [EditorAction.CollapseSelection] = new Keybind(ImGuiKey.LeftArrow),
            [EditorAction.FocusHierarchySearch] = new Keybind(ImGuiKey.F, ctrl: true),
            [EditorAction.ToggleGameView] = new Keybind(ImGuiKey.G),
            [EditorAction.TogglePhysicsHitboxPreview] = new Keybind(ImGuiKey.F8),
            [EditorAction.OpenAssetsFolder] = new Keybind(ImGuiKey.O, ctrl: true, shift: true),
            [EditorAction.OpenProjectInVSCode] = new Keybind(ImGuiKey.V, ctrl: true, shift: true),
            [EditorAction.ExportGame] = new Keybind(ImGuiKey.E, ctrl: true, shift: true),
            [EditorAction.CreatePrefabFromSelection] = new Keybind(ImGuiKey.M, ctrl: true, shift: true),
            [EditorAction.ApplyPrefab] = new Keybind(ImGuiKey.P, ctrl: true, alt: true),
            [EditorAction.RevertPrefab] = new Keybind(ImGuiKey.R, ctrl: true, alt: true),
            [EditorAction.MakeUniquePrefab] = new Keybind(ImGuiKey.U, ctrl: true, shift: true),
            [EditorAction.UnpackPrefab] = new Keybind(ImGuiKey.K, ctrl: true, alt: true),
            [EditorAction.TogglePlayModeCursorLock] = new Keybind(ImGuiKey.F1, shift: true),
            [EditorAction.FrameSelected] = new Keybind(ImGuiKey.F),
        };
    }

    public void RegisterAction(EditorAction action, Action callback)
    {
        _actions[action] = callback;
    }

    public string GetShortcutText(EditorAction action)
    {
        return _bindings.TryGetValue(action, out var keybind) ? keybind.ToDisplayString() : string.Empty;
    }

    public void ProcessKeybinds()
    {
        if (IsCapturingKeybind)
            return;

        if (ImGui.GetIO().WantTextInput)
            return;

        foreach (var (action, keybind) in _bindings)
        {
            if (keybind.IsPressed() && _actions.TryGetValue(action, out var callback))
            {
                if (!CanProcessActionInCurrentMode(action))
                    continue;

                callback();
                return;
            }
        }
    }

    private static bool CanProcessActionInCurrentMode(EditorAction action)
    {
        var app = EditorApplication.Instance;
        if (app == null)
            return true;

        if (app.Mode != EditorMode.Play)
            return true;

        // In Play mode, only allow these specific actions
        return action is EditorAction.PlayStop
            or EditorAction.SimulateStop
            or EditorAction.ToggleGameView
            or EditorAction.TogglePlayModeCursorLock
            or EditorAction.FrameSelected
            or EditorAction.DeselectEntity
            or EditorAction.TogglePhysicsHitboxPreview;
    }

    public Keybind GetBinding(EditorAction action) =>
        _bindings.TryGetValue(action, out var kb) ? kb : default;

    public Keybind GetDefaultBinding(EditorAction action) =>
        Defaults.TryGetValue(action, out var kb) ? kb : default;

    public void SetBinding(EditorAction action, Keybind keybind)
    {
        _bindings[action] = keybind;
        SaveConfig();
    }

    public void ResetBinding(EditorAction action)
    {
        if (Defaults.TryGetValue(action, out var defaultKb))
            _bindings[action] = defaultKb;
        SaveConfig();
    }

    public List<EditorAction> FindConflicts(EditorAction exclude, Keybind keybind)
    {
        var conflicts = new List<EditorAction>();
        foreach (var (action, binding) in _bindings)
        {
            if (action != exclude && binding == keybind)
                conflicts.Add(action);
        }
        return conflicts;
    }

    public static string FormatActionName(EditorAction action)
    {
        var name = action.ToString();
        var sb = new System.Text.StringBuilder(name.Length + 4);
        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
                sb.Append(' ');
            sb.Append(name[i]);
        }
        return sb.ToString();
    }

    public void LoadConfig(string? projectDirectory)
    {
        if (projectDirectory == null)
            return;

        var configDir = Path.Combine(projectDirectory, ".frinky");
        _configPath = Path.Combine(configDir, "keybinds.json");

        if (File.Exists(_configPath))
        {
            try
            {
                var json = File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<KeybindConfig>(json, JsonOptions);
                if (config != null)
                    ApplyConfig(config);
            }
            catch (Exception ex)
            {
                FrinkyLog.Warning($"Failed to load keybinds config: {ex.Message}");
            }
        }
        else
        {
            // Create default config file so users can discover and edit it
            Directory.CreateDirectory(configDir);
            SaveConfig();
        }
    }

    public void SaveConfig()
    {
        if (_configPath == null)
            return;

        try
        {
            var config = new KeybindConfig();
            foreach (var (action, keybind) in _bindings)
            {
                config.Keybinds.Add(new KeybindEntry
                {
                    Action = action.ToString(),
                    Key = keybind.Key.ToString(),
                    Ctrl = keybind.Ctrl,
                    Shift = keybind.Shift,
                    Alt = keybind.Alt
                });
            }

            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"Failed to save keybinds config: {ex.Message}");
        }
    }

    public void ResetToDefaults()
    {
        _bindings.Clear();
        foreach (var (action, keybind) in Defaults)
            _bindings[action] = keybind;
    }

    private void ApplyConfig(KeybindConfig config)
    {
        foreach (var entry in config.Keybinds)
        {
            if (Enum.TryParse<EditorAction>(entry.Action, out var action) &&
                Enum.TryParse<ImGuiKey>(entry.Key, out var key))
            {
                _bindings[action] = new Keybind(key, entry.Ctrl, entry.Shift, entry.Alt);
            }
        }
    }
}
