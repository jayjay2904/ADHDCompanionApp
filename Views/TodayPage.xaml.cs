using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.ViewModels;

namespace ADHDCompanionApp.Views;

public partial class TodayPage : ContentPage
{
    private bool _isLoadingData;

    public TodayPage(TodayViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        viewModel.CelebrationRequested += ShowCelebrationAsync;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Shell.SetTabBarIsVisible(this, true);

        if (BindingContext is TodayViewModel viewModel)
        {
            try
            {
                _isLoadingData = true;
                await viewModel.LoadDataAsync();
            }
            finally
            {
                _isLoadingData = false;
            }
        }
    }

    private async void OnPreferencesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PreferencesPage));
    }

    private async Task ShowCelebrationAsync()
    {
        var messages = new[]
        {
            "Nice job",
            "Well done",
            "That counts",
            "Progress made",
            "Good move"
        };

        CelebrationText.Text = messages[Random.Shared.Next(messages.Length)];

        CelebrationOverlay.IsVisible = true;

        CelebrationStack.Opacity = 0;
        CelebrationStack.Scale = 0.6;
        CelebrationStack.TranslationY = 30;

        await Task.WhenAll(
            CelebrationStack.FadeTo(1, 220, Easing.CubicOut),
            CelebrationStack.ScaleTo(1.25, 300, Easing.CubicOut),
            CelebrationStack.TranslateTo(0, 0, 300, Easing.CubicOut)
        );

        await Task.Delay(1200);

        await Task.WhenAll(
            CelebrationStack.FadeTo(0, 350, Easing.CubicIn),
            CelebrationStack.ScaleTo(1.4, 350, Easing.CubicIn),
            CelebrationStack.TranslateTo(0, -40, 350, Easing.CubicIn)
        );

        CelebrationOverlay.IsVisible = false;

        CelebrationStack.TranslationY = 0;
        CelebrationStack.Scale = 1;
    }
    private async void OnTaskReminderToggled(object sender, ToggledEventArgs e)
    {
        if (_isLoadingData)
            return;

        if (sender is not Switch reminderSwitch)
            return;

        if (reminderSwitch.BindingContext is not TaskItem task)
            return;

        if (BindingContext is not TodayViewModel viewModel)
            return;

        await viewModel.SetTaskReminderEnabledAsync(task, e.Value);
    }
}