using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.ViewModels;

public partial class ArloViewModel : BaseViewModel
{
    private readonly IArloService _arloService;

    [ObservableProperty]
    private string userInput = string.Empty;

    [ObservableProperty]
    private bool areQuickPromptsVisible = true;

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public ObservableCollection<string> QuickPrompts { get; } = new()
    {
        "I'm overwhelmed",
        "I can't start",
        "I'm anxious",
        "I'm tired"
    };

    public ArloViewModel(IArloService arloService)
    {
        _arloService = arloService;
        Title = "Arlo";
    }

    public async Task LoadMessagesAsync()
    {
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
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(UserInput))
        {
            return;
        }

        var currentInput = UserInput.Trim();
        UserInput = string.Empty;

        await ProcessMessageAsync(currentInput);
    }

    [RelayCommand]
    private async Task SendQuickPrompt(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return;
        }

        await ProcessMessageAsync(prompt);
    }

    [RelayCommand]
    private async Task ClearChat()
    {
        await _arloService.ClearMessagesAsync();
        await LoadMessagesAsync();
    }

    private async Task ProcessMessageAsync(string input)
    {
        if (AreQuickPromptsVisible)
        {
            AreQuickPromptsVisible = false;
        }

        var userMessage = new ChatMessage
        {
            Role = "User",
            Text = input
        };

        Messages.Add(userMessage);
        await _arloService.AddMessageAsync(userMessage);

        await Task.Delay(400);

        var replyText = await _arloService.GetReplyAsync(input);

        var arloReply = new ChatMessage
        {
            Role = "Arlo",
            Text = replyText
        };

        Messages.Add(arloReply);
        await _arloService.AddMessageAsync(arloReply);
    }
}