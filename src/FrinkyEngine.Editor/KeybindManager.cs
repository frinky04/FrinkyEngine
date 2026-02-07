using System.Text.Json;
using FrinkyEngine.Core.Rendering;
using ImGuiNET;

namespace FrinkyEngine.Editor;

public class KeybindManager
{
    public static KeybindManager Instance { get; } = new();

    private readonly Dictionary<EditorAction, Keybind> _bindings = new();
    private readonly Dictionary<EditorAction, Action> _actions = new();
    private string? _configPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private KeybindManager()
    {
        ResetToDefaults();
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
        if (ImGui.GetIO().WantTextInput)
            return;

        foreach (var (action, keybind) in _bindings)
        {
            if (keybind.IsPressed() && _actions.TryGetValue(action, out var callback))
            {
                callback();
                return;
            }
        }
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

        _bindings[EditorAction.NewScene] = new Keybind(ImGuiKey.N, ctrl: true);
        _bindings[EditorAction.OpenScene] = new Keybind(ImGuiKey.O, ctrl: true);
        _bindings[EditorAction.SaveScene] = new Keybind(ImGuiKey.S, ctrl: true);
        _bindings[EditorAction.SaveSceneAs] = new Keybind(ImGuiKey.S, ctrl: true, shift: true);
        _bindings[EditorAction.Undo] = new Keybind(ImGuiKey.Z, ctrl: true);
        _bindings[EditorAction.Redo] = new Keybind(ImGuiKey.Y, ctrl: true);
        _bindings[EditorAction.BuildScripts] = new Keybind(ImGuiKey.B, ctrl: true);
        _bindings[EditorAction.PlayStop] = new Keybind(ImGuiKey.P, ctrl: true);
        _bindings[EditorAction.DeleteEntity] = new Keybind(ImGuiKey.Delete);
        _bindings[EditorAction.DuplicateEntity] = new Keybind(ImGuiKey.D, ctrl: true);
        _bindings[EditorAction.RenameEntity] = new Keybind(ImGuiKey.F2);
        _bindings[EditorAction.NewProject] = new Keybind(ImGuiKey.N, ctrl: true, shift: true);
        _bindings[EditorAction.GizmoTranslate] = new Keybind(ImGuiKey._1);
        _bindings[EditorAction.GizmoRotate] = new Keybind(ImGuiKey._2);
        _bindings[EditorAction.GizmoScale] = new Keybind(ImGuiKey._3);
        _bindings[EditorAction.GizmoToggleSpace] = new Keybind(ImGuiKey.X);
        _bindings[EditorAction.DeselectEntity] = new Keybind(ImGuiKey.Escape);
        _bindings[EditorAction.SelectAllEntities] = new Keybind(ImGuiKey.A, ctrl: true);
        _bindings[EditorAction.ExpandSelection] = new Keybind(ImGuiKey.RightArrow);
        _bindings[EditorAction.CollapseSelection] = new Keybind(ImGuiKey.LeftArrow);
        _bindings[EditorAction.FocusHierarchySearch] = new Keybind(ImGuiKey.F, ctrl: true);
        _bindings[EditorAction.ToggleGameView] = new Keybind(ImGuiKey.G);
        _bindings[EditorAction.OpenAssetsFolder] = new Keybind(ImGuiKey.O, ctrl: true, shift: true);
        _bindings[EditorAction.OpenProjectInVSCode] = new Keybind(ImGuiKey.V, ctrl: true, shift: true);
        _bindings[EditorAction.ExportGame] = new Keybind(ImGuiKey.E, ctrl: true, shift: true);
        _bindings[EditorAction.CreatePrefabFromSelection] = new Keybind(ImGuiKey.P, ctrl: true, shift: true);
        _bindings[EditorAction.ApplyPrefab] = new Keybind(ImGuiKey.P, ctrl: true, alt: true);
        _bindings[EditorAction.RevertPrefab] = new Keybind(ImGuiKey.R, ctrl: true, alt: true);
        _bindings[EditorAction.MakeUniquePrefab] = new Keybind(ImGuiKey.U, ctrl: true, shift: true);
        _bindings[EditorAction.UnpackPrefab] = new Keybind(ImGuiKey.K, ctrl: true, alt: true);
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
