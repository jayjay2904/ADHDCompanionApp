using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;


namespace ADHDCompanionApp.ViewModels;

public partial class TodayViewModel : BaseViewModel
{
    private readonly ICheckInService _checkInService;
    private readonly ITaskService _taskService;
    private readonly IWinService _winService;
    private readonly ITruthBombService _truthBombService;
    private readonly IUserProfileService _profileService;

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

    [ObservableProperty]
    private string checkInPrompt = string.Empty;

    [ObservableProperty]
    private string flowMessage = string.Empty;

    [ObservableProperty]
    private string greetingText = "Welcome";

    public ObservableCollection<WinEntry> RecentWins { get; } = new();

    public ObservableCollection<TaskItem> Tasks { get; } = new();

    public TodayViewModel(ICheckInService checkInService, ITaskService taskService, IWinService winService, ITruthBombService truthBombService, IUserProfileService profileService)
    {
        _checkInService = checkInService;
        _taskService = taskService;
        _winService = winService;
        _truthBombService = truthBombService;
        _profileService = profileService;
        Title = "Today";
        
    }
    public async Task LoadDataAsync()
    {
        await LoadTasksAsync();
        await LoadWinsAsync();
        await LoadTruthBombAsync();
        await LoadLatestCheckInAsync();
        await LoadGreetingAsync();
        UpdateFlowMessage();
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

        StatusMessage = "Task added.";
        await Task.Delay(2000);
        StatusMessage = string.Empty;

        await LoadTasksAsync();
        UpdateFlowMessage();
    }

    [RelayCommand]
    private async Task ToggleTask(TaskItem task)
    {
        if (task is null)
            return;

        task.IsCompleted = !task.IsCompleted;
        task.CompletedUtc = task.IsCompleted ? DateTime.UtcNow : null;

        await _taskService.UpdateTaskAsync(task);

        StatusMessage = task.IsCompleted
            ? "Nice — task completed."
            : "Task marked as active again.";

        await LoadTasksAsync();
        UpdateFlowMessage();
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
        await Task.Delay(2000);
        StatusMessage = string.Empty;

        await LoadWinsAsync();
        UpdateFlowMessage();
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
        UpdateFlowMessage();
    }
    public async Task LoadTasksAsync()
    {
        var tasks = await _taskService.GetAllTasksAsync();

        System.Diagnostics.Debug.WriteLine("---- TASKS LOADED ----");

        foreach (var t in tasks)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Id={t.Id}, Title={t.Title}, IsCompleted={t.IsCompleted}, CreatedUtc={t.CreatedUtc}, CompletedUtc={t.CompletedUtc}");
        }

        Tasks.Clear();

        foreach (var task in tasks.OrderBy(t => t.IsCompleted).ThenBy(t => t.CreatedUtc))
        {
            Tasks.Add(task);
        }
    }
    private async Task LoadGreetingAsync()
    {
        var profile = await _profileService.GetProfileAsync();

        if (profile is not null && !string.IsNullOrWhiteSpace(profile.Nickname))
        {
            GreetingText = $"Welcome {profile.Nickname}";
        }
        else
        {
            GreetingText = "Welcome";
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
    public async Task LoadLatestCheckInAsync()
    {
        var latestCheckIn = await _checkInService.GetLatestCheckInAsync();

        if (latestCheckIn is null)
            return;

        Mood = latestCheckIn.Mood;
        EnergyLevel = latestCheckIn.EnergyLevel;
        FocusLevel = latestCheckIn.FocusLevel;
        Note = latestCheckIn.Note;

        GenerateNextStepSuggestion();

        // 👇 NEW BIT
        var timeSince = DateTime.UtcNow - latestCheckIn.TimestampUtc;

        if (timeSince.TotalHours < 12)
        {
            CheckInPrompt = $"Earlier you felt {latestCheckIn.Mood}. How are you feeling now?";
        }
        else
        {
            CheckInPrompt = $"Last time you checked in, you felt {latestCheckIn.Mood}. How are things today?";
        }
    }
    private void UpdateFlowMessage()
    {
        if (string.IsNullOrWhiteSpace(Mood))
        {
            FlowMessage = "Start with a quick check-in. How are you feeling?";
        }
        else if (Tasks.Any(t => !t.IsCompleted))
        {
            FlowMessage = "Pick one small task and make a start.";
        }
        else if (Tasks.Any())
        {
            FlowMessage = "Nice work. Log a win before you finish.";
        }
        else
        {
            FlowMessage = "Add a task to get started.";
        }
    }
}