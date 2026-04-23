using ADHDCompanionApp.ViewModels;

namespace ADHDCompanionApp.Views;

public partial class SupportPage : ContentPage
{
    private readonly SupportViewModel _viewModel;

    public SupportPage(SupportViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadSupportOptionsAsync();
    }

    private async void OnPreferencesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PreferencesPage));
    }
}