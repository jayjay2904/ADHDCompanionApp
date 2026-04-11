using ADHDCompanionApp.ViewModels;

namespace ADHDCompanionApp.Views;

public partial class QuickSetupPage : ContentPage
{
    public QuickSetupPage(QuickSetupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}