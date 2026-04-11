namespace ADHDCompanionApp.Models.Entities;

public class TruthBomb
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Text { get; set; } = string.Empty;
}