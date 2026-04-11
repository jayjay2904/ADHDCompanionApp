using ADHDCompanionApp.Models.Entities;

namespace ADHDCompanionApp.Services.Interfaces;

public interface ICheckInService
{
    Task SaveCheckInAsync(CheckInEntry entry);
    Task<CheckInEntry?> GetLatestCheckInAsync();
    Task<List<CheckInEntry>> GetAllCheckInsAsync();
}