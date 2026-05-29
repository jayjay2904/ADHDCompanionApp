namespace ADHDCompanionApp.Api.Services;

public class ArloResponseModeDetector
{
    public string Detect(string message)
    {
        var text = message
            .ToLowerInvariant()
            .Replace("'", "")
            .Replace("’", "");

        if (ContainsAny(text, "panic", "panicking", "anxious", "anxiety", "racing thoughts"))
            return "grounding";

        if (ContainsAny(text, "cant start", "cannot start", "stuck", "frozen", "dont know where to start"))
            return "task_paralysis";

        if (ContainsAny(text, "overwhelmed", "too much", "too many things", "everything feels impossible"))
            return "overwhelm";

        if (ContainsAny(text, "exhausted", "drained", "no energy", "tired", "burnt out", "burned out"))
            return "low_energy";

        if (ContainsAny(text, "failed", "useless", "not good enough", "messed up", "hate myself"))
            return "shame_spiral";

        if (ContainsAny(text, "did it", "done", "proud", "managed", "finished"))
            return "celebration";

        return "general";
    }
    private static bool ContainsAny(string text, params string[] phrases)
    {
        return phrases.Any(text.Contains);
    }
}