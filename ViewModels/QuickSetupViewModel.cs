using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.ViewModels;

public partial class QuickSetupViewModel : BaseViewModel
{
    private readonly IUserProfileService _profileService;
    private readonly IReminderEngine _reminderEngine;

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

    public QuickSetupViewModel(
    IUserProfileService profileService,
    IReminderEngine reminderEngine)
    {
        _profileService = profileService;
        _reminderEngine = reminderEngine;
        LoadExistingProfile();
    }

    private async void LoadExistingProfile()
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
        try
        {
            if (string.IsNullOrWhiteSpace(Nickname))
            {
                await Shell.Current.DisplayAlert("Just one thing", "Please tell me what to call you.", "OK");
                return;
            }

            var profile = new UserProfile
            {
                Nickname = Nickname.Trim(),
                UsesMedicationSupport = UsesMedicationSupport,
                UsesTaskSupport = UsesTaskSupport,
                MedicationReminderTime = UsesMedicationSupport ? ReminderTime : null,
                MedicationStartDate = UsesMedicationSupport ? MedicationStartDate : null,
                UpdatedUtc = DateTime.UtcNow
            };

            await _profileService.SaveProfileAsync(profile);

#if ANDROID
            if (UsesMedicationSupport && profile.MedicationReminderTime.HasValue && profile.MedicationStartDate.HasValue)
            {
                var notificationPermission = await Permissions.RequestAsync<Permissions.PostNotifications>();

                if (notificationPermission == PermissionStatus.Granted)
                {
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
                }
                else
                {
                    await Shell.Current.DisplayAlert(
                        "Notifications are off",
                        "Medication reminders need notification permission before I can send them.",
                        "OK");

                    await _reminderEngine.CancelMedicationReminderAsync();
                }
            }
            else
            {
                await _reminderEngine.CancelMedicationReminderAsync();
            }
#endif

            await Shell.Current.GoToAsync("//TodayPage");
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