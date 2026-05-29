using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services;
using ADHDCompanionApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ADHDCompanionApp.ViewModels;

public partial class PreferencesViewModel : BaseViewModel
{
    private readonly IUserProfileService _profileService;
    private readonly IReminderEngine _reminderEngine;

    [ObservableProperty]
    private string nickname = string.Empty;

    [ObservableProperty]
    private bool usesMedicationSupport;

    [ObservableProperty]
    private TimeSpan reminderTime = new(9, 0, 0);

    [ObservableProperty]
    private DateTime medicationStartDate = DateTime.Today;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public PreferencesViewModel(
    IUserProfileService profileService,
    IReminderEngine reminderEngine)
    {
        _profileService = profileService;
        _reminderEngine = reminderEngine;

        Title = "Preferences";
        LoadPreferences();
    }

    private async void LoadPreferences()
    {
        var profile = await _profileService.GetProfileAsync();

        if (profile is null)
            return;

        Nickname = profile.Nickname;
        UsesMedicationSupport = profile.UsesMedicationSupport;
        ReminderTime = profile.MedicationReminderTime ?? new TimeSpan(9, 0, 0);
        MedicationStartDate = profile.MedicationStartDate ?? DateTime.Today;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(Nickname))
        {
            await Shell.Current.DisplayAlert(
                "Just one thing",
                "Please tell me what to call you.",
                "OK");
            return;
        }

        var existingProfile = await _profileService.GetProfileAsync();

        var profile = new UserProfile
        {
            Id = existingProfile?.Id ?? Guid.NewGuid().ToString(),
            Nickname = Nickname.Trim(),
            PreferredTone = existingProfile?.PreferredTone ?? string.Empty,
            ReminderStyle = existingProfile?.ReminderStyle ?? string.Empty,
            UsesMedicationSupport = UsesMedicationSupport,
            MedicationReminderTime = UsesMedicationSupport ? ReminderTime : null,
            MedicationStartDate = UsesMedicationSupport ? MedicationStartDate : null,
            CreatedUtc = existingProfile?.CreatedUtc ?? DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };

        await _profileService.SaveProfileAsync(profile);

#if ANDROID
        if (UsesMedicationSupport &&
    profile.MedicationReminderTime.HasValue &&
    profile.MedicationStartDate.HasValue)
        {
            var permission = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();

            if (permission != PermissionStatus.Granted)
            {
                permission = await Permissions.RequestAsync<Permissions.PostNotifications>();
            }

            if (permission != PermissionStatus.Granted)
            {
                await _reminderEngine.CancelMedicationReminderAsync();

                StatusMessage = "Preferences saved, but notification permission was not granted.";
                return;
            }

            var canScheduleExact = await _reminderEngine.CanScheduleExactRemindersAsync();

            if (!canScheduleExact)
            {
                var openSettings = await Shell.Current.DisplayAlert(
                    "More accurate reminders",
                    "Android may delay medication reminders unless 'Alarms & reminders' is enabled for this app. Open settings now?",
                    "Open settings",
                    "Not now");

                if (openSettings)
                {
                    await _reminderEngine.OpenExactReminderSettingsAsync();
                }
            }

            await _reminderEngine.ScheduleMedicationReminderAsync(profile);

            StatusMessage = $"Preferences saved. Reminder set for {profile.MedicationReminderTime.Value:hh\\:mm}.";
            await Shell.Current.Navigation.PopModalAsync();
        }
        else
        {
            await _reminderEngine.CancelMedicationReminderAsync();
            StatusMessage = "Preferences saved. Medication reminders are off.";
            await Shell.Current.Navigation.PopModalAsync();
        }
#else
StatusMessage = "Preferences saved.";
await Shell.Current.Navigation.PopModalAsync();
#endif
    }

    [RelayCommand]
    private async Task TestRestoreReminder()
    {
        try
        {
            StatusMessage = "Testing restore...";

            await _reminderEngine.RestoreRemindersAsync();

            StatusMessage = "Restore test completed. Check logs.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Restore test failed.";
            System.Diagnostics.Debug.WriteLine($"[RestoreTest] Error: {ex}");
        }
    }
    [RelayCommand]
    private async Task ShowAboutArlo()
    {
        await Shell.Current.DisplayAlert(
            "About Arlo",
            "Arlo is your low-effort support companion.\n\n" +
            "You can talk to Arlo when you feel stuck, overwhelmed, anxious, low on energy, or just need somewhere to start.\n\n" +
            "Arlo can help you:\n" +
            "• feel a bit less alone in the moment\n" +
            "• break things down without pressure\n" +
            "• capture wins automatically\n" +
            "• notice recent check-ins and progress\n" +
            "• set reminders when you mention something you need to remember\n" +
            "• support medication reminders if you turn them on\n\n" +
            "! Arlo is not emergency, medical, or crisis support. If you feel unsafe or at risk, contact emergency services or someone you trust.",
            "OK");
    }
}