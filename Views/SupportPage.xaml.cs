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

        _viewModel.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName == nameof(SupportViewModel.HasSelectedOption) && _viewModel.HasSelectedOption)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Task.Delay(100);
                    await SupportScrollView.ScrollToAsync(SupportPlanSection, ScrollToPosition.Start, true);
                });
            }
        };
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