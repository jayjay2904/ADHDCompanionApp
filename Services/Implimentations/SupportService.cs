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
            ValidationText = "That’s okay. You do not need to solve everything right now.",
            ImmediateActionText = "Put both feet on the floor and take one slow breath.",
            NextStepText = "Pick one tiny action: drink some water, sit up properly, or put one thing away.",
            AlternateImmediateActionText = "Look around and name 3 things you can see.",
            AlternateNextStepText = "Choose one thing you can ignore for the next 10 minutes."
        },
        new SupportOption
        {
            Title = "I can't start",
            ValidationText = "You do not need to finish it. You just need to begin smaller.",
            ImmediateActionText = "Open the task. Do not do it yet. Just open it.",
            NextStepText = "Do the 2-minute version only.",
            AlternateImmediateActionText = "Set a timer for 2 minutes and give yourself permission to stop after that.",
            AlternateNextStepText = "Write the first messy line, not the perfect one."
        },
        new SupportOption
        {
            Title = "I'm spiralling",
            ValidationText = "Your brain is loud right now. That does not mean everything is urgent.",
            ImmediateActionText = "Look around and name 3 things you can see.",
            NextStepText = "Choose one thing you can control in the next 5 minutes.",
            AlternateImmediateActionText = "Unclench your jaw and drop your shoulders.",
            AlternateNextStepText = "Move away from the thing that is winding you up for 2 minutes."
        },
        new SupportOption
        {
            Title = "I feel flat",
            ValidationText = "Flat days happen. You are allowed to keep this gentle.",
            ImmediateActionText = "Roll your shoulders, unclench your jaw, and take one breath.",
            NextStepText = "Aim for one gentle win, not a perfect day.",
            AlternateImmediateActionText = "Stand up and stretch for 10 seconds.",
            AlternateNextStepText = "Do one low-effort task that makes life slightly easier later."
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