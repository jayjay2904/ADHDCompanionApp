using SQLite;

namespace ADHDCompanionApp.Models.Entities;

public class CheckInEntry
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    // OLD (keep temporarily)
    public int EnergyLevel { get; set; }
    public int FocusLevel { get; set; }

    // NEW
    public string Mood { get; set; } = string.Empty;
    public int MoodScore { get; set; }
    public string MoodEmoji { get; set; } = string.Empty;

    public string? Note { get; set; }
}