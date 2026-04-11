namespace ADHDCompanionApp.Models.Entities;

public class CheckInEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public string Mood { get; set; } = string.Empty;

    public int EnergyLevel { get; set; }

    public int FocusLevel { get; set; }

    public string Note { get; set; } = string.Empty;
}