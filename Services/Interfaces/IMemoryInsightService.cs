using ADHDCompanionApp.Models.Entities;

namespace ADHDCompanionApp.Services.Interfaces;

public interface IMemoryInsightService
{
    Task AddInsightAsync(MemoryInsight insight);
    Task<List<MemoryInsight>> GetRecentInsightsAsync(int count = 10);
}