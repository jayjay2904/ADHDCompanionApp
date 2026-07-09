using System.Collections.ObjectModel;
using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.ViewModels;

public class ProgressSummaryViewModel
{
    private readonly IWinService _winService;
    private readonly ICheckInService _checkInService;

    public ObservableCollection<WinEntry> RecentWins { get; } = new();
    public ObservableCollection<CheckInEntry> RecentCheckIns { get; } = new();

    public ProgressSummaryViewModel(
        IWinService winService,
        ICheckInService checkInService)
    {
        _winService = winService;
        _checkInService = checkInService;
    }

    public async Task LoadAsync()
    {
        RecentWins.Clear();
        RecentCheckIns.Clear();

        var recentWins = (await _winService.GetRecentWinsAsync())
            .Take(5)
            .ToList();

        var recentCheckIns = (await _checkInService.GetAllCheckInsAsync())
            .OrderByDescending(c => c.TimestampUtc)
            .Take(5)
            .ToList();

        foreach (var win in recentWins)
            RecentWins.Add(win);

        foreach (var checkIn in recentCheckIns)
            RecentCheckIns.Add(checkIn);
    }
}