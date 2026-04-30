using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services;
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
    private readonly IReminderEngine _reminderEngine;
    public event Func<Task>? CelebrationRequested;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private string newTaskTitle = string.Empty;

    [ObservableProperty]
    private string newWinText = string.Empty;

    [ObservableProperty]
    private string truthBombText = string.Empty;

    [ObservableProperty]
    private string flowMessage = string.Empty;

    [ObservableProperty]
    private string greetingText = "Welcome";

    [ObservableProperty]
    private string selectedMood = string.Empty;

    [ObservableProperty]
    private int selectedMoodScore;

    [ObservableProperty]
    private string selectedMoodEmoji = string.Empty;

    [ObservableProperty]
    private string selectedMoodDisplayText = string.Empty;

    [ObservableProperty]
    private string nextStepText = string.Empty;

    [ObservableProperty]
    private bool hasSavedCheckIn;

    [ObservableProperty]
    private string checkInNote = string.Empty;

    [ObservableProperty]
    private bool isGreatSelected;

    [ObservableProperty]
    private bool isGoodSelected;

    [ObservableProperty]
    private bool isOkaySelected;

    [ObservableProperty]
    private bool isLowSelected;

    [ObservableProperty]
    private bool isStrugglingSelected;

    public ObservableCollection<WinEntry> RecentWins { get; } = new();
    public ObservableCollection<TaskItem> Tasks { get; } = new();

    public TodayViewModel(
        ICheckInService checkInService,
        ITaskService taskService,
        IWinService winService,
        ITruthBombService truthBombService,
        IUserProfileService profileService,
        IReminderEngine reminderEngine)
    {
        _checkInService = checkInService;
        _taskService = taskService;
        _winService = winService;
        _truthBombService = truthBombService;
        _profileService = profileService;
        _reminderEngine = reminderEngine;

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
    private async Task SelectMood(string value)
    {
        try
        {
            var parts = value.Split('|');

            if (parts.Length != 3)
                return;

            SelectedMood = parts[0];
            SelectedMoodScore = int.Parse(parts[1]);
            SelectedMoodEmoji = parts[2];

            UpdateMoodSelectionState();

            SelectedMoodDisplayText = $"You said you're feeling: {SelectedMoodEmoji} {SelectedMood}";
            NextStepText = GetNextStep(SelectedMoodScore);
            HasSavedCheckIn = true;

            var checkIn = new CheckInEntry
            {
                Mood = SelectedMood,
                MoodScore = SelectedMoodScore,
                MoodEmoji = SelectedMoodEmoji,

                // Temporary mapping for older app code/database fields
                EnergyLevel = SelectedMoodScore,
                FocusLevel = SelectedMoodScore,

                TimestampUtc = DateTime.UtcNow
            };

            await _checkInService.SaveCheckInAsync(checkIn);

            UpdateFlowMessage();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TodayViewModel] SelectMood failed: {ex}");
            StatusMessage = "Could not save check-in.";
        }
    }

    [RelayCommand]
    private async Task SaveNote()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(CheckInNote))
            {
                StatusMessage = "Nothing to save yet.";
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedMood))
            {
                StatusMessage = "Pick how you're feeling first.";
                return;
            }

            var checkIn = new CheckInEntry
            {
                Mood = SelectedMood,
                MoodScore = SelectedMoodScore,
                MoodEmoji = SelectedMoodEmoji,

                // Temporary mapping for older app code/database fields
                EnergyLevel = SelectedMoodScore,
                FocusLevel = SelectedMoodScore,

                Note = CheckInNote.Trim(),
                TimestampUtc = DateTime.UtcNow
            };

            await _checkInService.SaveCheckInAsync(checkIn);

            StatusMessage = "Note saved.";
            await Task.Delay(2000);
            StatusMessage = string.Empty;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TodayViewModel] SaveNote failed: {ex}");
            StatusMessage = "Could not save note.";
        }
    }

    [RelayCommand]
    private async Task AddTask()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle))
                return;

            var task = new TaskItem
            {
                Title = NewTaskTitle.Trim(),
                ReminderEnabled = false,
                ReminderDateTime = null,
                IsEditingReminder = false
            };

            await _taskService.AddTaskAsync(task);
            await LoadTasksAsync();
            NewTaskTitle = string.Empty;

            UpdateFlowMessage();
            StatusMessage = "Task added.";
            await Task.Delay(1500);
            StatusMessage = string.Empty;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TodayViewModel] AddTask failed: {ex}");
            StatusMessage = "Could not add task.";
        }
    }

    [RelayCommand]
    private async Task ToggleTask(TaskItem task)
    {
        try
        {
            if (task is null)
                return;

            if (task.IsCompleted)
                return;

            task.IsCompleted = true;
            task.CompletedUtc = DateTime.UtcNow;
            task.IsEditingReminder = false;

            await _reminderEngine.CancelTaskReminderAsync(task);
            await _taskService.UpdateTaskAsync(task);

            var messages = new[]
            {
            $"Completed: {task.Title}",
            $"Ticked off: {task.Title}",
            $"Made progress on: {task.Title}",
            $"Done: {task.Title}"
        };

            var win = new WinEntry
            {
                Text = messages[Random.Shared.Next(messages.Length)]
            };

            await _winService.AddWinAsync(win);

            try
            {
                HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TodayViewModel] Haptic feedback failed: {ex}");
            }

            StatusMessage = "Nice — task completed.";

            await LoadTasksAsync();
            await LoadWinsAsync();
            UpdateFlowMessage();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TodayViewModel] ToggleTask failed: {ex}");
            StatusMessage = "Could not update task.";
        }
    }

    [RelayCommand]
    private async Task ToggleTaskReminder(TaskItem task)
    {
        try
        {
            if (task is null)
                return;

            if (task.IsCompleted)
                return;

            task.ReminderEnabled = !task.ReminderEnabled;

            if (task.ReminderEnabled)
            {
                var defaultDateTime = task.ReminderDateTime ?? DateTime.Now.AddMinutes(10);

                task.ReminderDate = defaultDateTime.Date;
                task.ReminderTime = defaultDateTime.TimeOfDay;
                task.IsEditingReminder = true;
            }
            else
            {
                task.ReminderDateTime = null;
                task.IsEditingReminder = false;

                await _reminderEngine.CancelTaskReminderAsync(task);
                await _taskService.UpdateTaskAsync(task);

                StatusMessage = "Reminder removed.";
                await LoadTasksAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TodayViewModel] ToggleTaskReminder failed: {ex}");
            StatusMessage = "Could not update reminder.";
        }
    }

    public async Task SetTaskReminderEnabledAsync(TaskItem task, bool isEnabled)
    {
        try
        {
            if (task is null)
                return;

            if (task.IsCompleted)
                return;

            task.ReminderEnabled = isEnabled;

            if (isEnabled)
            {
                var defaultDateTime = task.ReminderDateTime ?? DateTime.Now.AddMinutes(10);

                task.ReminderDate = defaultDateTime.Date;
                task.ReminderTime = defaultDateTime.TimeOfDay;
                task.IsEditingReminder = true;

                await _taskService.UpdateTaskAsync(task);

                StatusMessage = "Choose when you want the reminder.";
            }
            else
            {
                task.ReminderDateTime = null;
                task.IsEditingReminder = false;

                await _reminderEngine.CancelTaskReminderAsync(task);
                await _taskService.UpdateTaskAsync(task);

                StatusMessage = "Reminder removed.";
            }

            await LoadTasksAsync();
            UpdateFlowMessage();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TodayViewModel] SetTaskReminderEnabledAsync failed: {ex}");
            StatusMessage = "Could not update reminder.";
        }
    }

    [RelayCommand]
    private async Task SaveTaskReminder(TaskItem task)
    {
        try
        {
            if (task is null)
                return;

            if (task.IsCompleted)
                return;

            if (!task.ReminderEnabled)
                return;

            var reminderDateTime = task.ReminderDate.Date.Add(task.ReminderTime);

            if (reminderDateTime <= DateTime.Now)
            {
                StatusMessage = "Pick a future date and time for the reminder.";
                return;
            }

#if ANDROID
            var notificationPermission = await Permissions.RequestAsync<Permissions.PostNotifications>();

            if (notificationPermission != PermissionStatus.Granted)
            {
                await Shell.Current.DisplayAlert(
                    "Notifications are off",
                    "Task reminders need notification permission before I can send them.",
                    "OK");

                task.ReminderEnabled = false;
                task.ReminderDateTime = null;
                task.IsEditingReminder = false;

                await _taskService.UpdateTaskAsync(task);
                await LoadTasksAsync();

                return;
            }

            var canScheduleExact = await _reminderEngine.CanScheduleExactRemindersAsync();

            if (!canScheduleExact)
            {
                var openSettings = await Shell.Current.DisplayAlert(
                    "More accurate reminders",
                    "Android may delay task reminders unless 'Alarms & reminders' is enabled for this app. Open settings now?",
                    "Open settings",
                    "Not now");

                if (openSettings)
                {
                    await _reminderEngine.OpenExactReminderSettingsAsync();
                }
            }
#endif

            task.ReminderDateTime = reminderDateTime;
            task.IsEditingReminder = false;

            await _taskService.UpdateTaskAsync(task);
            await _reminderEngine.ScheduleTaskReminderAsync(task);

            StatusMessage = "Task reminder saved.";

            await LoadTasksAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TodayViewModel] SaveTaskReminder failed: {ex}");
            StatusMessage = "Could not save task reminder.";
        }
    }

    [RelayCommand]
    private async Task AddWin()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NewWinText))
                return;

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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TodayViewModel] AddWin failed: {ex}");
            StatusMessage = "Could not save win.";
        }
    }

    [RelayCommand]
    private async Task CompleteTask(TaskItem task)
    {
        try
        {
            if (task is null)
                return;

            task.IsCompleted = true;
            task.CompletedUtc = DateTime.UtcNow;
            task.IsEditingReminder = false;

            await _reminderEngine.CancelTaskReminderAsync(task);
            await _taskService.CompleteTaskAsync(task.Id);

            StatusMessage = "Nice — task completed.";

            await LoadTasksAsync();
            UpdateFlowMessage();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TodayViewModel] CompleteTask failed: {ex}");
            StatusMessage = "Could not complete task.";
        }
    }
    
    public async Task LoadTasksAsync()
    {
        try
        {
            
            var tasks = await _taskService.GetAllTasksAsync();

            System.Diagnostics.Debug.WriteLine($"[TodayViewModel] Loaded {tasks.Count()} tasks from database.");

            Tasks.Clear();

            foreach (var task in tasks
                    .OrderBy(t => t.IsCompleted)
                    .ThenByDescending(t => t.IsCompleted ? t.CompletedUtc : t.CreatedUtc)
                    .ThenByDescending(t => t.CreatedUtc))
            {
                if (task.ReminderDateTime.HasValue)
                {
                    task.ReminderDate = task.ReminderDateTime.Value.Date;
                    task.ReminderTime = task.ReminderDateTime.Value.TimeOfDay;
                }

                Tasks.Add(task);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TodayViewModel] LoadTasksAsync failed: {ex}");
            StatusMessage = "Could not load tasks.";
        }
    }

    public async Task LoadWinsAsync()
    {
        try
        {
            var wins = await _winService.GetRecentWinsAsync();

            RecentWins.Clear();

            foreach (var win in wins.Take(5))
            {
                RecentWins.Add(win);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TodayViewModel] LoadWinsAsync failed: {ex}");
        }
    }

    public async Task LoadTruthBombAsync()
    {
        try
        {
            var bomb = await _truthBombService.GetTruthBombAsync();
            TruthBombText = bomb.Text;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TodayViewModel] LoadTruthBombAsync failed: {ex}");
        }
    }

    public async Task LoadLatestCheckInAsync()
    {
        try
        {
            var latestCheckIn = await _checkInService.GetLatestCheckInAsync();

            if (latestCheckIn is null)
                return;

            SelectedMood = latestCheckIn.Mood;
            SelectedMoodScore = latestCheckIn.MoodScore > 0
                ? latestCheckIn.MoodScore
                : latestCheckIn.EnergyLevel;

            SelectedMoodEmoji = latestCheckIn.MoodEmoji;

            if (!string.IsNullOrWhiteSpace(SelectedMood))
            {
                SelectedMoodDisplayText = $"Last check-in: {SelectedMoodEmoji} {SelectedMood}";
                NextStepText = GetNextStep(SelectedMoodScore);
                HasSavedCheckIn = true;
                UpdateMoodSelectionState();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TodayViewModel] LoadLatestCheckInAsync failed: {ex}");
        }
    }

    private async Task LoadGreetingAsync()
    {
        try
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TodayViewModel] LoadGreetingAsync failed: {ex}");
        }
    }

    private void UpdateFlowMessage()
    {
        if (string.IsNullOrWhiteSpace(SelectedMood))
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

    private string GetNextStep(int moodScore)
    {
        return moodScore switch
        {
            5 => "You’re in a good place. Pick one thing you can move forward while your brain is cooperating.",
            4 => "Nice. Keep it light and choose one small task to build momentum.",
            3 => "Let’s keep it simple. Pick one task you can finish in five minutes.",
            2 => "Go gentle. Choose the smallest possible step, even if it feels ridiculously easy.",
            1 => "No pressure today. Start with something kind to your nervous system: water, food, movement, or a reset.",
            _ => "Pick one small thing. Tiny steps still count."
        };
    }

    private void UpdateMoodSelectionState()
    {
        // Reset everything first
        IsGreatSelected = false;
        IsGoodSelected = false;
        IsOkaySelected = false;
        IsLowSelected = false;
        IsStrugglingSelected = false;

        // Then set the one that matches
        switch (SelectedMood?.Trim())
        {
            case "Great":
                IsGreatSelected = true;
                break;

            case "Good":
                IsGoodSelected = true;
                break;

            case "Okay":
                IsOkaySelected = true;
                break;

            case "Low":
                IsLowSelected = true;
                break;

            case "Struggling":
                IsStrugglingSelected = true;
                break;
        }
    }
}