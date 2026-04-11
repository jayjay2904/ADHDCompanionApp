using ADHDCompanionApp.Models.Entities;

namespace ADHDCompanionApp.Services.Interfaces;

public interface IWinService
{
    Task AddWinAsync(WinEntry win);
    Task<List<WinEntry>> GetRecentWinsAsync();
}