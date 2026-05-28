using System.Net.Http.Json;
using ADHDCompanionApp.Services.Interfaces;
using System.Diagnostics;

namespace ADHDCompanionApp.Services.Implementations;

public class BackendArloAiClient : IArloAiClient
{
    private readonly HttpClient _httpClient;
    private const int AiTimeoutMilliseconds = 5000;

    public BackendArloAiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetReplyAsync(ArloAiRequest request)
    {
        try
        {
            var requestTask = _httpClient.PostAsJsonAsync("api/arlo/chat", new
            {
                message = request.UserMessage,
                emotionalContext = request.EmotionalContext,
                openTasks = request.OpenTasks,
                recentModes = request.RecentModes,
                recentChat = request.RecentChat
            });

            var timeoutTask = Task.Delay(AiTimeoutMilliseconds);

            var completedTask = await Task.WhenAny(requestTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                Debug.WriteLine("Arlo AI timed out. Falling back to local support.");
                return null;
            }

            var response = await requestTask;

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Arlo AI backend returned: {response.StatusCode}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ArloAiResponse>();

            return result?.Reply;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Arlo AI client error: {ex.Message}");
            return null;
        }
    }

    private class ArloAiResponse
    {
        public string Reply { get; set; } = string.Empty;
    }
}