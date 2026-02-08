using System.Numerics;
using Hexa.NET.ImGui;

namespace FrinkyEngine.Editor;

public class NotificationManager
{
    public static NotificationManager Instance { get; } = new();

    private readonly List<EditorNotification> _notifications = new();
    private readonly object _lock = new();

    private const float FadeOutDuration = 0.5f;
    private static readonly char[] SpinnerChars = { '|', '/', '-', '\\' };

    public EditorNotification Post(string message, NotificationType type, float duration = 4f)
    {
        var notification = new EditorNotification(message, type, duration);
        lock (_lock)
        {
            _notifications.Add(notification);
        }
        return notification;
    }

    public EditorNotification PostPersistent(string message, NotificationType type)
    {
        var notification = new EditorNotification(message, type, 0f);
        lock (_lock)
        {
            _notifications.Add(notification);
        }
        return notification;
    }

    public void Complete(EditorNotification notification, string? newMessage = null, NotificationType? newType = null)
    {
        lock (_lock)
        {
            notification.IsCompleted = true;
            notification.Elapsed = 0f;
            notification.Duration = 3f;
            if (newMessage != null) notification.Message = newMessage;
            if (newType != null) notification.Type = newType.Value;
        }
    }

    public void Update(float dt)
    {
        lock (_lock)
        {
            for (int i = _notifications.Count - 1; i >= 0; i--)
            {
                var n = _notifications[i];

                if (n.IsPersistent)
                {
                    n.Elapsed += dt;
                    n.Alpha = 1f;
                    continue;
                }

                n.Elapsed += dt;

                float remaining = n.Duration - n.Elapsed;
                if (remaining <= 0)
                {
                    _notifications.RemoveAt(i);
                    continue;
                }

                n.Alpha = remaining < FadeOutDuration ? remaining / FadeOutDuration : 1f;
            }
        }
    }

    public void Draw()
    {
        List<EditorNotification> snapshot;
        lock (_lock)
        {
            if (_notifications.Count == 0) return;
            snapshot = new List<EditorNotification>(_notifications);
        }

        var viewport = ImGui.GetMainViewport();
        float padding = 16f;
        float yOffset = padding;

        for (int i = snapshot.Count - 1; i >= 0; i--)
        {
            var n = snapshot[i];
            DrawNotification(n, viewport, padding, ref yOffset);
        }
    }

    private void DrawNotification(EditorNotification n, ImGuiViewportPtr viewport, float padding, ref float yOffset)
    {
        var (icon, color) = n.Type switch
        {
            NotificationType.Info => ("[i]", new Vector4(0.3f, 0.5f, 1f, 1f)),
            NotificationType.Success => ("[OK]", new Vector4(0.3f, 0.9f, 0.3f, 1f)),
            NotificationType.Warning => ("[!]", new Vector4(1f, 0.85f, 0.2f, 1f)),
            NotificationType.Error => ("[X]", new Vector4(1f, 0.3f, 0.3f, 1f)),
            _ => ("[i]", new Vector4(0.3f, 0.5f, 1f, 1f))
        };

        string displayText;
        if (n.IsPersistent)
        {
            int spinnerIndex = (int)(n.Elapsed * 4f) % SpinnerChars.Length;
            displayText = $"{icon} {n.Message} {SpinnerChars[spinnerIndex]}";
        }
        else
        {
            displayText = $"{icon} {n.Message}";
        }

        var textSize = ImGui.CalcTextSize(displayText);
        float windowWidth = textSize.X + 24f;
        float windowHeight = textSize.Y + 16f;

        float posX = viewport.WorkPos.X + viewport.WorkSize.X - windowWidth - padding;
        float posY = viewport.WorkPos.Y + viewport.WorkSize.Y - yOffset - windowHeight;

        ImGui.SetNextWindowPos(new Vector2(posX, posY));
        ImGui.SetNextWindowSize(new Vector2(windowWidth, windowHeight));
        ImGui.SetNextWindowBgAlpha(0.9f * n.Alpha);

        var flags = ImGuiWindowFlags.NoDecoration
            | ImGuiWindowFlags.NoInputs
            | ImGuiWindowFlags.AlwaysAutoResize
            | ImGuiWindowFlags.NoSavedSettings
            | ImGuiWindowFlags.NoFocusOnAppearing
            | ImGuiWindowFlags.NoNav;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 6f);
        if (ImGui.Begin($"##notification_{n.Id}", flags))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, color * new Vector4(1, 1, 1, n.Alpha));
            ImGui.TextUnformatted(displayText);
            ImGui.PopStyleColor();
        }
        ImGui.End();
        ImGui.PopStyleVar();

        yOffset += windowHeight + 4f;
    }
}
