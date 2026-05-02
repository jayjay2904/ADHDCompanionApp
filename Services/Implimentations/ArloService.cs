using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services.Implementations;

public class ArloService : IArloService
{
    private readonly ICheckInService _checkInService;
    private readonly ITaskService _taskService;
    private readonly IArloAiClient _arloAiClient;
    private readonly List<ChatMessage> _messages = new();
    private readonly Dictionary<string, int> _lastResponseIndex = new();

    private readonly Dictionary<string, List<ArloResponse>> _responses = new()
    {
        ["overwhelmed"] = new()
        {
            new ArloResponse
            {
                Validation = "Yeah… that feeling when everything piles up is a lot.",
                Action = "Let’s shrink it. Pick one small thing — not the most important, just the easiest.",
                NextStep = "If that feels okay, ignore everything else for now."
            },
            new ArloResponse
            {
                Validation = "That sounds like overload, not failure.",
                Action = "Take 30 seconds and just look at what’s in front of you.",
                NextStep = "You don’t need to solve the whole day. Just the next tiny bit."
            },
            new ArloResponse
            {
                Validation = "No wonder your brain feels full. Too many open loops can feel brutal.",
                Action = "Write down one thing that’s shouting loudest in your head.",
                NextStep = "Then we can make that one thing smaller."
            },
            new ArloResponse
            {
                Validation = "That’s that ‘everything at once’ feeling, isn’t it?",
                Action = "Let’s zoom right in. What’s one thing you could touch or move right now?",
                NextStep = "We’re just grounding first, not solving everything."
            },
            new ArloResponse
            {
                Validation = "Your brain’s trying to carry too much at once.",
                Action = "Pick one thing to ignore on purpose.",
                NextStep = "Then we’ll deal with just one thing left."
            },
            new ArloResponse
            {
                Validation = "When everything feels urgent, it’s hard to start anything.",
                Action = "Say out loud: ‘I only need to do one small thing.’",
                NextStep = "Then pick the easiest possible thing."
            },
        },

        ["cant_start"] = new()
        {
            new ArloResponse
            {
                Validation = "That stuck-at-the-start feeling is horrible.",
                Action = "Open what you need and just sit with it for 30 seconds.",
                NextStep = "If that feels okay, do the tiniest version of it."
            },
            new ArloResponse
            {
                Validation = "Starting can feel bigger than the task itself.",
                Action = "Don’t start the task. Just prepare the first thing you need.",
                NextStep = "That counts as movement."
            },
            new ArloResponse
            {
                Validation = "Your brain isn’t being lazy. It’s struggling to find the entry point.",
                Action = "Make the first step ridiculously small.",
                NextStep = "Tiny counts. Tiny is how we get moving."
            },
            new ArloResponse
            {
                Validation = "Starting is often the hardest part, not the task itself.",
                Action = "Don’t do the task yet. Just open it.",
                NextStep = "That’s enough for now."
            },
            new ArloResponse
            {
                Validation = "Your brain is waiting for a clear entry point.",
                Action = "What’s the first visible action? Not the whole task.",
                NextStep = "Just do that one bit."
            },
            new ArloResponse
            {
                Validation = "This is that ‘stuck before starting’ moment.",
                Action = "Set a 60-second timer and do anything related to it.",
                NextStep = "Stop when the timer ends. That still counts."
}
        },

        ["low_energy"] = new()
        {
            new ArloResponse
            {
                Validation = "Low energy days hit differently.",
                Action = "Pick something so small it feels almost silly.",
                NextStep = "Today isn’t about pushing. It’s about easing in."
            },
            new ArloResponse
            {
                Validation = "Your battery sounds low, so we lower the expectation.",
                Action = "Choose one gentle thing: drink water, stand up, or clear one tiny space.",
                NextStep = "That is enough for now."
            },
            new ArloResponse
            {
                Validation = "This doesn’t sound like a power-through moment.",
                Action = "Do one thing that helps Future You by 1%.",
                NextStep = "Small support still counts."
            },
            new ArloResponse
            {
                Validation = "This feels like a low battery moment, not a motivation problem.",
                Action = "Pick the smallest possible thing that helps Future You.",
                NextStep = "That’s enough for now."
            },
            new ArloResponse
            {
                Validation = "Your energy isn’t there today, and that changes how we approach things.",
                Action = "Lower the bar. What’s the easiest version of today?",
                NextStep = "Do that, and call it a win."
            },
            new ArloResponse
            {
                Validation = "Pushing through isn’t always the answer on days like this.",
                Action = "Do one gentle reset: drink water, stretch, or sit somewhere different.",
                NextStep = "Then see what feels possible."
            }
        },

        ["anxious"] = new()
        {
            new ArloResponse
            {
                Validation = "Anxiety can make everything feel urgent.",
                Action = "Take one slow breath and drop your shoulders.",
                NextStep = "Nothing needs solving right this second."
            },
            new ArloResponse
            {
                Validation = "That anxious buzz can make your brain race ahead.",
                Action = "Look around and name three things you can see.",
                NextStep = "Bring yourself back to the room first."
            },
            new ArloResponse
            {
                Validation = "That sounds uncomfortable and noisy inside your head.",
                Action = "Put both feet on the floor and take one slower breath than usual.",
                NextStep = "We can deal with the next thing after that."
            },
            new ArloResponse
            {
                Validation = "That anxious feeling can make everything feel urgent and important.",
                Action = "Slow it down. Take one breath that’s longer than usual.",
                NextStep = "Nothing needs solving right this second."
            },
            new ArloResponse
            {
                Validation = "Your brain is trying to protect you, even if it feels uncomfortable.",
                Action = "Look around and name three things you can see.",
                NextStep = "Bring yourself back to the present first."
            },
            new ArloResponse
            {
                Validation = "That buzz in your chest or head can be hard to sit with.",
                Action = "Put your feet flat on the ground and press them down gently.",
                NextStep = "We’ll take this one step at a time."
            }
        },

        ["avoiding"] = new()
        {
            new ArloResponse
            {
                Validation = "Avoiding something usually means it feels bigger than it looks.",
                Action = "Name the thing you’re avoiding without judging yourself for it.",
                NextStep = "Naming it is the first bit of control."
            },
            new ArloResponse
            {
                Validation = "Avoidance makes sense when your brain expects discomfort.",
                Action = "Just move one step closer to the thing. Don’t do it yet.",
                NextStep = "Closer is progress."
            },
            new ArloResponse
            {
                Validation = "You’re not broken for avoiding it.",
                Action = "Ask: what is the smallest safe part of this?",
                NextStep = "Start there, not with the whole thing."
            },
            new ArloResponse
            {
                Validation = "Avoiding it usually means it feels bigger than it actually is.",
                Action = "Say what you’re avoiding out loud or in your head.",
                NextStep = "Naming it gives you a bit of control back."
            },
            new ArloResponse
            {
                Validation = "Your brain is trying to dodge discomfort, not responsibility.",
                Action = "Move one step closer to the thing without doing it yet.",
                NextStep = "Closer still counts."
            },
            new ArloResponse
            {
                Validation = "This isn’t laziness. It’s your brain protecting you from something.",
                Action = "Ask: what part of this feels hardest?",
                NextStep = "Start with a smaller part of that."
            }
        },

        ["too_much"] = new()
        {
            new ArloResponse
            {
                Validation = "Too many things at once can make everything feel impossible.",
                Action = "Pick one thing to ignore on purpose for now.",
                NextStep = "Then choose one thing to look at."
            },
            new ArloResponse
            {
                Validation = "That sounds like too many tabs open in your brain.",
                Action = "Choose one task as the ‘only task’ for the next five minutes.",
                NextStep = "The rest can wait."
            },
            new ArloResponse
            {
                Validation = "When everything feels important, nothing feels possible.",
                Action = "Pick the easiest thing, not the biggest thing.",
                NextStep = "Momentum matters more than perfect priority right now."
            },
            new ArloResponse
            {
                Validation = "That feeling of ‘too much’ can freeze everything.",
                Action = "Pick one thing to ignore for now.",
                NextStep = "Then choose just one thing to focus on."
            },
            new ArloResponse
            {
                Validation = "When everything feels important, it’s hard to start anything.",
                Action = "Pick the easiest task, not the most important.",
                NextStep = "Momentum matters more right now."
            },
            new ArloResponse
            {
                Validation = "Your brain has too many tabs open at once.",
                Action = "Choose one task as the only thing that exists for 5 minutes.",
                NextStep = "The rest can wait."
            }
        },

        ["stuck"] = new()
        {
            new ArloResponse
            {
                Validation = "Feeling stuck is frustrating, especially when you know you want to move.",
                Action = "Change your position: stand up, sit somewhere else, or move to another room.",
                NextStep = "Sometimes the body has to move before the brain does."
            },
            new ArloResponse
            {
                Validation = "That stuck feeling can feel like a wall.",
                Action = "Ask yourself: what is the next visible action?",
                NextStep = "Not the whole plan. Just the next visible action."
            },
            new ArloResponse
            {
                Validation = "You’re not doing nothing. Your brain is jammed.",
                Action = "Do a reset action: water, stretch, breathe, or open the thing.",
                NextStep = "Then see what feels possible."
            },
            new ArloResponse
            {
                Validation = "That stuck feeling is frustrating when you want to move but can’t.",
                Action = "Change something physical: stand up, move, or switch rooms.",
                NextStep = "Movement can help unstick the brain."
            },
            new ArloResponse
            {
                Validation = "It feels like a wall, doesn’t it?",
                Action = "Ask: what’s the next visible step, not the whole task?",
                NextStep = "Just that one step."
            },
            new ArloResponse
            {
                Validation = "You’re not doing nothing, your brain is jammed.",
                Action = "Do a reset: water, stretch, or open the task.",
                NextStep = "Then see what feels possible."
            }
        },

        ["failed"] = new()
        {
            new ArloResponse
            {
                Validation = "That ‘I’ve failed’ feeling hits hard.",
                Action = "Write down one thing you did do today. Anything counts.",
                NextStep = "Your brain may be skipping the evidence."
            },
            new ArloResponse
            {
                Validation = "Feeling like you’ve messed up doesn’t mean you are a mess.",
                Action = "Pause and say: this is a hard moment, not my whole identity.",
                NextStep = "Then choose one kind next move."
            },
            new ArloResponse
            {
                Validation = "That shame spiral can be loud.",
                Action = "Find one tiny repair step. Message someone, tidy one thing, drink water, or reset.",
                NextStep = "Repair beats punishment."
            },
            new ArloResponse
            {
                Validation = "That ‘I’ve messed everything up’ feeling is heavy.",
                Action = "Pause. This is a moment, not a definition.",
                NextStep = "Then pick one small repair step."
            },
            new ArloResponse
            {
                Validation = "Your brain is focusing on what went wrong.",
                Action = "Write down one thing that didn’t go wrong.",
                NextStep = "There’s always something it’s skipping."
            },
            new ArloResponse
            {
                Validation = "That spiral can get loud quickly.",
                Action = "Do something kind for yourself, even if it feels small.",
                NextStep = "You don’t earn kindness by being perfect."
            }
        }
    };

