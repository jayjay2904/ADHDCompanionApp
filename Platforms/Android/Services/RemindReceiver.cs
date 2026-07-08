using Android.App;
using Android.Content;
using Android.Util;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Platforms.Android.Services;

[BroadcastReceiver(Enabled = true, Exported = true)]
public class ReminderReceiver : BroadcastReceiver
{
    private const string LogTag = "AppReminder";

    public override async void OnReceive(Context? context, Intent? intent)
    {
        try
        {
            
            Log.Debug(LogTag, "ReminderReceiver fired.");

            if (context is null || intent is null)
            {
                Log.Warn(LogTag, "ReminderReceiver exited because context or intent was null.");
                return;
            }

            var request = AndroidReminderScheduler.BuildRequestFromIntent(intent);

            if (request is null)
            {
                Log.Warn(LogTag, "ReminderReceiver could not build reminder request.");
                return;
            }

            Log.Debug(LogTag, $"Built request: Key={request.ReminderKey}, Id={request.NotificationId}, Title={request.Title}, Message={request.Message}");
            AndroidReminderScheduler.ShowNotification(context, request);
            Log.Debug(LogTag, "ShowNotification called.");

            var services = IPlatformApplication.Current?.Services;
            var reminderEngine = services?.GetService(typeof(IReminderEngine)) as IReminderEngine;

            if (reminderEngine is null)
            {
                Log.Warn(LogTag, "ReminderReceiver could not resolve reminder engine.");
                return;
            }

            await reminderEngine.HandleTriggeredReminderAsync(request);

            Log.Debug(LogTag, $"ReminderReceiver completed for '{request.ReminderKey}'.");
        }
        catch (Exception ex)
        {
            Log.Error(LogTag, $"ReminderReceiver failed: {ex}");
        }
    }
}