using SQLite;
namespace ADHDCompanionApp.Models.Entities;

public class WinEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Text { get; set; } = string.Empty;

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}