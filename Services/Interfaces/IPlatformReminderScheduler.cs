using ADHDCompanionApp.Models;

namespace ADHDCompanionApp.Services.Interfaces;

public interface IPlatformReminderScheduler
{
    Task<bool> CanScheduleExactRemindersAsync();
    Task OpenExactReminderSettingsAsync();

    Task ScheduleAsync(ReminderRequest request);
    Task CancelAsync(string reminderKey, int notificationId);
}