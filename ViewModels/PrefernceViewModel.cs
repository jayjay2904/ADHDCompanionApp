using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.ViewModels;

public partial class PreferencesViewModel : BaseViewModel
{
    private readonly IUserProfileService _profileService;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private string nickname = string.Empty;

    [ObservableProperty]
    private bool usesMedicationSupport;

    [ObservableProperty]
    private bool usesTaskSupport = true;

    [ObservableProperty]
    private TimeSpan reminderTime = new(9, 0, 0);

    [ObservableProperty]
    private DateTime medicationStartDate = DateTime.Today;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public PreferencesViewModel(
        IUserProfileService profileService,
        INotificationService notificationService)
    {
        _profileService = profileService;
        _notificationService = notificationService;

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
        UsesTaskSupport = profile.UsesTaskSupport;
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
            UsesTaskSupport = UsesTaskSupport,
            MedicationReminderTime = UsesMedicationSupport ? ReminderTime : null,
            MedicationStartDate = UsesMedicationSupport ? MedicationStartDate : null,
            CreatedUtc = existingProfile?.CreatedUtc ?? DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };

        await _profileService.SaveProfileAsync(profile);

#if ANDROID
        var permission = await Permissions.RequestAsync<Permissions.PostNotifications>();

        if (UsesMedicationSupport &&
            profile.MedicationReminderTime.HasValue &&
            profile.MedicationStartDate.HasValue)
        {
            if (permission == PermissionStatus.Granted)
            {
                await _notificationService.ScheduleDailyMedicationReminderAsync(
                    profile.Nickname,
                    profile.MedicationStartDate.Value,
                    profile.MedicationReminderTime.Value);
            }
            else
            {
                StatusMessage = "Preferences saved, but notification permission was not granted.";
                return;
            }
        }
        else
        {
            await _notificationService.CancelMedicationReminderAsync();
        }
#endif

        StatusMessage = "Preferences saved.";
    }
}