using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services.Implementations;

public class CheckInService : ICheckInService
{
    private readonly List<CheckInEntry> _checkIns = new();

    public Task SaveCheckInAsync(CheckInEntry entry)
    {
        _checkIns.Add(entry);
        return Task.CompletedTask;
    }

    public Task<CheckInEntry?> GetLatestCheckInAsync()
    {
        var latest = _checkIns
            .OrderByDescending(c => c.TimestampUtc)
            .FirstOrDefault();

        return Task.FromResult(latest);
    }

    public Task<List<CheckInEntry>> GetAllCheckInsAsync()
    {
        return Task.FromResult(_checkIns.ToList());
    }
}