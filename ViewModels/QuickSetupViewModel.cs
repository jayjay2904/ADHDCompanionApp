using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.ViewModels;

public partial class QuickSetupViewModel : BaseViewModel
{
    private readonly IUserProfileService _profileService;

    [ObservableProperty]
    private string nickname = string.Empty;

    [ObservableProperty]
    private bool usesMedicationSupport;

    [ObservableProperty]
    private bool usesTaskSupport = true;

    [ObservableProperty]
    private TimeSpan reminderTime = new(9, 0, 0);

    public QuickSetupViewModel(IUserProfileService profileService)
    {
        _profileService = profileService;
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
    }

    [RelayCommand]
    private async Task Save()
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
            UpdatedUtc = DateTime.UtcNow
        };

        await _profileService.SaveProfileAsync(profile);

        await Shell.Current.GoToAsync("//TodayPage");
    }
}