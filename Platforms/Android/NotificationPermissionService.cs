using Android.Content;
using Android.Provider;
using ADHDCompanionApp.Services.Interfaces;


namespace ADHDCompanionApp.Platforms.Android.Services;

public class NotificationPermissionService : INotificationPermissionService
{
    public async Task<bool> RequestNotificationPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.PostNotifications>();
        }

        return status == PermissionStatus.Granted;
    }

    public Task OpenNotificationSettingsAsync()
    {
        var context = Platform.CurrentActivity ?? global::Android.App.Application.Context;

        var intent = new Intent(Settings.ActionAppNotificationSettings)
            .PutExtra(Settings.ExtraAppPackage, context.PackageName);

        intent.AddFlags(ActivityFlags.NewTask);
        context.StartActivity(intent);

        return Task.CompletedTask;
    }
}