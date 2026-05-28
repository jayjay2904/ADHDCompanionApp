using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;
using System.Diagnostics;

namespace ADHDCompanionApp.Services.Implementations;

public class ArloService : IArloService
{
    private readonly ICheckInService _checkInService;
    private readonly ITaskService _taskService;
    private readonly IArloAiClient _arloAiClient;
    private readonly IWinService _winService;

    private readonly List<ChatMessage> _messages = new();
    private readonly Dictionary<string, int> _lastResponseIndex = new();
    private readonly Queue<string> _recentModes = new();

    private DateTime? _lastAutoCheckInUtc;
    private DateTime? _lastAutoWinUtc;
    private string? _pendingReminderText;

    private readonly Dictionary<string, List<ArloResponse>> _responses = new()
    {
        ["overwhelmed"] = new()
        {
            new ArloResponse { Validation = "Yeah… that feeling when everything piles up is a lot.", Action = "Let’s shrink it. Pick one small thing — not the most important, just the easiest.", NextStep = "If that feels okay, ignore everything else for now." },
            new ArloResponse { Validation = "That sounds like overload, not failure.", Action = "Take 30 seconds and just look at what’s in front of you.", NextStep = "You don’t need to solve the whole day. Just the next small bit." },
            new ArloResponse { Validation = "No wonder your brain feels full. Too many open loops can feel brutal.", Action = "Write down one thing that’s shouting loudest in your head.", NextStep = "Then we can make that one thing smaller." }
        },

        ["cant_start"] = new()
        {
            new ArloResponse { Validation = "That stuck-at-the-start feeling is horrible.", Action = "Open what you need and just sit with it for 30 seconds.", NextStep = "If that feels okay, do the tiniest version of it." },
            new ArloResponse { Validation = "Starting can feel bigger than the task itself.", Action = "Don’t start the task. Just prepare the first thing you need.", NextStep = "That counts as movement." },
            new ArloResponse { Validation = "Your brain isn’t being lazy. It’s struggling to find the entry point.", Action = "Make the first step ridiculously small.", NextStep = "small counts. small is how we get moving." }
        },

        ["low_energy"] = new()
        {
            new ArloResponse { Validation = "Low energy days hit differently.", Action = "Pick something so small it feels almost silly.", NextStep = "Today isn’t about pushing. It’s about easing in." },
            new ArloResponse { Validation = "Your battery sounds low, so we lower the expectation.", Action = "Choose one gentle thing: drink water, stand up, or clear one small space.", NextStep = "That is enough for now." },
            new ArloResponse { Validation = "This doesn’t sound like a power-through moment.", Action = "Do one thing that helps Future You by 1%.", NextStep = "Small support still counts." }
        },

        ["anxious"] = new()
        {
            new ArloResponse { Validation = "Anxiety can make everything feel urgent.", Action = "Take one slow breath and drop your shoulders.", NextStep = "Nothing needs solving right this second." },
            new ArloResponse { Validation = "That anxious buzz can make your brain race ahead.", Action = "Look around and name three things you can see.", NextStep = "Bring yourself back to the room first." },
            new ArloResponse { Validation = "That sounds uncomfortable and noisy inside your head.", Action = "Put both feet on the floor and take one slower breath than usual.", NextStep = "We can deal with the next thing after that." }
        },

        ["avoiding"] = new()
        {
            new ArloResponse { Validation = "Avoiding something usually means it feels bigger than it looks.", Action = "Name the thing you’re avoiding without judging yourself for it.", NextStep = "Naming it is the first bit of control." },
            new ArloResponse { Validation = "Avoidance makes sense when your brain expects discomfort.", Action = "Just move one step closer to the thing. Don’t do it yet.", NextStep = "Closer is progress." },
            new ArloResponse { Validation = "You’re not broken for avoiding it.", Action = "Ask: what is the smallest safe part of this?", NextStep = "Start there, not with the whole thing." }
        },

        ["too_much"] = new()
        {
            new ArloResponse { Validation = "Too many things at once can make everything feel impossible.", Action = "Pick one thing to ignore on purpose for now.", NextStep = "Then choose one thing to look at." },
            new ArloResponse { Validation = "That sounds like too many tabs open in your brain.", Action = "Choose one task as the only task for the next five minutes.", NextStep = "The rest can wait." },
            new ArloResponse { Validation = "When everything feels important, nothing feels possible.", Action = "Pick the easiest thing, not the biggest thing.", NextStep = "Momentum matters more than perfect priority right now." }
        },

        ["stuck"] = new()
        {
            new ArloResponse { Validation = "Feeling stuck is frustrating, especially when you know you want to move.", Action = "Change your position: stand up, sit somewhere else, or move to another room.", NextStep = "Sometimes the body has to move before the brain does." },
            new ArloResponse { Validation = "That stuck feeling can feel like a wall.", Action = "Ask yourself: what is the next visible action?", NextStep = "Not the whole plan. Just the next visible action." },
            new ArloResponse { Validation = "You’re not doing nothing. Your brain is jammed.", Action = "Do a reset action: water, stretch, breathe, or open the thing.", NextStep = "Then see what feels possible." }
        },

        ["failed"] = new()
        {
            new ArloResponse { Validation = "That ‘I’ve failed’ feeling hits hard.", Action = "Write down one thing you did do today. Anything counts.", NextStep = "Your brain may be skipping the evidence." },
            new ArloResponse { Validation = "Feeling like you’ve messed up doesn’t mean you are a mess.", Action = "Pause and say: this is a hard moment, not my whole identity.", NextStep = "Then choose one kind next move." },
            new ArloResponse { Validation = "That shame spiral can be loud.", Action = "Find one small repair step. Message someone, tidy one thing, drink water, or reset.", NextStep = "Repair beats punishment." }
        },

        ["overstimulated"] = new()
        {
            new ArloResponse { Validation = "That sounds like too much input all at once.", Action = "Reduce one thing: light, sound, movement, or people.", NextStep = "You don’t need to explain it. Just lower the noise first." },
            new ArloResponse { Validation = "Your system sounds overloaded, not broken.", Action = "Step away from one source of stimulation for two minutes.", NextStep = "Quiet first. Decisions later." },
            new ArloResponse { Validation = "When everything feels loud, even small things can feel massive.", Action = "Find one calmer spot, even if it’s just the bathroom or the car.", NextStep = "Let your nervous system come down before doing anything else." }
        },

        ["okay"] = new()
        {
            new ArloResponse { Validation = "Nice. We don’t need to force anything big.", Action = "Pick one small thing that would make today slightly easier.", NextStep = "Small steady steps count." },
            new ArloResponse { Validation = "Okay is a good place to start from.", Action = "Choose one thing you can move forward gently.", NextStep = "No need to sprint." },
            new ArloResponse { Validation = "Good. Let’s keep it light and useful.", Action = "Do one small thing before your brain changes the channel.", NextStep = "small momentum is still momentum." }
        }
    };

