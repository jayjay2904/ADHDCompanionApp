using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services.Implementations;

public class WinService : IWinService
{
    private readonly DatabaseService _databaseService;

    public WinService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task AddWinAsync(WinEntry win)
    {
        if (win is null)
            return;

        if (string.IsNullOrWhiteSpace(win.Id))
        {
            win.Id = Guid.NewGuid().ToString();
        }

        if (win.TimestampUtc == default)
        {
            win.TimestampUtc = DateTime.UtcNow;
        }

        await _databaseService.SaveWinAsync(win);
    }

    public async Task<List<WinEntry>> GetRecentWinsAsync()
    {
        var wins = await _databaseService.GetWinsAsync();

        return wins
            .OrderByDescending(w => w.TimestampUtc)
            .ToList();
    }
}