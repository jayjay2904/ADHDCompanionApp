using CommunityToolkit.Mvvm.ComponentModel;

namespace ADHDCompanionApp.Models.Entities;

public class ChatMessage: ObservableObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Role { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public bool IsFromUser => Role == "User";
    public bool IsFromArlo => Role == "Arlo";

}