    public ArloService(
       ICheckInService checkInService,
       ITaskService taskService,
       IArloAiClient arloAiClient,
       IWinService winService)
    {
        _checkInService = checkInService;
        _taskService = taskService;
        _arloAiClient = arloAiClient;
        _winService = winService;
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

        var state = DetectState(message);
        TrackRecentMode(state);

        await TrySaveAutoCheckInAsync(state, userMessage);
        await TrySaveAutoWinAsync(userMessage);

        var reminderSuggestion = string.Empty;

        if (LooksLikeReminderIntent(userMessage))
        {
            _pendingReminderText = CleanReminderText(userMessage);
            
        }

        var aiRequest = new ArloAiRequest
        {
            UserMessage = userMessage,
            EmotionalContext = emotionalContext,
            RecentChat = _messages
                .TakeLast(3)
                .Select(m => $"{(m.IsFromUser ? "U" : "A")}: {m.Text}")
                .ToList(),
            OpenTasks = unfinishedTasks
                .Take(3)
                .Select(t => t.Title)
                .ToList(),
            RecentModes = _recentModes.ToList()
        };

        var aiReply = await _arloAiClient.GetReplyAsync(aiRequest);

        if (!string.IsNullOrWhiteSpace(aiReply))
        {
            return aiReply + reminderSuggestion;
        }

        if (_responses.TryGetValue(state, out var stateResponses))
        {
            var random = Random.Shared.Next(stateResponses.Count);

            if (_lastResponseIndex.TryGetValue(state, out var lastIndex) && stateResponses.Count > 1)
            {
                while (random == lastIndex)
                {
                    random = Random.Shared.Next(stateResponses.Count);
                }
            }

            _lastResponseIndex[state] = random;

            var selectedResponse = stateResponses[random];

            var structuredReply =
                $"{selectedResponse.Validation} {selectedResponse.Action} {selectedResponse.NextStep}";

            var gentleTaskNudge = BuildGentleTaskNudge(unfinishedTasks, state);

            return CombineParts(emotionalContext, structuredReply, gentleTaskNudge) + reminderSuggestion;
        }

        return GetDefaultReply(emotionalContext, unfinishedTasks) + reminderSuggestion;
    }

