using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services.Implementations;

public class UserProfileService : IUserProfileService
{
    private UserProfile? _profile;

    public Task<UserProfile?> GetProfileAsync()
    {
        return Task.FromResult(_profile);
    }

    public Task SaveProfileAsync(UserProfile profile)
    {
        _profile = profile;
        return Task.CompletedTask;
    }

    public Task<bool> IsOnboardingCompleteAsync()
    {
        return Task.FromResult(_profile != null);
    }
}