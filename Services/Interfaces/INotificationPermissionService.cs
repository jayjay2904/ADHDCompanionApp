namespace ADHDCompanionApp.Services.Interfaces;

public interface INotificationPermissionService
{
    Task<bool> RequestNotificationPermissionAsync();
    Task OpenNotificationSettingsAsync();
}