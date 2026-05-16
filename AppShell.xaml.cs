using ADHDCompanionApp.Services.Interfaces;
using ADHDCompanionApp.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ADHDCompanionApp;

public partial class AppShell : Shell
{
    private readonly IUserProfileService _profileService;
    private readonly IServiceProvider _serviceProvider;
    private bool _hasInitialised;
    private bool _isResettingTab;


    public AppShell(IUserProfileService profileService, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _profileService = profileService;
        _serviceProvider = serviceProvider;

        
        Routing.RegisterRoute(nameof(PreferencesPage), typeof(PreferencesPage));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_hasInitialised)
            return;

        _hasInitialised = true;

        await InitialiseAsync();
    }
    public async Task InitialiseAsync()
    {
        try
        {
            var isOnboardingComplete = await _profileService.IsOnboardingCompleteAsync();

            MainTabBar.IsVisible = isOnboardingComplete;

            if (!isOnboardingComplete)
            {
                var quickSetupPage = _serviceProvider.GetRequiredService<QuickSetupPage>();
                await Shell.Current.Navigation.PushAsync(quickSetupPage);
                return;
            }

            await GoToAsync("//ArloPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AppShell] InitialiseAsync failed: {ex}");

            MainTabBar.IsVisible = false;

            try
            {
                var quickSetupPage = _serviceProvider.GetRequiredService<QuickSetupPage>();
                await Shell.Current.Navigation.PushAsync(quickSetupPage);
            }
            catch (Exception navEx)
            {
                System.Diagnostics.Debug.WriteLine($"[AppShell] Fallback navigation failed: {navEx}");
            }
        }
    }

    private async void OnPreferencesClicked(object sender, EventArgs e)
    {
        try
        {
            var preferencesPage = _serviceProvider.GetRequiredService<PreferencesPage>();
            await Shell.Current.Navigation.PushModalAsync(preferencesPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AppShell] Preferences navigation failed: {ex}");
        }
    }

    public async Task UpdateNavigationForSetupStateAsync()
    {
        try
        {
            var isOnboardingComplete = await _profileService.IsOnboardingCompleteAsync();
            MainTabBar.IsVisible = isOnboardingComplete;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AppShell] UpdateNavigationForSetupStateAsync failed: {ex}");
            MainTabBar.IsVisible = false;
        }
    }
    
    protected override async void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        if (_isResettingTab)
            return;

        try
        {
            var location = args.Current.Location.OriginalString;

            if (location.Contains("PreferencesPage"))
                return;

            if (location.Contains("TodayPage/") ||
                location.Contains("ArloPage/") ||
                location.Contains("SupportPage/") ||
                location.Contains("ProgressPage/"))
            {
                _isResettingTab = true;

                if (location.Contains("TodayPage"))
                    await Shell.Current.GoToAsync("//TodayPage");
                else if (location.Contains("ArloPage"))
                    await Shell.Current.GoToAsync("//ArloPage");
                else if (location.Contains("SupportPage"))
                    await Shell.Current.GoToAsync("//SupportPage");
                else if (location.Contains("ProgressPage"))
                    await Shell.Current.GoToAsync("//ProgressPage");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AppShell] Tab reset failed: {ex}");
        }
        finally
        {
            _isResettingTab = false;
        }
    }

}