using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Dispatching;

namespace ADHDCompanionApp.ViewModels;

public partial class ArloViewModel : BaseViewModel
{
    private readonly IArloService _arloService;

    private readonly ISpeechToTextService _speechToTextService;

    public event Action? ArloFinishedResponding;

    [ObservableProperty]
    private string userInput = string.Empty;

    [ObservableProperty]
    private bool areQuickPromptsVisible = true;

    [ObservableProperty]
    private bool arePromptPickerVisible;

    [ObservableProperty]
    private string greetingText = "Hi.";

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public ObservableCollection<ArloPrompt> QuickPrompts { get; } = new()
{
    new() { Text = "I’m overwhelmed", Icon = "cloud.png" },
    new() { Text = "I can’t start", Icon = "play.png" },
    new() { Text = "I’ve low energy", Icon = "battery.png" },
    new() { Text = "I’m anxious", Icon = "anxious.png" },
    new() { Text = "I feel stuck", Icon = "pause.png" },
    new() { Text = "I’m overstimulated", Icon = "list.png" }
};

    private readonly IUserProfileService _profileService;

    public ArloViewModel(
        IArloService arloService,
        IUserProfileService profileService,
        ISpeechToTextService speechToTextService)
    {
        _arloService = arloService;
        _profileService = profileService;
        _speechToTextService = speechToTextService;

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

            GreetingText = BuildGreeting(name, now, lastOpened);
        }
        catch
        {
            GreetingText = "Hi.";
        }
    }

    private static string BuildGreeting(string name, DateTime nowUtc, DateTime? lastOpenedUtc)
    {
        var localHour = DateTime.Now.Hour;

        var firstName = name.Split(' ')[0];

        // First ever launch
        if (lastOpenedUtc is null)
        {
            return $"Hi {firstName}.";
        }

        var gap = nowUtc - lastOpenedUtc.Value;

        // Recently returned
        if (gap.TotalHours < 3)
        {
            return $"Hey {firstName}. Still here with you.";
        }

        // Same day return
        if (gap.TotalHours < 24)
        {
            return $"Hey {firstName}. Glad you’re back.";
        }

        // Few days away
        if (gap.TotalDays >= 2)
        {
            return $"Hey {firstName}. No pressure. Let’s just start where you are.";
        }

        // Morning
        if (localHour < 12)
        {
            return $"Morning {firstName}. How’s your brain feeling today?";
        }

        // Evening
        if (localHour >= 18)
        {
            return $"Hey {firstName}. You don’t need to have everything figured out tonight.";
        }

        // Default
        return $"Hi {firstName}.";
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

    private async Task ProcessMessageAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        var userMessage = new ChatMessage
        {
            Role = "User",
            Text = input.Trim()
        };

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Messages.Add(userMessage);
        });

        await _arloService.AddMessageAsync(userMessage);

        AreQuickPromptsVisible = false;

        await Task.Delay(400);

        var replyText = await _arloService.GetReplyAsync(input);

        System.Diagnostics.Debug.WriteLine($"[Arlo] Reply text: '{replyText}'");

        if (string.IsNullOrWhiteSpace(replyText))
        {
            replyText = "Yeah — I’m here. Let’s keep this tiny. Take one slow breath, then pick the smallest next step.";
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
}