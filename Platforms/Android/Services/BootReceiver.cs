using Android.App;
using Android.Content;
using Android.Util;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Platforms.Android.Services;

[BroadcastReceiver(Enabled = true, Exported = false)]
[IntentFilter(new[]
{
    Intent.ActionBootCompleted,
    Intent.ActionMyPackageReplaced
})]
public class BootReceiver : BroadcastReceiver
{
    private const string LogTag = "ReminderEngine";

    public override async void OnReceive(Context? context, Intent? intent)
    {
        Log.Debug(LogTag, $"BootReceiver fired. Action={intent?.Action}");

        if (context is null || intent is null)
        {
            Log.Warn(LogTag, "BootReceiver aborted: context or intent null.");
            return;
        }

        try
        {
            var services = IPlatformApplication.Current?.Services;

            if (services is null)
            {
                Log.Warn(LogTag, "BootReceiver: DI services not available.");
                return;
            }

            var reminderEngine = services.GetService(typeof(IReminderEngine)) as IReminderEngine;

            if (reminderEngine is null)
            {
                Log.Warn(LogTag, "BootReceiver: Reminder engine missing.");
                return;
            }

            await reminderEngine.RestoreRemindersAsync();

            Log.Debug(LogTag, "BootReceiver: Reminder restore complete.");
        }
        catch (Exception ex)
        {
            Log.Error(LogTag, $"BootReceiver failed: {ex}");
        }
    }
}