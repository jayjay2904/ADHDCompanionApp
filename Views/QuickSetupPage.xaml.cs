using ADHDCompanionApp.ViewModels;

namespace ADHDCompanionApp.Views;

public partial class QuickSetupPage : ContentPage
{
    public QuickSetupPage(QuickSetupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        Shell.SetTabBarIsVisible(this, false);
    }
}