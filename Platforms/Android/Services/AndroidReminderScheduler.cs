using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Util;
using AndroidX.Core.App;
using ADHDCompanionApp.Models;
using ADHDCompanionApp.Services.Interfaces;
using Application = Android.App.Application;

namespace ADHDCompanionApp.Platforms.Android.Services;

public class AndroidReminderScheduler : IPlatformReminderScheduler
{
    private const string ChannelId = "reminders";
    private const string ChannelName = "Reminders";
    private const string ReminderAction = "ADHDCOMPANIONAPP_REMINDER";
    private const string LogTag = "AppReminder";

    public Task<bool> CanScheduleExactRemindersAsync()
    {
        try
        {
            var context = Application.Context;
            var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager;

            if (alarmManager is null)
                return Task.FromResult(false);

            if (Build.VERSION.SdkInt < BuildVersionCodes.S)
                return Task.FromResult(true);

            return Task.FromResult(alarmManager.CanScheduleExactAlarms());
        }
        catch (Exception ex)
        {
            Log.Error(LogTag, $"CanScheduleExactRemindersAsync failed: {ex}");
            return Task.FromResult(false);
        }
    }

    public Task OpenExactReminderSettingsAsync()
    {
        try
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.S)
                return Task.CompletedTask;

            var context = Application.Context;

            var intent = new Intent(Settings.ActionRequestScheduleExactAlarm);
            intent.SetData(global::Android.Net.Uri.Parse($"package:{context.PackageName}"));
            intent.SetFlags(ActivityFlags.NewTask);

            context.StartActivity(intent);

