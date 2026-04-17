using ADHDCompanionApp.Platforms.Android.Services;
using Android.App;
using Android.Content;
using Android.OS;

namespace ADHDCompanionApp.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class MedicationReminderReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent is null)
            return;

        var notificationId = intent.GetIntExtra("notification_id", 1001);
        var title = intent.GetStringExtra("title") ?? "Medication reminder";
        var message = intent.GetStringExtra("message") ?? "Time to take your medication.";

        NotificationService.ShowNotification(context, title, message, notificationId);

        var nextIntent = new Intent(context, typeof(MedicationReminderReceiver));
        nextIntent.PutExtra("notification_id", notificationId);
        nextIntent.PutExtra("title", title);
        nextIntent.PutExtra("message", message);

        var flags = PendingIntentFlags.UpdateCurrent;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            flags |= PendingIntentFlags.Immutable;
        }

        var pendingIntent = PendingIntent.GetBroadcast(context, 2001, nextIntent, flags);

        var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager;

        if (alarmManager is null)
            return;

        var nextTrigger = DateTimeOffset.Now.AddDays(1).ToUnixTimeMilliseconds();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            alarmManager.SetAndAllowWhileIdle(AlarmType.RtcWakeup, nextTrigger, pendingIntent);
        }
        else
        {
            alarmManager.Set(AlarmType.RtcWakeup, nextTrigger, pendingIntent);
        }
    }
}