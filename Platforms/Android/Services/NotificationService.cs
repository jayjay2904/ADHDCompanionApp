using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using ADHDCompanionApp.Services.Interfaces;
using Application = Android.App.Application;

namespace ADHDCompanionApp.Platforms.Android.Services;

public class NotificationService : INotificationService
{
    private const string ChannelId = "medication_reminders";
    private const string ChannelName = "Medication Reminders";
    private const int NotificationId = 1001;
    private const int PendingIntentRequestCode = 2001;

    public Task<bool> RequestPermissionAsync()
    {
        // Android notification permission can be requested via MAUI permissions API.
        // We keep this thin and let the caller handle the result.
        return Task.FromResult(true);
    }

    public Task ScheduleDailyMedicationReminderAsync(string userName, DateTime startDate, TimeSpan reminderTime)
    {
        CreateNotificationChannel();

        var now = DateTime.Now;
        var firstTrigger = new DateTime(
            Math.Max(startDate.Year, now.Year),
            startDate.Month,
            startDate.Day,
            reminderTime.Hours,
            reminderTime.Minutes,
            0);

        if (firstTrigger < now)
        {
            firstTrigger = now.Date.Add(reminderTime);
            if (firstTrigger < now)
            {
                firstTrigger = firstTrigger.AddDays(1);
            }
        }

        if (firstTrigger.Date < startDate.Date)
        {
            firstTrigger = startDate.Date.Add(reminderTime);
        }

        var context = Application.Context;

        var intent = new Intent(context, typeof(MedicationReminderReceiver));
        intent.PutExtra("notification_id", NotificationId);
        intent.PutExtra("title", "Medication reminder");
        intent.PutExtra("message", string.IsNullOrWhiteSpace(userName)
            ? "Time to take your medication."
            : $"{userName}, time to take your medication.");

        var pendingIntentFlags = PendingIntentFlags.UpdateCurrent;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            pendingIntentFlags |= PendingIntentFlags.Immutable;
        }

        var pendingIntent = PendingIntent.GetBroadcast(
            context,
            PendingIntentRequestCode,
            intent,
            pendingIntentFlags);

        var alarmManager = (AlarmManager?)context.GetSystemService(Context.AlarmService);

        if (alarmManager is null)
            return Task.CompletedTask;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            alarmManager.SetAndAllowWhileIdle(
                AlarmType.RtcWakeup,
                new DateTimeOffset(firstTrigger).ToUnixTimeMilliseconds(),
                pendingIntent);
        }
        else
        {
            alarmManager.Set(
                AlarmType.RtcWakeup,
                new DateTimeOffset(firstTrigger).ToUnixTimeMilliseconds(),
                pendingIntent);
        }

        return Task.CompletedTask;
    }

    public Task CancelMedicationReminderAsync()
    {
        var context = Application.Context;

        var intent = new Intent(context, typeof(MedicationReminderReceiver));

        var pendingIntentFlags = PendingIntentFlags.UpdateCurrent;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            pendingIntentFlags |= PendingIntentFlags.Immutable;
        }

        var pendingIntent = PendingIntent.GetBroadcast(
            context,
            PendingIntentRequestCode,
            intent,
            pendingIntentFlags);

        var alarmManager = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        alarmManager?.Cancel(pendingIntent);

        return Task.CompletedTask;
    }

    private static void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            return;

        var context = Application.Context;
        var notificationManager = (NotificationManager?)context.GetSystemService(Context.NotificationService);

        if (notificationManager is null)
            return;

        var channel = new NotificationChannel(ChannelId, ChannelName, NotificationImportance.Default)
        {
            Description = "Daily medication reminders"
        };

        notificationManager.CreateNotificationChannel(channel);
    }

    public static void ShowNotification(Context context, string title, string message, int notificationId)
    {
        CreateNotificationChannel();

        var launchIntent = context.PackageManager?.GetLaunchIntentForPackage(context.PackageName);
        PendingIntent? contentIntent = null;

        if (launchIntent is not null)
        {
            var contentFlags = PendingIntentFlags.UpdateCurrent;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                contentFlags |= PendingIntentFlags.Immutable;
            }

            contentIntent = PendingIntent.GetActivity(context, 0, launchIntent, contentFlags);
        }

        var builder = new NotificationCompat.Builder(context, ChannelId)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetAutoCancel(true)
            .SetPriority((int)NotificationPriority.Default);

        if (contentIntent is not null)
        {
            builder.SetContentIntent(contentIntent);
        }

        NotificationManagerCompat.From(context).Notify(notificationId, builder.Build());
    }
}