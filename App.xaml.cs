namespace ADHDCompanionApp;

public partial class App : Application
{
    public App(AppShell shell)
    {
        InitializeComponent();

        MainPage = shell;

        Dispatcher.Dispatch(async () =>
        {
            await shell.InitialiseAsync();
        });
    }
}