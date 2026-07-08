using ADHDCompanionApp.Models;
using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;
using ADHDCompanionApp.Views;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Dispatching;
using System.Collections.ObjectModel;
using System.Windows.Input;


namespace ADHDCompanionApp.ViewModels;

public partial class ArloViewModel : BaseViewModel
{
    private readonly IArloService _arloService;
    private readonly IUserActivityService _userActivityService;
    private readonly ISpeechToTextService _speechToTextService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IReminderEngine _reminderEngine;
    private ConversationMode _currentConversationMode = ConversationMode.None;
    private string? _pendingActionText;
    private bool _isProcessingMessage;
    private readonly IConversationIntentService _intentService;

    public event Action? ArloFinishedResponding;

    [ObservableProperty]
    private string userInput = string.Empty;

    [ObservableProperty]
    private bool areQuickPromptsVisible = true;

    [ObservableProperty]
    private bool arePromptPickerVisible;

    [ObservableProperty]
    private string greetingTitle = "Hi.";

    [ObservableProperty]
    private string greetingSubtitle = "I'm here with you.";

    [ObservableProperty]
    private bool isReminderSuggestionVisible;

    [ObservableProperty]
    private string pendingReminderText = string.Empty;

    [ObservableProperty]
    private bool isReminderSetupVisible;

    [ObservableProperty]
    private DateTime reminderDate = DateTime.Today;

    [ObservableProperty]
    private TimeSpan reminderTime = DateTime.Now.AddHours(1).TimeOfDay;

    public ICommand ShowProgressCommand { get; }

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public ObservableCollection<ArloPrompt> QuickPrompts { get; } = new()
{
    new() { Text = "I’m overwhelmed", Icon = "cloud.png" },
    new() { Text = "I can’t start", Icon = "play.png" },
    new() { Text = "I'm low on energy", Icon = "battery.png" },
    new() { Text = "I’m anxious", Icon = "anxious.png" },
    new() { Text = "I feel stuck", Icon = "pause.png" },
    new() { Text = "I’m overstimulated", Icon = "list.png" }
};

    private readonly IUserProfileService _profileService;

    public ArloViewModel(
        IArloService arloService,
        IUserProfileService profileService,
        ISpeechToTextService speechToTextService,
        IServiceProvider serviceProvider,
        IReminderEngine reminderEngine,
        IUserActivityService userActivityService,
        IConversationIntentService intentService)
    {
        _arloService = arloService;
        _profileService = profileService;
        _speechToTextService = speechToTextService;
        _serviceProvider = serviceProvider;
        ShowProgressCommand = new Command(ShowProgress);
        _reminderEngine = reminderEngine;
        _userActivityService = userActivityService;
        _intentService = intentService;

        Title = "Arlo";

        LoadGreeting();
    }
    
    private async void LoadGreeting()
    {
        try
        {
            var profile = await _profileService.GetProfileAsync();

            var name = string.IsNullOrWhiteSpace(profile?.Nickname)
                ? "there"
                : profile.Nickname.Trim();

            var now = DateTime.UtcNow;

            var lastOpenedString = Preferences.Get("LastOpenedUtc", string.Empty);

            DateTime? lastOpened = null;

            if (DateTime.TryParse(lastOpenedString, out var parsed))
            {
                lastOpened = parsed;
            }

            Preferences.Set("LastOpenedUtc", now.ToString("O"));

            var greeting = BuildGreeting(name, now, lastOpened);
            greetingTitle = greeting.Title;
            greetingSubtitle = greeting.Subtitle;
        }
        catch
        {
            greetingTitle = "Hi.";
            greetingSubtitle = "I'm here with you.";
        }
    }

    private static (string Title, string Subtitle) BuildGreeting(string name, DateTime nowUtc, DateTime? lastOpenedUtc)
    {
        var localHour = DateTime.Now.Hour;
        var firstName = name.Split(' ')[0];

        if (lastOpenedUtc is null)
            return ($"Hi {firstName}.", "I'm Arlo. Start wherever feels easiest.");

        var gap = nowUtc - lastOpenedUtc.Value;

        if (gap.TotalHours < 3)
            return ($"Hey {firstName}.", "Still here with you.");

        if (gap.TotalHours < 24)
            return ($"Hey {firstName}.", "Glad you’re back.");

        if (gap.TotalDays >= 2)
            return ($"Hey {firstName}.", "I'm here whenever you're ready.");

        if (localHour < 12)
            return ($"Morning {firstName}.", "Start wherever feels easiest.");

        if (localHour >= 18)
            return ($"Hey {firstName}.", "We can take this one step at a time.");

        return ($"Hi {firstName}.", "I'm here with you.");
    }

