using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;
using Microsoft.Maui.Storage;

namespace ADHDCompanionApp.Services.Implementations;

public class UserProfileService : IUserProfileService
{
    private const string NicknameKey = "profile_nickname";
    private const string UsesMedicationSupportKey = "profile_uses_medication_support";
    private const string UsesTaskSupportKey = "profile_uses_task_support";
    private const string OnboardingCompleteKey = "profile_onboarding_complete";
    private const string ReminderHourKey = "profile_reminder_hour";
    private const string ReminderMinuteKey = "profile_reminder_minute";

    public Task<UserProfile?> GetProfileAsync()
    {
        var nickname = Preferences.Default.Get(NicknameKey, string.Empty);

        if (string.IsNullOrWhiteSpace(nickname))
        {
            return Task.FromResult<UserProfile?>(null);
        }

        var hour = Preferences.Default.Get(ReminderHourKey, -1);
        var minute = Preferences.Default.Get(ReminderMinuteKey, -1);

        TimeSpan? reminderTime = null;

        if (hour >= 0 && minute >= 0)
        {
            reminderTime = new TimeSpan(hour, minute, 0);
        }

        var profile = new UserProfile
        {
            Nickname = nickname,
            UsesMedicationSupport = Preferences.Default.Get(UsesMedicationSupportKey, false),
            UsesTaskSupport = Preferences.Default.Get(UsesTaskSupportKey, true),
            MedicationReminderTime = reminderTime
        };

        return Task.FromResult<UserProfile?>(profile);
    }

    public Task SaveProfileAsync(UserProfile profile)
    {
        Preferences.Default.Set(NicknameKey, profile.Nickname ?? string.Empty);
        Preferences.Default.Set(UsesMedicationSupportKey, profile.UsesMedicationSupport);
        Preferences.Default.Set(UsesTaskSupportKey, profile.UsesTaskSupport);
        Preferences.Default.Set(OnboardingCompleteKey, true);

        if (profile.MedicationReminderTime.HasValue)
        {
            Preferences.Default.Set(ReminderHourKey, profile.MedicationReminderTime.Value.Hours);
            Preferences.Default.Set(ReminderMinuteKey, profile.MedicationReminderTime.Value.Minutes);
        }

        return Task.CompletedTask;
    }

    public Task<bool> IsOnboardingCompleteAsync()
    {
        var isComplete = Preferences.Default.Get(OnboardingCompleteKey, false);
        return Task.FromResult(isComplete);
    }
}