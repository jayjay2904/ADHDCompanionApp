namespace ADHDCompanionApp.Services.Interfaces;

public interface INotificationService
{
    Task<bool> RequestPermissionAsync();
    Task ScheduleDailyMedicationReminderAsync(string userName, DateTime startDate, TimeSpan reminderTime);
    Task CancelMedicationReminderAsync();
}