            Log.Debug(LogTag, "Opened exact reminder settings.");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Log.Error(LogTag, $"OpenExactReminderSettingsAsync failed: {ex}");
            return Task.CompletedTask;
        }
    }

    public async Task ScheduleAsync(ReminderRequest request)
    {
        try
        {
            CreateNotificationChannel();

            var context = Application.Context;
            var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager;

            if (alarmManager is null)
            {
                Log.Warn(LogTag, "AlarmManager unavailable.");
                return;
            }

            var pendingIntent = CreateReminderPendingIntent(context, request);

            // Treat reminder time as local phone time
            var localTriggerTime = DateTime.SpecifyKind(request.TriggerTime, DateTimeKind.Local);
            var triggerMillis = new DateTimeOffset(localTriggerTime).ToUnixTimeMilliseconds();

            var canScheduleExact = await CanScheduleExactRemindersInternalAsync(alarmManager);

            Log.Debug(LogTag, $"Scheduling reminder '{request.ReminderKey}'");
            Log.Debug(LogTag, $"Trigger time: {localTriggerTime:yyyy-MM-dd HH:mm:ss}");
            Log.Debug(LogTag, $"Now: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Log.Debug(LogTag, $"Trigger millis: {triggerMillis}");
            Log.Debug(LogTag, $"Can schedule exact alarms: {canScheduleExact}");

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                if (canScheduleExact)
                {
                    alarmManager.SetExactAndAllowWhileIdle(
                        AlarmType.RtcWakeup,
                        triggerMillis,
                        pendingIntent);

                    Log.Debug(LogTag, "Exact alarm scheduled.");
                }
                else
                {
                    alarmManager.SetAndAllowWhileIdle(
                        AlarmType.RtcWakeup,
                        triggerMillis,
                        pendingIntent);

                    Log.Debug(LogTag, "Exact alarm unavailable. Fallback alarm scheduled.");
                }
            }
            else
            {
                alarmManager.Set(
                    AlarmType.RtcWakeup,
                    triggerMillis,
                    pendingIntent);

                Log.Debug(LogTag, "Legacy alarm scheduled.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(LogTag, $"ScheduleAsync failed: {ex}");
        }
    }

    public Task CancelAsync(string reminderKey, int notificationId)
    {
        try
        {
            var context = Application.Context;
            var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager;

            if (alarmManager is null)
            {
                Log.Warn(LogTag, "AlarmManager unavailable. Nothing to cancel.");
                return Task.CompletedTask;
            }

            var intent = new Intent(context, typeof(ReminderReceiver));
            intent.SetAction(ReminderAction);
            intent.PutExtra("reminder_key", reminderKey);
            intent.PutExtra("notification_id", notificationId);

            var flags = PendingIntentFlags.UpdateCurrent;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                flags |= PendingIntentFlags.Immutable;
            }

            var pendingIntent = PendingIntent.GetBroadcast(
                context,
                notificationId,
                intent,
                flags);

            alarmManager.Cancel(pendingIntent);
            pendingIntent.Cancel();

            Log.Debug(LogTag, $"Alarm cancelled for '{reminderKey}'.");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Log.Error(LogTag, $"CancelAsync failed: {ex}");
            return Task.CompletedTask;
        }
    }

    public static void ShowNotification(Context context, ReminderRequest request)
    {
        try
        {
            CreateNotificationChannel();

            PendingIntent? contentIntent = null;
            var launchIntent = context.PackageManager?.GetLaunchIntentForPackage(context.PackageName);

            if (launchIntent is not null)
            {
                var flags = PendingIntentFlags.UpdateCurrent;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    flags |= PendingIntentFlags.Immutable;
                }

                contentIntent = PendingIntent.GetActivity(context, 0, launchIntent, flags);
            }

            var builder = new NotificationCompat.Builder(context, ChannelId)
                .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
                .SetContentTitle(request.Title)
                .SetContentText(request.Message)
                .SetAutoCancel(true)
                .SetPriority((int)NotificationPriority.High);

            if (contentIntent is not null)
            {
                builder.SetContentIntent(contentIntent);
            }

            NotificationManagerCompat.From(context).Notify(request.NotificationId, builder.Build());

            Log.Debug(LogTag, $"Notification displayed. Id={request.NotificationId}, Title={request.Title}");
        }
        catch (Exception ex)
        {
            Log.Error(LogTag, $"ShowNotification failed: {ex}");
        }
    }

    public static ReminderRequest? BuildRequestFromIntent(Intent intent)
    {
        try
        {
            var reminderKey = intent.GetStringExtra("reminder_key") ?? string.Empty;
            var title = intent.GetStringExtra("title") ?? "Reminder";
            var message = intent.GetStringExtra("message") ?? "You have a reminder.";
            var notificationId = intent.GetIntExtra("notification_id", 1001);
            var userName = intent.GetStringExtra("user_name") ?? string.Empty;
            var repeatTicks = intent.GetLongExtra("repeat_time_ticks", -1);
            var typeValue = intent.GetIntExtra("reminder_type", (int)ReminderType.Custom);

            return new ReminderRequest
            {
                ReminderKey = reminderKey,
                NotificationId = notificationId,
                Type = Enum.IsDefined(typeof(ReminderType), typeValue)
                    ? (ReminderType)typeValue
                    : ReminderType.Custom,
                Title = title,
                Message = message,
                UserName = userName,
                RepeatTimeOfDay = repeatTicks >= 0 ? new TimeSpan(repeatTicks) : null
            };
        }
        catch (Exception ex)
        {
            Log.Error(LogTag, $"BuildRequestFromIntent failed: {ex}");
            return null;
        }
    }

    private static PendingIntent CreateReminderPendingIntent(Context context, ReminderRequest request)
    {
        var intent = new Intent(context, typeof(ReminderReceiver));
        intent.SetAction(ReminderAction);
        intent.PutExtra("reminder_key", request.ReminderKey);
        intent.PutExtra("notification_id", request.NotificationId);
        intent.PutExtra("reminder_type", (int)request.Type);
        intent.PutExtra("title", request.Title);
        intent.PutExtra("message", request.Message);
        intent.PutExtra("user_name", request.UserName ?? string.Empty);
        intent.PutExtra("repeat_time_ticks", request.RepeatTimeOfDay?.Ticks ?? -1L);

        var flags = PendingIntentFlags.UpdateCurrent;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            flags |= PendingIntentFlags.Immutable;
        }

        return PendingIntent.GetBroadcast(
            context,
            request.NotificationId,
            intent,
            flags);
    }

    private static Task<bool> CanScheduleExactRemindersInternalAsync(AlarmManager alarmManager)
    {
        try
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.S)
                return Task.FromResult(true);

            return Task.FromResult(alarmManager.CanScheduleExactAlarms());
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private static void CreateNotificationChannel()
    {
        try
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            var context = Application.Context;
            var notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;

            if (notificationManager is null)
                return;

            var channel = new NotificationChannel(ChannelId, ChannelName, NotificationImportance.High)
            {
                Description = "App reminders"
            };

            notificationManager.CreateNotificationChannel(channel);
        }
        catch (Exception ex)
        {
            Log.Error(LogTag, $"CreateNotificationChannel failed: {ex}");
        }
    }
}