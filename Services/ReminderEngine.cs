using Android.Util;
using ADHDCompanionApp.Models;
using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services;

public class ReminderEngine : IReminderEngine
{
    private const string LogTag = "ReminderEngine";

    private readonly IPlatformReminderScheduler _platformReminderScheduler;
    private readonly IUserProfileService _userProfileService;

    public ReminderEngine(
        IPlatformReminderScheduler platformReminderScheduler,
        IUserProfileService userProfileService)
    {
        _platformReminderScheduler = platformReminderScheduler;
        _userProfileService = userProfileService;
    }

    public Task<bool> CanScheduleExactRemindersAsync()
    {
        return _platformReminderScheduler.CanScheduleExactRemindersAsync();
    }

    public Task OpenExactReminderSettingsAsync()
    {
        return _platformReminderScheduler.OpenExactReminderSettingsAsync();
    }

    public Task ScheduleReminderAsync(ReminderRequest request)
    {
        return _platformReminderScheduler.ScheduleAsync(request);
    }

    public Task CancelReminderAsync(string reminderKey, int notificationId)
    {
        return _platformReminderScheduler.CancelAsync(reminderKey, notificationId);
    }

    public async Task ScheduleMedicationReminderAsync(UserProfile profile)
    {
        if (!profile.UsesMedicationSupport ||
            !profile.MedicationReminderTime.HasValue ||
            !profile.MedicationStartDate.HasValue)
        {
            Log.Debug(LogTag, "Medication reminder skipped because support is disabled or incomplete.");
            await CancelMedicationReminderAsync();
            return;
        }

        var reminderTime = profile.MedicationReminderTime.Value;
        var nextTrigger = GetNextTriggerTime(DateTime.Now, profile.MedicationStartDate.Value, reminderTime);

        var request = new ReminderRequest
        {
            ReminderKey = ReminderConstants.MedicationReminderKey,
            NotificationId = ReminderConstants.MedicationNotificationId,
            Type = ReminderType.Medication,
            Title = "Medication reminder",
            Message = string.IsNullOrWhiteSpace(profile.Nickname)
                ? "Time to take your medication."
                : $"{profile.Nickname}, time to take your medication.",
            TriggerTime = nextTrigger,
            RepeatTimeOfDay = reminderTime,
            UserName = profile.Nickname
        };

        Log.Debug(LogTag, $"Scheduling medication reminder for {request.TriggerTime:yyyy-MM-dd HH:mm:ss}");

        await CancelMedicationReminderAsync();
        await ScheduleReminderAsync(request);
    }

    public Task CancelMedicationReminderAsync()
    {
        return CancelReminderAsync(
            ReminderConstants.MedicationReminderKey,
            ReminderConstants.MedicationNotificationId);
    }

    public async Task RestoreRemindersAsync()
    {
        var profile = await _userProfileService.GetProfileAsync();

        if (profile is null)
        {
            Log.Debug(LogTag, "Restore skipped because profile is null.");
            return;
        }

        if (profile.UsesMedicationSupport &&
            profile.MedicationReminderTime.HasValue &&
            profile.MedicationStartDate.HasValue)
        {
            Log.Debug(LogTag, "Restoring medication reminder.");
            await ScheduleMedicationReminderAsync(profile);
        }
        else
        {
            Log.Debug(LogTag, "No medication reminder to restore.");
            await CancelMedicationReminderAsync();
        }
    }

    public async Task HandleTriggeredReminderAsync(ReminderRequest request)
    {
        if (!request.RepeatTimeOfDay.HasValue)
        {
            Log.Debug(LogTag, $"Reminder '{request.ReminderKey}' does not repeat.");
            return;
        }

        var nextTrigger = DateTime.Today.AddDays(1).Add(request.RepeatTimeOfDay.Value);

        var nextRequest = new ReminderRequest
        {
            ReminderKey = request.ReminderKey,
            NotificationId = request.NotificationId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            TriggerTime = nextTrigger,
            RepeatTimeOfDay = request.RepeatTimeOfDay,
            UserName = request.UserName,
            Metadata = new Dictionary<string, string>(request.Metadata)
        };

        Log.Debug(LogTag, $"Rescheduling reminder '{request.ReminderKey}' for {nextTrigger:yyyy-MM-dd HH:mm:ss}");

        await ScheduleReminderAsync(nextRequest);
    }

    private static DateTime GetNextTriggerTime(DateTime now, DateTime startDate, TimeSpan reminderTime)
    {
        var todayAtReminderTime = now.Date.Add(reminderTime);
        var startDateAtReminderTime = startDate.Date.Add(reminderTime);

        var nextTrigger = todayAtReminderTime;

        if (nextTrigger <= now)
        {
            nextTrigger = nextTrigger.AddDays(1);
        }

        if (startDateAtReminderTime > nextTrigger)
        {
            nextTrigger = startDateAtReminderTime;
        }

        return nextTrigger;
    }
}