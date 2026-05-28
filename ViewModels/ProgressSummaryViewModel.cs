using System.Collections.ObjectModel;
using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.ViewModels;

public class ProgressSummaryViewModel
{
    private readonly IWinService _winService;
    private readonly ICheckInService _checkInService;
    private readonly ITaskService _taskService;

    public ObservableCollection<WinEntry> RecentWins { get; } = new();
    public ObservableCollection<CheckInEntry> RecentCheckIns { get; } = new();
    public ObservableCollection<TaskItem> OpenTasks { get; } = new();

    public string WinsSummary { get; private set; } = "No wins captured yet.";
    public string CheckInSummary { get; private set; } = "No check-ins captured yet.";
    public string TasksSummary { get; private set; } = "No open small steps.";

    public ProgressSummaryViewModel(
        IWinService winService,
        ICheckInService checkInService,
        ITaskService taskService)
    {
        _winService = winService;
        _checkInService = checkInService;
        _taskService = taskService;
    }

    public async Task LoadAsync()
{
    RecentWins.Clear();
    RecentCheckIns.Clear();
    OpenTasks.Clear();

    var recentWins = (await _winService.GetRecentWinsAsync())
        .Take(5)
        .ToList();

        List<CheckInEntry> recentCheckIns = new();

        try
        {
            recentCheckIns = (await _checkInService.GetAllCheckInsAsync())
                .OrderByDescending(c => c.TimestampUtc)
                .Take(5)
                .ToList();
        }
        catch
        {
            CheckInSummary = "Check-ins couldn’t be loaded yet.";
        }
        var openTasks = (await _taskService.GetAllTasksAsync())
        .Where(t => !t.IsCompleted)
        .OrderByDescending(t => t.CreatedUtc)
        .Take(5)
        .ToList();

    foreach (var win in recentWins)
        RecentWins.Add(win);

    foreach (var checkIn in recentCheckIns)
        RecentCheckIns.Add(checkIn);

    foreach (var task in openTasks)
        OpenTasks.Add(task);

    WinsSummary = recentWins.Count == 0
        ? "No wins captured yet."
        : $"{recentWins.Count} recent win{(recentWins.Count == 1 ? "" : "s")} captured.";

    CheckInSummary = recentCheckIns.Count == 0
        ? "No check-ins captured yet."
        : $"{recentCheckIns.Count} recent check-in{(recentCheckIns.Count == 1 ? "" : "s")} captured.";

    TasksSummary = openTasks.Count == 0
        ? "No open small steps."
        : $"{openTasks.Count} open small step{(openTasks.Count == 1 ? "" : "s")}.";
}
}