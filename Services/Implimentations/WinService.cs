using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services.Implementations;

public class WinService : IWinService
{
    private readonly List<WinEntry> _wins = new();

    public Task AddWinAsync(WinEntry win)
    {
        _wins.Add(win);
        return Task.CompletedTask;
    }

    public Task<List<WinEntry>> GetRecentWinsAsync()
    {
        var wins = _wins
            .OrderByDescending(w => w.TimestampUtc)
            .ToList();

        return Task.FromResult(wins);
    }
}