namespace ADHDCompanionApp.Services.Interfaces;

public interface IArloAiClient
{
    Task<string?> GetReplyAsync(ArloAiRequest request);
}

public class ArloAiRequest
{
    public string UserMessage { get; set; } = string.Empty;
    public string EmotionalContext { get; set; } = string.Empty;
    public List<string> OpenTasks { get; set; } = new();
    public List<string> RecentModes { get; set; } = new();
    public List<string> RecentChat { get; set; } = new();
    public bool ReminderIntentDetected { get; set; }
}