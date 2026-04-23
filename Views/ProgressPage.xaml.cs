using ADHDCompanionApp.ViewModels;

namespace ADHDCompanionApp.Views;

public partial class ProgressPage : ContentPage
{
    private readonly ProgressViewModel _viewModel;

    public ProgressPage(ProgressViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadProgressAsync();
    }
    private async void OnPreferencesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PreferencesPage));
    }
}