using ADHDCompanionApp.Models.Entities;

namespace ADHDCompanionApp.Services.Interfaces;

public interface IArloService
{
    Task<string> GetReplyAsync(string userMessage);
    Task<List<ChatMessage>> GetMessagesAsync();
    Task AddMessageAsync(ChatMessage message);
    Task ClearMessagesAsync();
}