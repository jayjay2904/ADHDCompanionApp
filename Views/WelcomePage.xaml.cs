using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Views;

public partial class WelcomePage : ContentPage
{
    private IUserProfileService? _profileService;
    private bool _hasCompletedSetup;

    public WelcomePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _profileService ??= Application.Current?
            .Handler?
            .MauiContext?
            .Services
            .GetService<IUserProfileService>();

        if (_profileService is null)
            return;

        var onboardingComplete = await _profileService.IsOnboardingCompleteAsync();
        var profile = await _profileService.GetProfileAsync();

        if (onboardingComplete && profile is not null && !string.IsNullOrWhiteSpace(profile.Nickname))
        {
            _hasCompletedSetup = true;
            WelcomeMessageLabel.Text = $"Welcome back {profile.Nickname}, how are you feeling today?";
            GetStartedButton.Text = "Continue";
        }
        else
        {
            _hasCompletedSetup = false;
            WelcomeMessageLabel.Text = "Your calm place to land.";
            GetStartedButton.Text = "Get Started";
        }
    }

    private async void OnGetStartedClicked(object sender, EventArgs e)
    {
        if (_hasCompletedSetup)
        {
            await Shell.Current.GoToAsync("//TodayPage");
        }
        else
        {
            await Shell.Current.GoToAsync(nameof(QuickSetupPage));
        }
    }
}