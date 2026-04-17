using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services.Implementations;

public class CheckInService : ICheckInService
{
    private readonly DatabaseService _databaseService;

    public CheckInService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task SaveCheckInAsync(CheckInEntry entry)
    {
        if (entry is null)
            return;

        if (string.IsNullOrWhiteSpace(entry.Id))
        {
            entry.Id = Guid.NewGuid().ToString();
        }

        if (entry.TimestampUtc == default)
        {
            entry.TimestampUtc = DateTime.UtcNow;
        }

        await _databaseService.SaveCheckInAsync(entry);
    }

    public async Task<CheckInEntry?> GetLatestCheckInAsync()
    {
        var checkIns = await _databaseService.GetCheckInsAsync();

        return checkIns
            .OrderByDescending(c => c.TimestampUtc)
            .FirstOrDefault();
    }

    public async Task<List<CheckInEntry>> GetAllCheckInsAsync()
    {
        return await _databaseService.GetCheckInsAsync();
    }
}