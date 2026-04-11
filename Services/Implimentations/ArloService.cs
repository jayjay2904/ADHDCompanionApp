using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services.Implementations;

public class ArloService : IArloService
{
    private readonly ICheckInService _checkInService;
    private readonly ITaskService _taskService;
    private readonly List<ChatMessage> _messages = new();

    public ArloService(
        ICheckInService checkInService,
        ITaskService taskService)
    {
        _checkInService = checkInService;
        _taskService = taskService;
    }

    public Task<List<ChatMessage>> GetMessagesAsync()
    {
        return Task.FromResult(_messages.ToList());
    }

    public Task AddMessageAsync(ChatMessage message)
    {
        _messages.Add(message);
        return Task.CompletedTask;
    }

    public Task ClearMessagesAsync()
    {
        _messages.Clear();
        return Task.CompletedTask;
    }

    public async Task<string> GetReplyAsync(string userMessage)
    {
        var message = userMessage.Trim().ToLowerInvariant();

        var latestCheckIn = await _checkInService.GetLatestCheckInAsync();
        var allTasks = await _taskService.GetAllTasksAsync();

        var unfinishedTasks = allTasks
            .Where(t => !t.IsCompleted)
            .OrderBy(t => t.CreatedUtc)
            .ToList();

        var emotionalContext = BuildEmotionalContext(latestCheckIn);

        string response;

        if (message.Contains("overwhelmed"))
        {
            response = "That makes sense. When everything stacks up, it can feel like too much all at once. Let’s not try to solve all of it. Just shrink the moment.";
        }
        else if (message.Contains("stuck") || message.Contains("can't start") || message.Contains("cant start"))
        {
            response = "That stuck feeling is real, especially when your brain is trying to do too much at once. Let’s make the first step so small it almost feels pointless.";
        }
        else if (message.Contains("anxious"))
        {
            response = "Anxiety has a way of making everything feel urgent. It doesn’t mean it actually is. Let’s slow it down a bit first.";
        }
        else if (message.Contains("tired") || message.Contains("low energy"))
        {
            response = "Low energy days are different. The goal isn’t to push through — it’s to adjust the expectation and be a bit kinder to yourself.";
        }
        else
        {
            response = "I’m here with you. We don’t need to fix everything right now. Just tell me what feels like the hardest part.";
        }

        var gentleTaskNudge = BuildGentleTaskNudge(unfinishedTasks, message);

        return CombineParts(emotionalContext, response, gentleTaskNudge);
    }

    private static string BuildEmotionalContext(CheckInEntry? checkIn)
    {
        if (checkIn is null)
            return string.Empty;

        var mood = checkIn.Mood?.Trim();

        if (checkIn.EnergyLevel <= 2 && checkIn.FocusLevel <= 2)
            return "Looks like today’s a low-energy, low-focus day.";

        if (checkIn.EnergyLevel <= 2)
            return "Looks like your energy is pretty low today.";

        if (checkIn.FocusLevel <= 2)
            return "Focus looks a bit tough today.";

        if (!string.IsNullOrWhiteSpace(mood) &&
            mood.Equals("anxious", StringComparison.OrdinalIgnoreCase))
            return "You mentioned feeling anxious today.";

        if (!string.IsNullOrWhiteSpace(mood) &&
            mood.Equals("overwhelmed", StringComparison.OrdinalIgnoreCase))
            return "You mentioned feeling overwhelmed today.";

        if (checkIn.EnergyLevel >= 4 && checkIn.FocusLevel >= 4)
            return "You’ve actually got a decent bit of energy and focus right now.";

        return string.Empty;
    }

    private static string BuildGentleTaskNudge(List<TaskItem> tasks, string message)
    {
        if (tasks.Count == 0)
            return string.Empty;

        if (message.Contains("overwhelmed") || message.Contains("stuck") || message.Contains("can't start"))
        {
            var nextTask = tasks.First().Title;
            return $"If it helps later, we could pick just one thing — maybe \"{nextTask}\" — and keep it really small.";
        }

        return string.Empty;
    }

    private static string CombineParts(string context, string response, string taskNudge)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(context))
            parts.Add(context);

        parts.Add(response);

        if (!string.IsNullOrWhiteSpace(taskNudge))
            parts.Add(taskNudge);

        return string.Join(" ", parts);
    }
}