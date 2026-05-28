using ADHDCompanionApp.ViewModels;
using CommunityToolkit.Maui.Views;

namespace ADHDCompanionApp.Views;

public partial class ProgressSummaryPopup : Popup
{
    private readonly ProgressSummaryViewModel _viewModel;

    public ProgressSummaryPopup(ProgressSummaryViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;

        //_= LoadAsync();
    }

    public async Task LoadAsync()
    {
        await _viewModel.LoadAsync();
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }
}