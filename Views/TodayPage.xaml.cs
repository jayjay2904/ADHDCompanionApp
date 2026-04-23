using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.ViewModels;

namespace ADHDCompanionApp.Views;

public partial class TodayPage : ContentPage
{
    private readonly TodayViewModel _viewModel;

    public TodayPage(TodayViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadTasksAsync();
        await _viewModel.LoadWinsAsync();
        await _viewModel.LoadTruthBombAsync();
        await _viewModel.LoadDataAsync();
    }
    private async void OnPreferencesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PreferencesPage));
    }



}