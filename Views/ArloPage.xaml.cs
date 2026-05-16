using System.Collections.Specialized;
using ADHDCompanionApp.ViewModels;
using Microsoft.Maui.Devices;

namespace ADHDCompanionApp.Views;

public partial class ArloPage : ContentPage
{
    private readonly ArloViewModel _viewModel;

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
    }

    private async void OnQuickPromptTapped(object sender, TappedEventArgs e)
    {
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
}