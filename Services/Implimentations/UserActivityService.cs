using ADHDCompanionApp.Services.Interfaces;
using Microsoft.Maui.Storage;

namespace ADHDCompanionApp.Services.Implementations;

public class UserActivityService : IUserActivityService
{
    private readonly IReminderEngine _reminderEngine;
    private readonly IUserProfileService _userProfileService;

    public UserActivityService(
        IReminderEngine reminderEngine,
        IUserProfileService userProfileService)
    {
        _reminderEngine = reminderEngine;
        _userProfileService = userProfileService;
    }

    public async Task RecordInteractionAsync()
    {
        try
        {
            Preferences.Set("LastArloInteractionUtc", DateTime.UtcNow.ToString("O"));

            var profile = await _userProfileService.GetProfileAsync();

            await _reminderEngine.ScheduleReEngagementRemindersAsync(profile?.Nickname);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UserActivity] RecordInteractionAsync failed: {ex}");
        }
    }
}