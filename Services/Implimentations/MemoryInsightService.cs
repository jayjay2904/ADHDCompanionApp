using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services.Implementations;

public class MemoryInsightService : IMemoryInsightService
{
    private readonly DatabaseService _databaseService;

    public MemoryInsightService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task AddInsightAsync(MemoryInsight insight)
    {
        if (insight is null)
            return;

        if (string.IsNullOrWhiteSpace(insight.Id))
        {
            insight.Id = Guid.NewGuid().ToString();
        }

        if (insight.CreatedUtc == default)
        {
            insight.CreatedUtc = DateTime.UtcNow;
        }

        await _databaseService.SaveMemoryInsightAsync(insight);
    }

    public async Task<List<MemoryInsight>> GetRecentInsightsAsync(int count = 10)
    {
        var insights = await _databaseService.GetMemoryInsightsAsync();

        return insights
            .OrderByDescending(i => i.CreatedUtc)
            .Take(count)
            .ToList();
    }
}