using ADHDCompanionApp.ViewModels;

namespace ADHDCompanionApp.Views;

public partial class SupportPage : ContentPage
{
    private readonly SupportViewModel _viewModel;

    public SupportPage(SupportViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSupportOptionsAsync();
    }
}