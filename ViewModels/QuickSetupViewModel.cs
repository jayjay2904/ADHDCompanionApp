using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;
using ADHDCompanionApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace ADHDCompanionApp.ViewModels;

public partial class QuickSetupViewModel : BaseViewModel
{
    private readonly IUserProfileService _profileService;
    private readonly INotificationPermissionService _notificationPermissionService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string nickname = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool hasAcceptedDisclaimer;

    public QuickSetupViewModel(IUserProfileService profileService, INotificationPermissionService notificationPermissionService)
    {
        _profileService = profileService;
        _notificationPermissionService = notificationPermissionService;
        Title = "Quick Setup";

        LoadExistingProfile();
    }

    private async void LoadExistingProfile()
    {
        var profile = await _profileService.GetProfileAsync();

        if (profile is null)
            return;

        Nickname = profile.Nickname;
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Nickname)
               && HasAcceptedDisclaimer;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        try
        {
            var existingProfile = await _profileService.GetProfileAsync();

            var profile = new UserProfile
            {
                Id = existingProfile?.Id ?? Guid.NewGuid().ToString(),
                Nickname = Nickname.Trim(),

                PreferredTone = existingProfile?.PreferredTone ?? string.Empty,
                ReminderStyle = existingProfile?.ReminderStyle ?? string.Empty,

                UsesMedicationSupport = existingProfile?.UsesMedicationSupport ?? false,
                UsesTaskSupport = existingProfile?.UsesTaskSupport ?? true,

                MedicationReminderTime = existingProfile?.MedicationReminderTime,
                MedicationStartDate = existingProfile?.MedicationStartDate,

                CreatedUtc = existingProfile?.CreatedUtc ?? DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            await _profileService.SaveProfileAsync(profile);

            Preferences.Set("HasAcceptedDisclaimer", true);
            Preferences.Set("IsOnboardingComplete", true);

            var notificationsAllowed =
            await _notificationPermissionService.RequestNotificationPermissionAsync();

            if (!notificationsAllowed)
            {
                await Shell.Current.DisplayAlert(
                    "Gentle reminders",
                    "No problem. You can turn reminders on later in Preferences.",
                    "OK");
            }

            if (Shell.Current is AppShell shell)
            {
                await shell.UpdateNavigationForSetupStateAsync();
            }

            await Shell.Current.GoToAsync("//ArloPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[QuickSetup] Save failed: {ex}");

            await Shell.Current.DisplayAlert(
                "Something went wrong",
                "I couldn't save your setup properly. Please try again.",
                "OK");
        }
    }
}