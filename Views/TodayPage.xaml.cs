using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.ViewModels;
using CommunityToolkit.Maui.Extensions;

namespace ADHDCompanionApp.Views;

public partial class TodayPage : ContentPage
{
    public TodayPage(TodayViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Shell.SetTabBarIsVisible(this, true);

        if (BindingContext is TodayViewModel viewModel)
        {
            await viewModel.LoadDataAsync();
        }
    }

    private async void OnPreferencesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PreferencesPage));
    }

    private async void OnTaskCardTapped(object sender, TappedEventArgs e)
    {
        if (sender is not VisualElement tappedArea)
            return;

        if (tappedArea.BindingContext is not TaskItem task)
            return;

        if (task.IsCompleted)
            return;

        if (BindingContext is not TodayViewModel viewModel)
            return;

        Element? current = tappedArea;

        while (current is not null && current is not Microsoft.Maui.Controls.Frame)
        {
            current = current.Parent;
        }

        if (current is not Microsoft.Maui.Controls.Frame taskFrame)
            return;

        var originalBackground = taskFrame.BackgroundColor;

        var thumb = taskFrame.FindByName<Label>("CardThumbsUp");

        if (thumb is not null)
        {
            thumb.Opacity = 0;
            thumb.Scale = 0.6;
            thumb.TranslationY = 10;
        }

        // 👇 PRESS FEEL
        await taskFrame.ScaleTo(0.97, 80, Easing.CubicOut);

        // 👇 INSTANT VISUAL FEEDBACK
        taskFrame.BackgroundColor = Color.FromArgb("#B3CF99");

        // 👇 RELEASE
        await taskFrame.ScaleTo(1.0, 120, Easing.CubicOut);

        if (thumb is not null)
        {
            await Task.WhenAll(
                thumb.FadeTo(1, 120, Easing.CubicOut),
                thumb.ScaleTo(1.2, 160, Easing.CubicOut),
                thumb.TranslateTo(0, 0, 160, Easing.CubicOut)
            );
        }

        await Task.Delay(180);

        await taskFrame.BackgroundColorTo(originalBackground, 300);

        if (thumb is not null)
        {
            await Task.WhenAll(
                thumb.FadeTo(0, 220, Easing.CubicIn),
                thumb.ScaleTo(1.45, 220, Easing.CubicIn),
                thumb.TranslateTo(0, -12, 220, Easing.CubicIn)
            );
        }

        await viewModel.ToggleTaskCommand.ExecuteAsync(task);
    }
}