    private async Task TrySaveAutoWinAsync(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return;

        var message = userMessage.Trim();

        if (!LooksLikeAWin(message))
            return;

        var now = DateTime.UtcNow;

        if (_lastAutoWinUtc.HasValue &&
            now - _lastAutoWinUtc.Value < TimeSpan.FromMinutes(10))
        {
            return;
        }

        var win = new WinEntry
        {
            Text = CleanWinText(message),
            TimestampUtc = now
        };

        await _winService.AddWinAsync(win);
        _lastAutoWinUtc = now;

        Debug.WriteLine($"WIN SAVED: {win.Text}");

    }

    private static bool LooksLikeAWin(string message)
    {
        var text = message
            .ToLowerInvariant()
            .Replace("'", "")
            .Replace("’", "");

        return text.Contains("i did")
            || text.Contains("i managed")
            || text.Contains("i finally")
            || text.Contains("i completed")
            || text.Contains("i finished")
            || text.Contains("i sent")
            || text.Contains("i cleaned")
            || text.Contains("i made it")
            || text.Contains("i showed up")
            || text.Contains("i went");
    }

    private static string CleanWinText(string message)
    {
        var cleaned = message.Trim();

        if (cleaned.Length > 160)
        {
            cleaned = cleaned[..160].TrimEnd() + "...";
        }

        return cleaned;
    }
    private void TrackRecentMode(string mode)
    {
        if (string.IsNullOrWhiteSpace(mode) || mode == "default")
            return;

        _recentModes.Enqueue(mode);

        while (_recentModes.Count > 3)
        {
            _recentModes.Dequeue();
        }
    }
    private static string DetectState(string message)
    {
        if (message.Contains("overstimulated") ||
            message.Contains("over stimulated") ||
            message.Contains("too loud") ||
            message.Contains("sensory") ||
            message.Contains("noise") ||
            message.Contains("everything is too much"))
            return "overstimulated";

        if (message.Contains("overwhelmed"))
            return "overwhelmed";

        if (message.Contains("can't start") ||
            message.Contains("cant start") ||
            message.Contains("cannot start") ||
            message.Contains("can't get started") ||
            message.Contains("cant get started"))
            return "cant_start";

        if (message.Contains("tired") ||
            message.Contains("low energy") ||
            message.Contains("no energy") ||
            message.Contains("exhausted") ||
            message.Contains("drained"))
            return "low_energy";

        if (message.Contains("anxious") ||
            message.Contains("anxiety") ||
            message.Contains("panicky") ||
            message.Contains("panic"))
            return "anxious";

        if (message.Contains("avoiding") ||
            message.Contains("avoid") ||
            message.Contains("putting off") ||
            message.Contains("procrastinating"))
            return "avoiding";

        if (message.Contains("too much") ||
            message.Contains("too many") ||
            message.Contains("everything to do") ||
            message.Contains("loads to do"))
            return "too_much";

        if (message.Contains("stuck") ||
            message.Contains("frozen") ||
            message.Contains("can't move") ||
            message.Contains("cant move"))
            return "stuck";

        if (message.Contains("failed") ||
            message.Contains("failure") ||
            message.Contains("messed up") ||
            message.Contains("useless") ||
            message.Contains("not good enough"))
            return "failed";

        if (message.Contains("okay") ||
            message.Contains("fine") ||
            message.Contains("alright") ||
            message.Contains("all right"))
            return "okay";

        return "default";
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

    private static string BuildGentleTaskNudge(List<TaskItem> tasks, string state)
    {
        if (tasks.Count == 0)
            return string.Empty;

        if (state is "overwhelmed" or "cant_start" or "stuck" or "too_much" or "avoiding")
        {
            var nextTask = tasks.First().Title;
            return $"Small option later: \"{nextTask}\". No pressure.";
        }

        return string.Empty;
    }

    private static string GetDefaultReply(
        string emotionalContext,
        List<TaskItem> unfinishedTasks)
    {
        var response = "I’m here with you. We don’t need to fix everything right now. Tell me what feels like the hardest part.";

        var taskNudge = unfinishedTasks.Count > 0
            ? $"Small option later: \"{unfinishedTasks.First().Title}\". No pressure."
            : string.Empty;

        return CombineParts(emotionalContext, response, taskNudge);
    }

    private static string CombineParts(string context, string response, string taskNudge)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(context))
            parts.Add(context.Trim());

