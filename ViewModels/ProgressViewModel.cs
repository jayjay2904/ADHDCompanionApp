using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.ViewModels;

public partial class ProgressViewModel : BaseViewModel
{
    private readonly ICheckInService _checkInService;
    private readonly ITaskService _taskService;
    private readonly IWinService _winService;

    [ObservableProperty]
    private int totalCheckIns;

    [ObservableProperty]
    private int totalTasks;

    [ObservableProperty]
    private int completedTasks;

    [ObservableProperty]
    private int totalWins;

    public ObservableCollection<WinEntry> RecentWins { get; } = new();

    public ProgressViewModel(
        ICheckInService checkInService,
        ITaskService taskService,
        IWinService winService)
    {
        _checkInService = checkInService;
        _taskService = taskService;
        _winService = winService;

        Title = "Progress";
    }

    public async Task LoadProgressAsync()
    {
        var checkIns = await _checkInService.GetAllCheckInsAsync();
        var tasks = await _taskService.GetAllTasksAsync();
        var wins = await _winService.GetRecentWinsAsync();

        TotalCheckIns = checkIns.Count;
        TotalTasks = tasks.Count;
        CompletedTasks = tasks.Count(t => t.IsCompleted);
        TotalWins = wins.Count;

        RecentWins.Clear();

        foreach (var win in wins.Take(5))
        {
            RecentWins.Add(win);
        }
    }
}