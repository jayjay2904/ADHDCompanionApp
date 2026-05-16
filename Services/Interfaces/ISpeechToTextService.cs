namespace ADHDCompanionApp.Services.Interfaces;

public interface ISpeechToTextService
{
    Task<string?> ListenAsync();
}