namespace ADHDCompanionApp.Views;

public partial class MeetArloPage : ContentPage
{
    private readonly bool _fromPreferences;

    public MeetArloPage(bool fromPreferences = false)
    {
        InitializeComponent();

        _fromPreferences = fromPreferences;

        ActionButton.Text = fromPreferences
            ? "Close"
            : "Got It";
    }

    private async void ActionButton_Clicked(object sender, EventArgs e)
    {
        if (_fromPreferences)
        {
            await Shell.Current.Navigation.PopModalAsync();
            return;
        }

        Preferences.Set("HasSeenArloGuide", true);

        await Shell.Current.GoToAsync("//ArloPage");
    }
}