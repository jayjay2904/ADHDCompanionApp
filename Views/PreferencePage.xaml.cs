using ADHDCompanionApp.ViewModels;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Views;

public partial class PreferencesPage : ContentPage
{
    private readonly IReminderEngine _reminderEngine;
    private readonly INotificationPermissionService _notificationPermissionService;

    public PreferencesPage(PreferencesViewModel viewModel, IReminderEngine reminderEngine, INotificationPermissionService notificationPermissionService )      
    {
        InitializeComponent();
        BindingContext = viewModel;
        _reminderEngine = reminderEngine;
        _notificationPermissionService = notificationPermissionService;
    }
    private async void HowToUseArlo_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushModalAsync(
            new MeetArloPage(fromPreferences: true));
    }
    private async void OnScheduleTestNotificationClicked(object sender, EventArgs e)
    {
        try
        {
            var hasExactReminderPermission =
                await _reminderEngine.CanScheduleExactRemindersAsync();

            if (!hasExactReminderPermission)
            {
                await DisplayAlert(
                    "Notifications need permission",
                    "Arlo needs notification permission to send gentle reminders.",
                    "Open settings");

                await _reminderEngine.OpenExactReminderSettingsAsync();
                return;
            }

            await _reminderEngine.ScheduleDebugNotificationAsync(
                "Arlo test",
                "Still here when you need me.",
                TimeSpan.FromMinutes(1));

            await DisplayAlert(
                "Test scheduled",
                "Close Arlo and wait 1 minute.",
                "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Arlo Notifications] Test failed: {ex}");

            await DisplayAlert(
                "Notification test failed",
                ex.Message,
                "OK");
        }
    }
    private async void OnNotificationSettingsClicked(object sender, EventArgs e)
    {
        await _notificationPermissionService.OpenNotificationSettingsAsync();
    }
}