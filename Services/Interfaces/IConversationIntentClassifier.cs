using ADHDCompanionApp.Models;

namespace ADHDCompanionApp.Services.Interfaces;

public interface IConversationIntentClassifier
{
    ConversationIntent Classify(string message);
}