        if (!string.IsNullOrWhiteSpace(response))
            parts.Add(response.Trim());

        if (!string.IsNullOrWhiteSpace(taskNudge))
            parts.Add(taskNudge.Trim());

        var combined = string.Join(" ", parts);

        if (combined.Length > 420)
        {
            combined = combined[..420].TrimEnd() + "...";
        }

        return combined;
    }
    private async Task TrySaveAutoCheckInAsync(string state, string userMessage)
    {
        if (string.IsNullOrWhiteSpace(state) || state == "default")
            return;

        var now = DateTime.UtcNow;

        // Only auto-save one check-in every 4 hours for now
        if (_lastAutoCheckInUtc.HasValue &&
            now - _lastAutoCheckInUtc.Value < TimeSpan.FromHours(4))
        {
            return;
        }

        var checkIn = new CheckInEntry
        {
            Mood = state,
            Note = userMessage,
            TimestampUtc = now,
            EnergyLevel = EstimateEnergyLevel(state),
            FocusLevel = EstimateFocusLevel(state)
        };

        await _checkInService.SaveCheckInAsync(checkIn);
        Debug.WriteLine($"CHECK-IN SAVED: {checkIn.Mood}");
        _lastAutoCheckInUtc = now;
    }
    private static int EstimateEnergyLevel(string state)
    {
        return state switch
        {
            "low_energy" => 1,
            "overwhelmed" => 2,
            "too_much" => 2,
            "stuck" => 2,
            "failed" => 2,
            "anxious" => 3,
            "okay" => 4,
            _ => 3
        };
    }

    private static int EstimateFocusLevel(string state)
    {
        return state switch
        {
            "overwhelmed" => 1,
            "too_much" => 1,
            "stuck" => 2,
            "cant_start" => 2,
            "low_energy" => 2,
            "anxious" => 2,
            "okay" => 4,
            _ => 3
        };
    }
    private static bool LooksLikeReminderIntent(string message)
    {
        var text = message
            .ToLowerInvariant()
            .Replace("'", "")
            .Replace("’", "");

        return text.Contains("i need to")
            || text.Contains("i have to")
            || text.Contains("i should")
            || text.Contains("remind me")
            || text.Contains("dont let me forget")
            || text.Contains("remember to");
    }

    private static string CleanReminderText(string message)
    {
        var cleaned = message.Trim();

        cleaned = cleaned
            .Replace("I need to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("I have to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("I should", "", StringComparison.OrdinalIgnoreCase)
            .Replace("remind me to", "", StringComparison.OrdinalIgnoreCase)
            .Replace("remember to", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        if (string.IsNullOrWhiteSpace(cleaned))
            return message.Trim();

        return char.ToUpper(cleaned[0]) + cleaned[1..];
    }
}