    public ArloService(
        ICheckInService checkInService,
        ITaskService taskService,
        IArloAiClient arloAiClient)
    {
        _checkInService = checkInService;
        _taskService = taskService;
        _arloAiClient = arloAiClient;
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

        // AI stays here for later, but fallback is now strong enough for MVP.
        var aiRequest = new ArloAiRequest
        {
            UserMessage = userMessage,
            EmotionalContext = emotionalContext,
            OpenTasks = unfinishedTasks
                .Take(3)
                .Select(t => t.Title)
                .ToList()
        };

        //var aiReply = await _arloAiClient.GetReplyAsync(aiRequest);

        //if (!string.IsNullOrWhiteSpace(aiReply))
        //{
          //  return aiReply;
        //}

        var state = DetectState(message);

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

            return CombineParts(emotionalContext, structuredReply, gentleTaskNudge);
        }

        return GetDefaultReply(emotionalContext, unfinishedTasks);
    }

    private static string DetectState(string message)
    {
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

            return $"If it helps later, we could pick just one thing — maybe \"{nextTask}\" — and keep it really small.";
        }

        return string.Empty;
    }

    private static string GetDefaultReply(
        string emotionalContext,
        List<TaskItem> unfinishedTasks)
    {
        var response = "I’m here with you. We don’t need to fix everything right now. Tell me what feels like the hardest part.";

        var taskNudge = unfinishedTasks.Count > 0
            ? $"If you want a tiny place to start later, \"{unfinishedTasks.First().Title}\" is sitting there — but no pressure."
            : string.Empty;

        return CombineParts(emotionalContext, response, taskNudge);
    }

    private static string CombineParts(string context, string response, string taskNudge)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(context))
            parts.Add(context);

        if (!string.IsNullOrWhiteSpace(response))
            parts.Add(response);

        if (!string.IsNullOrWhiteSpace(taskNudge))
            parts.Add(taskNudge);

        return string.Join(" ", parts);
    }
}