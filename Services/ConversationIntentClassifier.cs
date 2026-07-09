using ADHDCompanionApp.Models;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services;

public class ConversationIntentClassifier : IConversationIntentClassifier
{
    private readonly IConversationIntentService _intentService;
    private readonly WinIntentDetector _winIntentDetector;

    public ConversationIntentClassifier(
        IConversationIntentService intentService,
        WinIntentDetector winIntentDetector)
    {
        _intentService = intentService;
        _winIntentDetector = winIntentDetector;
    }

    public ConversationIntent Classify(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return ConversationIntent.General;

        //if (_intentService.LooksLikeCrisisIntent(message))
            //return ConversationIntent.Crisis;

        if (_intentService.LooksLikeRecallQuestion(message))
            return ConversationIntent.ReminderRecall;

        if (_intentService.LooksLikeReminderIntent(message))
            return ConversationIntent.ReminderCreate;

        if (_winIntentDetector.IsWinIntent(message))
            return ConversationIntent.Win;

        return ConversationIntent.General;
    }
}