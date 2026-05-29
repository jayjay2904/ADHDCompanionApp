namespace ADHDCompanionApp.Api.Services;

public class ArloSafetyService
{
    public bool ContainsCrisisLanguage(string message)
    {
        var text = message
            .ToLowerInvariant()
            .Replace("'", "")
            .Replace("’", "");

        return text.Contains("kill myself")
            || text.Contains("suicide")
            || text.Contains("end it all")
            || text.Contains("self harm")
            || text.Contains("hurt myself")
            || text.Contains("not safe")
            || text.Contains("dont want to be here")
            || text.Contains("want to disappear")
            || text.Contains("cant do this anymore")
            || text.Contains("i give up");
    }

    public string GetCrisisReply()
    {
        return "I’m really glad you said that out loud. You don’t need to carry this on your own right now. Please contact someone you trust, a mental health service, or emergency services if you feel unsafe. Small next step: send one message to a real person saying you’re struggling.";
    }
}