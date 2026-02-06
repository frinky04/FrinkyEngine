namespace FrinkyEngine.Editor;

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

public class EditorNotification
{
    private static int _nextId;

    public int Id { get; } = Interlocked.Increment(ref _nextId);
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public float Duration { get; set; }
    public float Elapsed { get; set; }
    public float Alpha { get; set; } = 1f;
    public bool IsCompleted { get; set; }

    public bool IsPersistent => Duration <= 0 && !IsCompleted;

    public EditorNotification(string message, NotificationType type, float duration)
    {
        Message = message;
        Type = type;
        Duration = duration;
    }
}
