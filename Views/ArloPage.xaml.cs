using System.Collections.Specialized;
using ADHDCompanionApp.ViewModels;
using Microsoft.Maui.Devices;

namespace ADHDCompanionApp.Views;

public partial class ArloPage : ContentPage
{
    private readonly ArloViewModel _viewModel;
    private bool _shouldPulseInput;
    private bool _isInputPulseRunning;

    public ArloPage(ArloViewModel vm)
    {
        InitializeComponent();

        _viewModel = vm;
        BindingContext = _viewModel;

        _viewModel.Messages.CollectionChanged += OnMessagesCollectionChanged;
        _viewModel.ArloFinishedResponding += OnArloFinishedResponding;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadMessagesAsync();

        if (ChatInputShadow is not null)
        {
            ChatInputShadow.IsVisible = true;
            ChatInputShadow.Color = Color.FromArgb("#9B6AD6");
            ChatInputShadow.Opacity = 0.24;
        }

        _shouldPulseInput = true;
        StartInputPulse();
    }

    protected override void OnDisappearing()
    {
        StopInputPulse();

        base.OnDisappearing();
    }

    private async void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        await ScrollToBottomAsync();
    }

    private async Task ScrollToBottomAsync()
    {
        await Task.Delay(50);

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await MessagesScrollView.ScrollToAsync(0, MessagesStack.Height, true);
        });
    }

    private async void OnPreferencesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PreferencesPage));
    }

    private async void OnUserInputCompleted(object sender, EventArgs e)
    {
        if (BindingContext is ArloViewModel viewModel)
        {
            await viewModel.SendMessageCommand.ExecuteAsync(null);
        }

        StopInputPulse();
    }

    private async void OnQuickPromptTapped(object sender, TappedEventArgs e)
    {
        StopInputPulse();

        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ArloPage] Prompt haptic failed: {ex}");
        }

        if (sender is Frame frame)
        {
            await frame.ScaleTo(0.96, 70, Easing.CubicOut);
            await frame.ScaleTo(1.0, 110, Easing.CubicOut);
        }
    }

    private async void OnSendButtonPressed(object sender, EventArgs e)
    {
        StopInputPulse();

        if (sender is Button button)
        {
            await button.ScaleTo(0.92, 70, Easing.CubicOut);
        }
    }

    private async void OnSendButtonReleased(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            await button.ScaleTo(1.0, 110, Easing.CubicOut);
        }
    }

    private void OnArloFinishedResponding()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ArloPage] Finished response haptic failed: {ex}");
        }
    }

    private async void OnPromptPlusPressed(object sender, EventArgs e)
    {
        StopInputPulse();

        if (sender is Button button)
        {
            await button.ScaleTo(0.92, 70, Easing.CubicOut);
        }
    }

    private async void OnPromptPlusReleased(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            await button.ScaleTo(1.0, 110, Easing.CubicOut);
        }
    }

    private async void StartInputPulse()
    {
        if (_isInputPulseRunning)
            return;

        if (ChatInputShadow is null || ChatInputFrame is null)
            return;

        _isInputPulseRunning = true;

        while (_shouldPulseInput)
        {
            await Task.WhenAll(
                ChatInputShadow.FadeTo(0.48, 1200, Easing.SinInOut),
                ChatInputFrame.ScaleTo(1.006, 1200, Easing.SinInOut),
                ChatInputFrame.TranslateTo(0, -1, 1200, Easing.SinInOut)
            );

            if (!_shouldPulseInput)
                break;

            await Task.WhenAll(
                ChatInputShadow.FadeTo(0.20, 1200, Easing.SinInOut),
                ChatInputFrame.ScaleTo(1.0, 1200, Easing.SinInOut),
                ChatInputFrame.TranslateTo(0, 0, 1200, Easing.SinInOut)
            );
        }

        ChatInputShadow.Opacity = 0.20;
        ChatInputFrame.Scale = 1.0;
        ChatInputFrame.TranslationY = 0;
        _isInputPulseRunning = false;
    }

    private void StopInputPulse()
    {
        _shouldPulseInput = false;

        if (ChatInputShadow is not null)
        {
            ChatInputShadow.AbortAnimation("InputShadowPulse");
            ChatInputShadow.IsVisible = false;
            ChatInputShadow.Opacity = 0;
        }

        if (ChatInputFrame is not null)
        {
            ChatInputFrame.AbortAnimation("InputFramePulse");
            ChatInputFrame.Scale = 1.0;
            ChatInputFrame.TranslationY = 0;
        }
    }

    private void UserInputEditor_Focused(object sender, FocusEventArgs e)
    {
        StopInputPulse();
    }

    private void OnMicButtonClicked(object sender, EventArgs e)
    {
        StopInputPulse();
    }
}