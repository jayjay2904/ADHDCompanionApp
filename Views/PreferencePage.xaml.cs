using ADHDCompanionApp.ViewModels;

namespace ADHDCompanionApp.Views;

public partial class PreferencesPage : ContentPage
{
    public PreferencesPage(PreferencesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    private async void HowToUseArlo_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushModalAsync(
            new MeetArloPage(fromPreferences: true));
    }
}