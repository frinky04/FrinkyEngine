using System.Numerics;
using FrinkyEngine.Core.Rendering;
using Hexa.NET.ImGui;

namespace FrinkyEngine.Editor;

internal sealed class DebugDrawOverlay : DebugDraw.IDebugDrawBackend
{
    private sealed class DebugMessage
    {
        public required string Text;
        public float Remaining;
        public Vector4 Color;
        public string? Key;
    }

    private readonly List<DebugMessage> _messages = new();

    public void PrintString(string message, float duration, Vector4 color, string? key)
    {
        if (key != null)
        {
            for (int i = 0; i < _messages.Count; i++)
            {
                if (string.Equals(_messages[i].Key, key, StringComparison.Ordinal))
                {
                    _messages[i].Text = message;
                    _messages[i].Remaining = duration;
                    _messages[i].Color = color;
                    return;
                }
            }
        }

        _messages.Add(new DebugMessage
        {
            Text = message,
            Remaining = duration,
            Color = color,
            Key = key
        });
    }

    public void Clear()
    {
        _messages.Clear();
    }

    public void Update(float dt)
    {
        for (int i = _messages.Count - 1; i >= 0; i--)
        {
            _messages[i].Remaining -= dt;
            if (_messages[i].Remaining <= 0f)
                _messages.RemoveAt(i);
        }
    }

    public void Draw()
    {
        if (_messages.Count == 0)
            return;

        ImGui.SetNextWindowPos(new Vector2(10f, 10f), ImGuiCond.Always);
        ImGui.SetNextWindowBgAlpha(0f);

        var flags = ImGuiWindowFlags.NoDecoration
                    | ImGuiWindowFlags.AlwaysAutoResize
                    | ImGuiWindowFlags.NoFocusOnAppearing
                    | ImGuiWindowFlags.NoNav
                    | ImGuiWindowFlags.NoMove
                    | ImGuiWindowFlags.NoSavedSettings
                    | ImGuiWindowFlags.NoInputs;

        if (ImGui.Begin("##DebugDrawOverlay", flags))
        {
            foreach (var msg in _messages)
            {
                float alpha = msg.Remaining < 0.5f ? msg.Remaining / 0.5f : 1f;
                var color = msg.Color with { W = msg.Color.W * alpha };

                // Draw shadow for readability
                var cursorPos = ImGui.GetCursorScreenPos();
                var drawList = ImGui.GetWindowDrawList();
                var shadowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.7f * alpha));
                drawList.AddText(cursorPos + new Vector2(1f, 1f), shadowColor, msg.Text);

                ImGui.PushStyleColor(ImGuiCol.Text, color);
                ImGui.TextUnformatted(msg.Text);
                ImGui.PopStyleColor();
            }
        }
        ImGui.End();
    }
}
