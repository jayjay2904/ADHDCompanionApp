using OpenAI;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace ADHDCompanionApp.Api.Services;

public class ArloAiService
{
    private readonly ArloAiOptions _options;
    

    public ArloAiService(IOptions<ArloAiOptions> options)
    {
        _options = options.Value;

    }

    private static readonly string[] AvoidPhrases =
    {
    "that makes sense",
    "come back tomorrow",
    "you've got this",
    "small step",
    "you are not alone",
    "just take it one step at a time",
    "i'm proud of you"
    };

    public async Task<string?> GetReplyAsync(string prompt)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        Debug.WriteLine($"OPENAI_API_KEY found: {!string.IsNullOrWhiteSpace(apiKey)}");

        if (string.IsNullOrWhiteSpace(apiKey))
            return null;

        try
        {
            var client = new OpenAIClient(apiKey);
            var chatClient = client.GetChatClient(_options.Model);

            var aiTask = chatClient.CompleteChatAsync(prompt);
            var timeoutTask = Task.Delay(_options.AiTimeoutMilliseconds);

            var completedTask = await Task.WhenAny(aiTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                Debug.WriteLine("OpenAI call timed out inside ArloAiService.");
                return null;
            }

            var response = await aiTask;

            var reply = response.Value.Content[0].Text?.Trim();

            if (string.IsNullOrWhiteSpace(reply))
            {
                Debug.WriteLine("OpenAI returned an empty reply.");
                return null;
            }

            foreach (var phrase in AvoidPhrases)
            {
                if (reply.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine($"Arlo avoid phrase detected: {phrase}");
                }
            }

            if (reply.Contains("As an AI", StringComparison.OrdinalIgnoreCase) ||
                reply.Contains("language model", StringComparison.OrdinalIgnoreCase) ||
                reply.Contains("I am not a therapist", StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine("OpenAI returned an off-brand reply. Falling back.");
                return null;
            }

            if (reply.Length > 700)
            {
                Debug.WriteLine("OpenAI reply was too long. Trimming response.");
                reply = TrimToLastSentence(reply, 700);
            }

            return reply;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OpenAI error in ArloAiService: {ex.Message}");
            return null;
        }
    }
    private static string TrimToLastSentence(string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength)
            return text;

        var trimmed = text[..maxLength].TrimEnd();

        var lastSentenceEnd = Math.Max(
            trimmed.LastIndexOf('.'),
            Math.Max(trimmed.LastIndexOf('!'), trimmed.LastIndexOf('?')));

        if (lastSentenceEnd > 80)
        {
            return trimmed[..(lastSentenceEnd + 1)].Trim();
        }

        return trimmed + "...";
    }

}