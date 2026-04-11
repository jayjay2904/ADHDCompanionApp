using ADHDCompanionApp.Models.Entities;

namespace ADHDCompanionApp.Services.Interfaces;

public interface ISupportService
{
    Task<List<SupportOption>> GetSupportOptionsAsync();
    Task<SupportOption?> GetSupportOptionByTitleAsync(string title);
}