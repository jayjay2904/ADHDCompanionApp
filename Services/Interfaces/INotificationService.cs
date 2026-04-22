using ADHDCompanionApp.Models.Entities;

namespace ADHDCompanionApp.Services.Interfaces;

public interface INotificationService
{
    Task<bool> RequestPermissionAsync();
    Task<bool> CanScheduleExactAlarmsAsync();
    Task OpenExactAlarmSettingsAsync();
    Task ScheduleDailyMedicationReminderAsync(string userName, DateTime startDate, TimeSpan reminderTime);
    Task CancelMedicationReminderAsync();
    Task RestoreMedicationReminderAsync(UserProfile? profile);
}