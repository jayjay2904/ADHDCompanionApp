using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services.Implementations;

public class MockArloAiClient : IArloAiClient
{
    public async Task<string?> GetReplyAsync(ArloAiRequest request)
    {
        await Task.Delay(700);

        var context = string.IsNullOrWhiteSpace(request.EmotionalContext)
            ? string.Empty
            : $"{request.EmotionalContext} ";

        return $"{context}I hear you. Let’s not try to solve everything at once. What feels like the smallest next step from here?";
    }
}