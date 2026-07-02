namespace ADHDCompanionApp.Models;

public class ConversationMemoryItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public string Category { get; set; } = "general";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}