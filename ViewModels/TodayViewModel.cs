using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.ViewModels;

public partial class TodayViewModel : BaseViewModel
{
    private readonly ICheckInService _checkInService;
    private readonly ITaskService _taskService;
    private readonly IWinService _winService;
    private readonly ITruthBombService _truthBombService;

    [ObservableProperty]
    private string mood = string.Empty;

    [ObservableProperty]
    private int energyLevel = 3;

    [ObservableProperty]
    private int focusLevel = 3;

    [ObservableProperty]
    private string note = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private string nextStepSuggestion = "No suggestion yet.";

    [ObservableProperty]
    private string newTaskTitle = string.Empty;

    [ObservableProperty]
    private string newWinText = string.Empty;

    [ObservableProperty]
    private string truthBombText = string.Empty;

    public ObservableCollection<WinEntry> RecentWins { get; } = new();

    public ObservableCollection<TaskItem> Tasks { get; } = new();

    public TodayViewModel(ICheckInService checkInService, ITaskService taskService, IWinService winService, ITruthBombService truthBombService)
    {
        _checkInService = checkInService;
        _taskService = taskService;
        _winService = winService;
        _truthBombService = truthBombService;

        Title = "Today";
    }

    [RelayCommand]
    private async Task SaveCheckIn()
    {
        var entry = new CheckInEntry
        {
            Mood = Mood,
            EnergyLevel = EnergyLevel,
            FocusLevel = FocusLevel,
            Note = Note
        };

        await _checkInService.SaveCheckInAsync(entry);

        GenerateNextStepSuggestion();

        StatusMessage = "Check-in saved.";
    }

    [RelayCommand]
    private async Task AddTask()
    {
        if (string.IsNullOrWhiteSpace(NewTaskTitle))
        {
            return;
        }

        var task = new TaskItem
        {
            Title = NewTaskTitle.Trim()
        };

        await _taskService.AddTaskAsync(task);

        NewTaskTitle = string.Empty;

        await LoadTasksAsync();
    }


    [RelayCommand]
    private async Task AddWin()
    {
        if (string.IsNullOrWhiteSpace(NewWinText))
        {
            return;
        }

        var win = new WinEntry
        {
            Text = NewWinText.Trim()
        };

        await _winService.AddWinAsync(win);

        NewWinText = string.Empty;
        StatusMessage = "Win saved.";

        await LoadWinsAsync();
    }

    public async Task LoadWinsAsync()
    {
        var wins = await _winService.GetRecentWinsAsync();

        RecentWins.Clear();

        foreach (var win in wins.Take(5))
        {
            RecentWins.Add(win);
        }
    }
    public async Task LoadTruthBombAsync()
    {
        var bomb = await _truthBombService.GetTruthBombAsync();
        TruthBombText = bomb.Text;
    }

    [RelayCommand]
    private async Task CompleteTask(TaskItem task)
    {
        if (task is null)
            return;

        task.IsCompleted = true;

        await _taskService.CompleteTaskAsync(task.Id);

        StatusMessage = "Nice — task completed.";

        await LoadTasksAsync();
    }

    public async Task LoadTasksAsync()
    {
        var tasks = await _taskService.GetAllTasksAsync();

        Tasks.Clear();

        foreach (var task in tasks.OrderBy(t => t.IsCompleted).ThenBy(t => t.CreatedUtc))
        {
            Tasks.Add(task);
        }
    }

    private void GenerateNextStepSuggestion()
    {
        if (EnergyLevel <= 2 && FocusLevel <= 2)
        {
            NextStepSuggestion = "Keep it simple. Pick one tiny task.";
        }
        else if (EnergyLevel <= 2)
        {
            NextStepSuggestion = "Low energy. Focus on something easy or take a short reset.";
        }
        else if (FocusLevel <= 2)
        {
            NextStepSuggestion = "You're struggling to focus. Try a 5-minute timer to get started.";
        }
        else if (EnergyLevel >= 4 && FocusLevel >= 4)
        {
            NextStepSuggestion = "Good window. Tackle something meaningful.";
        }
        else
        {
            NextStepSuggestion = "Make a small step forward. Progress over perfection.";
        }
    }
}