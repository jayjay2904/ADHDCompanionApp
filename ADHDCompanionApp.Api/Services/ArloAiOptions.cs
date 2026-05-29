namespace ADHDCompanionApp.Api.Services;

public class ArloAiOptions
{
    public string Model { get; set; } = "gpt-4o-mini";

    public int AiTimeoutMilliseconds { get; set; } = 5000;
}