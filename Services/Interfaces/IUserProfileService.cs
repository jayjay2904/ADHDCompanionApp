using ADHDCompanionApp.Models.Entities;

namespace ADHDCompanionApp.Services.Interfaces;

public interface IUserProfileService
{
    Task<UserProfile?> GetProfileAsync();
    Task SaveProfileAsync(UserProfile profile);
    Task<bool> IsOnboardingCompleteAsync();
}