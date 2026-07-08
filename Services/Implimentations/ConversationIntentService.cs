using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services.Implementations;

public class ConversationIntentService : IConversationIntentService
{
    public bool LooksLikeReminderIntent(string input)
    {
        var text = Normalise(input);

        return text.Contains("remind me to")
            || text.Contains("can you remind me")
            || text.Contains("please remind me")
            || text.Contains("dont let me forget")
            || text.Contains("remember to")
            || text.Contains("book an appointment")
            || text.Contains("make an appointment")
            || text.Contains("arrange an appointment")
            || text.Contains("schedule an appointment")
            || text.Contains("i have an appointment")
            || text.Contains("ive got an appointment")
            || text.Contains("appointment with")
            || text.Contains("appointment at")
            || text.Contains("appointment on")
            || text.Contains("i need to pay")
            || text.Contains("i need to call")
            || text.Contains("i need to phone")
            || text.Contains("i need to ring")
            || text.Contains("i need to collect")
            || text.Contains("i need to pick up")
            || text.Contains("i have to pay")
            || text.Contains("i have to call")
            || text.Contains("i have to phone")
            || text.Contains("i must pay")
            || text.Contains("i must call");
    }

    public bool LooksLikeRecallQuestion(string input)
    {
        var text = Normalise(input);

        var asksForRecall =
            text.Contains("remind me") ||
            text.Contains("what") ||
            text.Contains("remember");

        var refersToSelf =
            text.Contains(" i ") ||
            text.StartsWith("i ") ||
            text.Contains(" me ") ||
            text.Contains(" my ");

        var refersToTasksOrMemory =
            text.Contains("need to do") ||
            text.Contains("have to do") ||
            text.Contains("got to do") ||
            text.Contains("supposed to do") ||
            text.Contains("meant to do") ||
            text.Contains("said") ||
            text.Contains("mention") ||
            text.Contains("got on") ||
            text.Contains("have on") ||
            text.Contains("remember");

        return asksForRecall && refersToSelf && refersToTasksOrMemory;
    }

    public string CleanReminderText(string input)
    {
        return CleanCommon(input);
    }

    public string CleanMemoryText(string input)
    {
        var cleaned = CleanCommon(input)
            .Replace("Hey Arlo", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Arlo", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Hello", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Hi", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Please", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        return Capitalise(cleaned, input);
    }

    private static string CleanCommon(string input)
    {
        var cleaned = input.Trim();

        cleaned = cleaned
            .Replace("I also need to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("I need to also", "", StringComparison.OrdinalIgnoreCase)
            .Replace("I still need to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("I also have to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("I have to also", "", StringComparison.OrdinalIgnoreCase)
            .Replace("I still have to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("I must also", "", StringComparison.OrdinalIgnoreCase)
            .Replace("I've got to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Ive got to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("I've still got to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Ive still got to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("I need to remember to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("I have to remember to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Can you remind me to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Please remind me to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Don't let me forget to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Dont let me forget to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Remind me to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Remember to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("I need to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("I have to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("I should", "", StringComparison.OrdinalIgnoreCase)
            .Replace("I must", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        return cleaned;
    }

    private static string Normalise(string input)
    {
        return input
            .ToLowerInvariant()
            .Replace("'", "")
            .Replace("’", "")
            .Trim();
    }

    private static string Capitalise(string cleaned, string fallback)
    {
        if (string.IsNullOrWhiteSpace(cleaned))
            return fallback.Trim();

        return char.ToUpper(cleaned[0]) + cleaned[1..];
    }
}