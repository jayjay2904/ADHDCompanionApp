using System.Net.Http.Json;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services.Implementations;

public class BackendArloAiClient : IArloAiClient
{
    private readonly HttpClient _httpClient;

    public BackendArloAiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetReplyAsync(ArloAiRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/arlo/chat", new
            {
                message = request.UserMessage,
                emotionalContext = request.EmotionalContext,
                openTasks = request.OpenTasks
            });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ArloAiResponse>();

            return result?.Reply;
        }
        catch
        {
            return null;
        }
    }

    private class ArloAiResponse
    {
        public string Reply { get; set; } = string.Empty;
    }
}