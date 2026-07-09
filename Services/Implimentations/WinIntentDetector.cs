using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services;

public class WinIntentDetector
{
    private static readonly string[] WinPatterns =
{
    "i did",
    "i finally",
    "i managed",
    "i managed to",
    "i got through",
    "i completed",
    "i finished",
    "i made it",
    "i remembered",
    "i remembered to",
    "i took my",
    "i went to",
    "i got out of bed",
    "i replied",
    "i replied to",
    "i sent",
    "i also sent",
    "i have sent",
    "i’ve sent",
    "i cleaned",
    "i sorted",
    "i got done",
    "i got round to",
    "i needed to"
};

    public bool IsWinIntent(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        var normalised = message.Trim().ToLowerInvariant();

        return WinPatterns.Any(pattern => normalised.Contains(pattern));
    }
}