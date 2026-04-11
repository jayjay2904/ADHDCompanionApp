namespace ADHDCompanionApp.Models.Entities;

public class SupportOption
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Title { get; set; } = string.Empty;

    public string ResponseText { get; set; } = string.Empty;
}