using ImGuiNET;

namespace FrinkyEngine.Editor;

public enum EditorAction
{
    NewScene,
    OpenScene,
    SaveScene,
    SaveSceneAs,
    Undo,
    Redo,
    BuildScripts,
    PlayStop,
    DeleteEntity,
    DuplicateEntity,
    RenameEntity,
    NewProject,
    GizmoTranslate,
    GizmoRotate,
    GizmoScale,
    GizmoToggleSpace
}

public struct Keybind
{
    public ImGuiKey Key { get; set; }
    public bool Ctrl { get; set; }
    public bool Shift { get; set; }
    public bool Alt { get; set; }

    public Keybind(ImGuiKey key, bool ctrl = false, bool shift = false, bool alt = false)
    {
        Key = key;
        Ctrl = ctrl;
        Shift = shift;
        Alt = alt;
    }

    public readonly bool IsPressed()
    {
        if (!ImGui.IsKeyPressed(Key))
            return false;

        var io = ImGui.GetIO();
        if (io.KeyCtrl != Ctrl) return false;
        if (io.KeyShift != Shift) return false;
        if (io.KeyAlt != Alt) return false;

        return true;
    }

    public readonly string ToDisplayString()
    {
        var parts = new List<string>();
        if (Ctrl) parts.Add("Ctrl");
        if (Alt) parts.Add("Alt");
        if (Shift) parts.Add("Shift");
        parts.Add(KeyName());
        return string.Join("+", parts);
    }

    private readonly string KeyName()
    {
        return Key switch
        {
            ImGuiKey.Delete => "Del",
            ImGuiKey.F2 => "F2",
            _ => Key.ToString()
        };
    }
}

public class KeybindEntry
{
    public string Action { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public bool Ctrl { get; set; }
    public bool Shift { get; set; }
    public bool Alt { get; set; }
}

public class KeybindConfig
{
    public List<KeybindEntry> Keybinds { get; set; } = new();
}
