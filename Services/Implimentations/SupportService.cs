using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services.Implementations;

public class SupportService : ISupportService
{
    private readonly List<SupportOption> _options = new()
    {
        new SupportOption
        {
            Title = "I'm overwhelmed",
            ResponseText = "Pause. Do not solve everything. Take one slow breath and pick the smallest possible next step."
        },
        new SupportOption
        {
            Title = "I can't start",
            ResponseText = "Shrink the task. What is the first 2-minute version of this? Start there, not at the finish line."
        },
        new SupportOption
        {
            Title = "I'm anxious",
            ResponseText = "Your brain is sounding the alarm. That does not always mean danger. Slow your breathing and focus on one thing you can control."
        },
        new SupportOption
        {
            Title = "I feel flat",
            ResponseText = "Flat days happen. Do not demand brilliance from yourself. Aim for one gentle win, not a perfect day."
        }
    };

    public Task<List<SupportOption>> GetSupportOptionsAsync()
    {
        return Task.FromResult(_options.ToList());
    }

    public Task<SupportOption?> GetSupportOptionByTitleAsync(string title)
    {
        var option = _options.FirstOrDefault(o => o.Title == title);
        return Task.FromResult(option);
    }
}