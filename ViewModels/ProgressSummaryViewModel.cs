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

    public string WinsSummary { get; private set; } = "";
    public string CheckInSummary { get; private set; } = "";
    public string TasksSummary { get; private set; } = "";

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
        var cutoffUtc = DateTime.UtcNow.AddHours(-48);
        RecentWins.Clear();
        RecentCheckIns.Clear();
        OpenTasks.Clear();

        var recentWins = (await _winService.GetRecentWinsAsync())
        .Where(w => w.TimestampUtc >= cutoffUtc)
        .Take(5)
        .ToList();

        var recentCheckIns = (await _checkInService.GetAllCheckInsAsync())
        .Where(w => w.TimestampUtc >= cutoffUtc)
        .OrderByDescending(c => c.TimestampUtc)
        .Take(5)
        .ToList();
        
        //var openTasks = (await _taskService.GetAllTasksAsync())
        //.Where(t => !t.IsCompleted)
        //.OrderByDescending(t => t.CreatedUtc)
        //.Take(5)
        //.ToList();

    foreach (var win in recentWins)
        RecentWins.Add(win);

    foreach (var checkIn in recentCheckIns)
        RecentCheckIns.Add(checkIn);

    //foreach (var task in openTasks)
    //    OpenTasks.Add(task);

        WinsSummary = recentWins.Count == 0
        ? "No wins captured yet."
        : string.Empty;

        CheckInSummary = recentCheckIns.Count == 0
            ? "No check-ins captured yet."
            : string.Empty;

        //TasksSummary = openTasks.Count == 0
        //    ? "No open reminders yet."
        //    : string.Empty;
    }
}