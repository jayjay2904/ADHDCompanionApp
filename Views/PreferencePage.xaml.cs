using ADHDCompanionApp.ViewModels;

namespace ADHDCompanionApp.Views;

public partial class PreferencesPage : ContentPage
{
    public PreferencesPage(PreferencesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}