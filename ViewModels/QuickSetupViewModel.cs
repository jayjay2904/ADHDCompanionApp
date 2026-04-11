using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;
using ADHDCompanionApp.Views;

namespace ADHDCompanionApp.ViewModels;

public partial class QuickSetupViewModel : BaseViewModel
{
    private readonly IUserProfileService _profileService;

    [ObservableProperty]
    private string nickname = string.Empty;

    [ObservableProperty]
    private bool usesMedicationSupport;

    [ObservableProperty]
    private bool usesTaskSupport;

    public QuickSetupViewModel(IUserProfileService profileService)
    {
        _profileService = profileService;
    }

    [RelayCommand]
    private async Task Save()
    {
        var profile = new UserProfile
        {
            Nickname = Nickname,
            UsesMedicationSupport = UsesMedicationSupport,
            UsesTaskSupport = UsesTaskSupport
        };

        await _profileService.SaveProfileAsync(profile);

        await Shell.Current.GoToAsync("//TodayPage");
    }
}