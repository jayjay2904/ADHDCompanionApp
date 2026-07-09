using ADHDCompanionApp.Models;
using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services;

public class ReminderEngine : IReminderEngine
{
    private const string LogTag = "AppReminder";

    private static readonly int[] ReEngagementDays =
    {
        3,
        7,
        14,
        30,
        90,
        180
    };

    private readonly IPlatformReminderScheduler _platformReminderScheduler;
    private readonly IUserProfileService _userProfileService;

    public ReminderEngine(
        IPlatformReminderScheduler platformReminderScheduler,
        IUserProfileService userProfileService)
    {
        _platformReminderScheduler = platformReminderScheduler;
        _userProfileService = userProfileService;
    }

    public async Task<bool> CanScheduleExactRemindersAsync()
    {
        try
        {
            return await _platformReminderScheduler.CanScheduleExactRemindersAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CanScheduleExactRemindersAsync failed: {ex}");
            return false;
        }
    }

    public async Task OpenExactReminderSettingsAsync()
    {
        try
        {
            await _platformReminderScheduler.OpenExactReminderSettingsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OpenExactReminderSettingsAsync failed: {ex}");
        }
    }

    public async Task ScheduleReminderAsync(ReminderRequest request)
    {
        try
        {
            await _platformReminderScheduler.ScheduleAsync(request);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ScheduleReminderAsync failed for '{request.ReminderKey}': {ex}");
        }
    }

    public async Task CancelReminderAsync(string reminderKey, int notificationId)
    {
        try
        {
            await _platformReminderScheduler.CancelAsync(reminderKey, notificationId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CancelReminderAsync failed for '{reminderKey}': {ex}");
        }
    }

    public async Task ScheduleMedicationReminderAsync(UserProfile profile)
    {
        try
        {
            if (!profile.UsesMedicationSupport ||
                !profile.MedicationReminderTime.HasValue ||
                !profile.MedicationStartDate.HasValue)
            {
                await CancelMedicationReminderAsync();
                return;
            }

            var reminderTime = profile.MedicationReminderTime.Value;
            var nextTrigger = GetNextTriggerTime(
                DateTime.Now,
                profile.MedicationStartDate.Value,
                reminderTime);

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

            await CancelMedicationReminderAsync();
            await ScheduleReminderAsync(request);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ScheduleMedicationReminderAsync failed: {ex}");
        }
    }

    public async Task CancelMedicationReminderAsync()
    {
        try
        {
            await CancelReminderAsync(
                ReminderConstants.MedicationReminderKey,
                ReminderConstants.MedicationNotificationId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CancelMedicationReminderAsync failed: {ex}");
        }
    }

    public async Task RestoreRemindersAsync()
    {
        try
        {
            var profile = await _userProfileService.GetProfileAsync();

            if (profile is null)
                return;

            if (profile.UsesMedicationSupport &&
                profile.MedicationReminderTime.HasValue &&
                profile.MedicationStartDate.HasValue)
            {
                await ScheduleMedicationReminderAsync(profile);
            }
            else
            {
                await CancelMedicationReminderAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RestoreRemindersAsync failed: {ex}");
        }
    }

    public async Task HandleTriggeredReminderAsync(ReminderRequest request)
    {
        try
        {
            if (!request.RepeatTimeOfDay.HasValue)
                return;

            var nextTrigger = DateTime.Today
                .AddDays(1)
                .Add(request.RepeatTimeOfDay.Value);

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

            await ScheduleReminderAsync(nextRequest);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"HandleTriggeredReminderAsync failed for '{request.ReminderKey}': {ex}");
        }
    }

    public async Task ScheduleTaskReminderAsync(TaskItem task)
    {
        try
        {
            if (task is null)
                return;

            if (string.IsNullOrWhiteSpace(task.Id))
                return;

            if (task.IsCompleted ||
                !task.ReminderEnabled ||
                !task.ReminderDateTime.HasValue)
            {
                await CancelTaskReminderAsync(task);
                return;
            }

            var reminderKey = ReminderConstants.GetTaskReminderKey(task.Id);
            var notificationId = ReminderConstants.GetTaskNotificationId(task.Id);

            var request = new ReminderRequest
            {
                ReminderKey = reminderKey,
                NotificationId = notificationId,
                Type = ReminderType.Task,
                Title = "Task reminder",
                Message = string.IsNullOrWhiteSpace(task.Title)
                    ? "You set a reminder for a task."
                    : $"Task reminder: {task.Title}",
                TriggerTime = task.ReminderDateTime.Value,
                RepeatTimeOfDay = null
            };

            await CancelTaskReminderAsync(task);

            System.Diagnostics.Debug.WriteLine(
                $"[TaskReminder] Scheduling '{request.Message}' for {request.TriggerTime:dd/MM/yyyy HH:mm:ss}. Now: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");

            await ScheduleReminderAsync(request);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReminderEngine ERROR] ScheduleTaskReminderAsync: {ex}");
        }
    }

    public async Task CancelTaskReminderAsync(TaskItem task)
    {
        try
        {
            if (task is null || string.IsNullOrWhiteSpace(task.Id))
                return;

            await CancelReminderAsync(
                ReminderConstants.GetTaskReminderKey(task.Id),
                ReminderConstants.GetTaskNotificationId(task.Id));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReminderEngine ERROR] CancelTaskReminderAsync: {ex}");
        }
    }

