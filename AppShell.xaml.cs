using ADHDCompanionApp.Services.Interfaces;
using ADHDCompanionApp.Views;

namespace ADHDCompanionApp;

public partial class AppShell : Shell
{
    private readonly IUserProfileService _profileService;

    public AppShell(IUserProfileService profileService)
    {
        InitializeComponent();

        _profileService = profileService;

        Routing.RegisterRoute(nameof(WelcomePage), typeof(WelcomePage));
        Routing.RegisterRoute(nameof(QuickSetupPage), typeof(QuickSetupPage));
        Routing.RegisterRoute(nameof(PreferencesPage), typeof(PreferencesPage));
    }

    public async Task InitialiseAsync()
    {
        var isOnboardingComplete = await _profileService.IsOnboardingCompleteAsync();

        if (!isOnboardingComplete)
        {
            await GoToAsync(nameof(WelcomePage));
        }
        else
        {
            await GoToAsync("//TodayPage");
        }
    }
    private async void OnPreferencesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PreferencesPage));
    }
}