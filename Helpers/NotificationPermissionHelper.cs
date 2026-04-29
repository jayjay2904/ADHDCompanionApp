namespace ADHDCompanionApp.Helpers;

public static class NotificationPermissionHelper
{
    public static async Task<bool> EnsureNotificationsAllowedAsync()
    {
#if ANDROID
        var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.PostNotifications>();
        }

        if (status != PermissionStatus.Granted)
        {
            bool openSettings = await Shell.Current.DisplayAlert(
                "Notifications are off",
                "Arlo needs notifications enabled so reminders can work properly.",
                "Open settings",
                "Not now");

            if (openSettings)
                AppInfo.ShowSettingsUI();

            return false;
        }
#endif

        return true;
    }
}