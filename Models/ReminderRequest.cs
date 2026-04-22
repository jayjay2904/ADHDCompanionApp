namespace ADHDCompanionApp.Models;

public class ReminderRequest
{
    public string ReminderKey { get; set; } = string.Empty;
    public int NotificationId { get; set; }
    public ReminderType Type { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public DateTime TriggerTime { get; set; }

    /// <summary>
    /// If set, the reminder should repeat daily at this time.
    /// </summary>
    public TimeSpan? RepeatTimeOfDay { get; set; }

    /// <summary>
    /// Optional user-facing label or owner, e.g. nickname.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Optional extra data for future use.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}