    public async Task LoadMessagesAsync()
    {
        if (Messages.Count > 0)
            return;

        var savedMessages = await _arloService.GetMessagesAsync();

        Messages.Clear();

        if (savedMessages.Count == 0)
        {
            var welcomeMessage = new ChatMessage
            {
                Role = "Arlo",
                Text = "Hey — I’m here. What’s going on?"
            };

            await _arloService.AddMessageAsync(welcomeMessage);
            Messages.Add(welcomeMessage);
            AreQuickPromptsVisible = true;
            return;
        }

        foreach (var message in savedMessages)
        {
            Messages.Add(message);
        }

        AreQuickPromptsVisible = Messages.Count <= 1;
    }

    [RelayCommand]
    private void ShowPrompts()
    {
        ArePromptPickerVisible = true;
    }

    [RelayCommand]
    private void HidePromptPicker()
    {
        ArePromptPickerVisible = false;
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        _ = _userActivityService.RecordInteractionAsync();
        if (string.IsNullOrWhiteSpace(UserInput))
            return;

        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ArloViewModel] Send haptic failed: {ex}");
        }

        var currentInput = UserInput.Trim();
        UserInput = string.Empty;

        await ProcessMessageAsync(currentInput);
     
    }

    [RelayCommand]
    private async Task SendQuickPrompt(string prompt)
    {
        _ = _userActivityService.RecordInteractionAsync();
        if (string.IsNullOrWhiteSpace(prompt))
            return;

        ArePromptPickerVisible = false;

        await ProcessMessageAsync(prompt);
    }

    [RelayCommand]
    private async Task Listen()
    {
        try
        {
            var spokenText = await _speechToTextService.ListenAsync();

            if (string.IsNullOrWhiteSpace(spokenText))
                return;

            _ = _userActivityService.RecordInteractionAsync();

            UserInput = spokenText.Trim();

            await Task.Delay(150);

            await SendMessage();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ArloViewModel] Listen failed: {ex}");
        }
    }

    [RelayCommand]
    private async Task ClearChat()
    {
        ArePromptPickerVisible = false;

        await _arloService.ClearMessagesAsync();

        Messages.Clear();

        await LoadMessagesAsync();
    }

    [RelayCommand]
    private void DismissReminderSuggestion()
    {
        IsReminderSuggestionVisible = false;
        PendingReminderText = string.Empty;
    }

   
    [RelayCommand]
    private void AcceptReminderSuggestion()
    {
        IsReminderSuggestionVisible = false;
        _currentConversationMode = ConversationMode.ReminderSetup;

        ReminderDate = DateTime.Today;
        ReminderTime = DateTime.Now.AddHours(1).TimeOfDay;

        IsReminderSetupVisible = true;
    }

    [RelayCommand]
    private async Task SaveReminder()
    {
        var reminderDateTime = ReminderDate.Date.Add(ReminderTime);

        if (reminderDateTime <= DateTime.Now)
        {
            await Shell.Current.CurrentPage.DisplayAlert(
                "Reminder",
                "Pick a time in the future.",
                "OK");

            return;
        }

        var canSchedule = await _reminderEngine.CanScheduleExactRemindersAsync();

        if (!canSchedule)
        {
            await Shell.Current.CurrentPage.DisplayAlert(
                "Reminder permission",
                "Your phone needs permission to schedule exact reminders. I’ll open the setting now.",
                "OK");

            await _reminderEngine.OpenExactReminderSettingsAsync();
            return;
        }

        var notificationId = Math.Abs(Guid.NewGuid().GetHashCode());

        var request = new ReminderRequest
        {
            ReminderKey = $"arlo_{notificationId}",
            NotificationId = notificationId,
            Type = ReminderType.Custom,
            Title = "Arlo reminder",
            Message = PendingReminderText,
            TriggerTime = reminderDateTime
        };

        await _reminderEngine.ScheduleReminderAsync(request);

        IsReminderSetupVisible = false;
        IsReminderSuggestionVisible = false;

        PendingReminderText = string.Empty;

        _currentConversationMode = ConversationMode.None;
        _pendingActionText = null;

        await Shell.Current.CurrentPage.DisplayAlert(
            "Reminder set",
            "I’ll remind you.",
            "OK");
    }
    [RelayCommand]
    private void CancelReminderSetup()
    {
        IsReminderSetupVisible = false;
        IsReminderSuggestionVisible = false;

        PendingReminderText = string.Empty;

        _currentConversationMode = ConversationMode.None;
        _pendingActionText = null;
    }
    private async Task ProcessMessageAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        if (_isProcessingMessage)
            return;

        _isProcessingMessage = true;

        ChatMessage? waitingMessage = null;
        var isWaiting = true;

        try
        {
            var trimmedInput = input.Trim();

            var userMessage = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                Role = "User",
                Text = trimmedInput,
                TimestampUtc = DateTime.UtcNow
            };

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Messages.Add(userMessage);
            });

            await _arloService.AddMessageAsync(userMessage);

            AreQuickPromptsVisible = false;

            await Task.Delay(400);

            waitingMessage = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                Role = "Arlo",
                Text = ".",
                TimestampUtc = DateTime.UtcNow
            };

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Messages.Add(waitingMessage);
            });

            var animationTask = Task.Run(async () =>
            {
                var frames = new[] { ".", "..", "...", "....", "..." };
                var frameIndex = 0;

                while (isWaiting)
                {
                    var frame = frames[frameIndex];

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        var index = Messages.IndexOf(waitingMessage);

                        if (index >= 0)
                        {
                            var updatedWaitingMessage = new ChatMessage
                            {
                                Id = waitingMessage.Id,
                                Role = "Arlo",
                                Text = frame,
                                TimestampUtc = waitingMessage.TimestampUtc
                            };

                            Messages[index] = updatedWaitingMessage;
                            waitingMessage = updatedWaitingMessage;
                        }
                    });

                    frameIndex = (frameIndex + 1) % frames.Length;

                    await Task.Delay(300);
                }
            });

            var replyText = await _arloService.GetReplyAsync(trimmedInput);

            isWaiting = false;
            await animationTask;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (waitingMessage is not null)
                {
                    Messages.Remove(waitingMessage);
                }
            });

            // Only offer a reminder if the user is creating a new reminder.
            // If they're asking Arlo to recall something, don't show the reminder suggestion.
            if (_intentService.LooksLikeReminderIntent(trimmedInput) &&
                !_intentService.LooksLikeRecallQuestion(trimmedInput))
            {
                PendingReminderText = _intentService.CleanReminderText(trimmedInput);
                _pendingActionText = PendingReminderText;

                IsReminderSuggestionVisible = true;
                _currentConversationMode = ConversationMode.ReminderSuggestion;
            }
            else if (_currentConversationMode == ConversationMode.None)
            {
                IsReminderSuggestionVisible = false;
                _pendingActionText = null;
            }

            System.Diagnostics.Debug.WriteLine($"[Arlo] Reply text: '{replyText}'");

            if (string.IsNullOrWhiteSpace(replyText))
            {
                replyText = "Yeah — I’m here. Let’s keep this small. Take one slow breath, then pick the smallest next step.";
            }

            var arloReply = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                Role = "Arlo",
                Text = string.Empty,
                TimestampUtc = DateTime.UtcNow
            };

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Messages.Add(arloReply);
            });

            var words = replyText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var currentText = string.Empty;

            foreach (var word in words)
            {
                currentText +=
                    (string.IsNullOrWhiteSpace(currentText) ? "" : " ")
                    + word;

                var updatedMessage = new ChatMessage
                {
                    Id = arloReply.Id,
                    Role = "Arlo",
                    Text = currentText,
                    TimestampUtc = arloReply.TimestampUtc
                };

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var index = Messages.IndexOf(arloReply);

                    if (index >= 0)
                    {
                        Messages[index] = updatedMessage;
                        arloReply = updatedMessage;
                    }
                });

                await Task.Delay(35);
            }

            await _arloService.AddMessageAsync(arloReply);

            ArloFinishedResponding?.Invoke();
        }
        finally
        {
            isWaiting = false;
            _isProcessingMessage = false;

            if (waitingMessage is not null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Messages.Remove(waitingMessage);
                });
            }
        }
    }
    private void ShowProgress()
    {
        var popup = _serviceProvider.GetRequiredService<ProgressSummaryPopup>();

        Shell.Current.CurrentPage.ShowPopup(popup);
    }
    
}