using ADHDCompanionApp.Models;
using ADHDCompanionApp.Models.Entities;

namespace ADHDCompanionApp.Services.Interfaces;

public interface IReminderEngine
{
    Task<bool> CanScheduleExactRemindersAsync();
    Task OpenExactReminderSettingsAsync();

    Task ScheduleReminderAsync(ReminderRequest request);
    Task CancelReminderAsync(string reminderKey, int notificationId);

    Task ScheduleMedicationReminderAsync(UserProfile profile);
    Task CancelMedicationReminderAsync();
    Task ScheduleTaskReminderAsync(TaskItem task);
    Task CancelTaskReminderAsync(TaskItem task);
    Task RestoreRemindersAsync();

    Task HandleTriggeredReminderAsync(ReminderRequest request);
}