    public async Task ScheduleReEngagementRemindersAsync(string? userName = null)
    {
        try
        {
            await CancelReEngagementRemindersAsync();

            var now = DateTime.Now;

            foreach (var days in ReEngagementDays)
            {
                var request = new ReminderRequest
                {
                    ReminderKey = ReminderConstants.GetReEngagementReminderKey(days),
                    NotificationId = ReminderConstants.ReEngagementNotificationIdBase + days,
                    Type = ReminderType.Custom,
                    Title = "Still here when you need me",
                    Message = GetReEngagementMessage(days, userName),
                    TriggerTime = now.AddDays(days),
                    RepeatTimeOfDay = null,
                    UserName = userName
                };

                await ScheduleReminderAsync(request);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ScheduleReEngagementRemindersAsync failed: {ex}");
        }
    }

    public async Task CancelReEngagementRemindersAsync()
    {
        try
        {
            foreach (var days in ReEngagementDays)
            {
                await CancelReminderAsync(
                    ReminderConstants.GetReEngagementReminderKey(days),
                    ReminderConstants.ReEngagementNotificationIdBase + days);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CancelReEngagementRemindersAsync failed: {ex}");
        }
    }

    private static string GetReEngagementMessage(int days, string? userName)
    {
        var namePrefix = string.IsNullOrWhiteSpace(userName)
            ? string.Empty
            : $"{userName}, ";

        return days switch
        {
            3 => $"{namePrefix}still here when you need me.",
            7 => $"{namePrefix}Want to take one small step today?",
            14 => $"{namePrefix}Arlo is here if your brain feels full.",
            30 => $"{namePrefix}just checking in. No catching up needed.",
            90 => $"{namePrefix}still here. You can start messy whenever you’re ready.",
            180 => $"{namePrefix}it’s been a while, but you don’t have to start from scratch.",
            _ => $"{namePrefix}still here when you need me."
        };
    }

    private static DateTime GetNextTriggerTime(
        DateTime now,
        DateTime startDate,
        TimeSpan reminderTime)
    {
        var todayAtReminderTime = now.Date.Add(reminderTime);
        var startDateAtReminderTime = startDate.Date.Add(reminderTime);

        var nextTrigger = todayAtReminderTime;

        if (nextTrigger <= now)
            nextTrigger = nextTrigger.AddDays(1);

        if (startDateAtReminderTime > nextTrigger)
            nextTrigger = startDateAtReminderTime;

        return nextTrigger;
    }
    public async Task ScheduleDebugNotificationAsync(string title, string message, TimeSpan delay)
    {
        var notificationId = 9001;

        var request = new ReminderRequest
        {
            ReminderKey = "debug_test_notification",
            NotificationId = notificationId,
            Type = ReminderType.Custom,
            Title = title,
            Message = message,
            TriggerTime = DateTime.Now.Add(delay)
        };

        await ScheduleReminderAsync(request);

        System.Diagnostics.Debug.WriteLine(
            $"[Arlo Notifications] Debug notification scheduled. ID={notificationId}, Time={request.TriggerTime}");
    }
}