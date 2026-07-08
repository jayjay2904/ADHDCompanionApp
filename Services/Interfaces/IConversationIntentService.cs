namespace ADHDCompanionApp.Services.Interfaces;

public interface IConversationIntentService
{
    bool LooksLikeReminderIntent(string input);
    bool LooksLikeRecallQuestion(string input);
    string CleanReminderText(string input);
    string CleanMemoryText(string input);
}