namespace ADHDCompanionApp.Models.Entities;

public class SupportOption
{
    public string Title { get; set; } = string.Empty;

    public string ValidationText { get; set; } = string.Empty;

    public string ImmediateActionText { get; set; } = string.Empty;

    public string NextStepText { get; set; } = string.Empty;

    public string AlternateImmediateActionText { get; set; } = string.Empty;

    public string AlternateNextStepText { get; set; } = string.Empty;
}