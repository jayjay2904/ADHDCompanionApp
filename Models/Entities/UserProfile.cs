namespace ADHDCompanionApp.Models.Entities;

public class UserProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Nickname { get; set; } = string.Empty;
    public string PreferredTone { get; set; } = string.Empty;
    public string ReminderStyle { get; set; } = string.Empty;
    public bool UsesMedicationSupport { get; set; }
    public TimeSpan? MedicationReminderTime { get; set; }
    public bool UsesTaskSupport { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}