using ADHDCompanionApp.Api.Controllers;
using Microsoft.Extensions.Options;

namespace ADHDCompanionApp.Api.Services;

public class ArloPromptBuilder
{
    private readonly ArloPromptOptions _options;

    public string Build(ArloChatRequest request, string responseMode)
    {
        var recentModes = string.Join(", ", request.RecentModes.Take(3));
        var openTasks = string.Join(", ", request.OpenTasks.Take(3));
        var modeGuidance = GetModeGuidance(responseMode);
        var recentChat = string.Join("\n", request.RecentChat.Take(4));

        var reminderGuidance = request.ReminderIntentDetected
            ? """
          REMINDER_GUIDANCE:
          The app is already showing the user a reminder option below the chat.
          Do not tell the user to write it down, make a note, set an alarm, or use another reminder system.
          If useful, gently refer to the reminder option that is already available.
          """
            : string.Empty;

            return $"""
            ROLE: {_options.Role}
            GOAL: {_options.Goal}
            STYLE: {_options.Style}
            AVOID: {_options.Avoid}
            FORMAT: {_options.Format}
            LIMITS: under {_options.WordLimit} words, max one question.
            MODE: {responseMode}
            MODE_GUIDANCE: {modeGuidance}
            {reminderGuidance}
            RECENT_PATTERN: {recentModes}
            CONTEXT: {request.EmotionalContext}
            TASKS: {openTasks}
            USER: {request.Message}
            RECENT_CHAT: {recentChat}
            """;
    }

    public ArloPromptBuilder(IOptions<ArloPromptOptions> options)
    {
        _options = options.Value;
    }
    private static string GetModeGuidance(string responseMode)
    {
    return responseMode switch
    {
        "grounding" =>
            """
            Support focus:
            - Slow things down.
            - Reduce panic and urgency.
            - Use calm grounding language.
            - Avoid overwhelming suggestions.
            """,

        "task_paralysis" =>
            """
            Support focus:
            - Reduce activation energy.
            - Make tasks feel smaller and safer.
            - Focus on starting, not finishing.
            - Avoid productivity pressure.
            """,

        "overwhelm" =>
            """
            Support focus:
            - Reduce cognitive load.
            - Help prioritise emotionally, not perfectly.
            - Reassure the user they do not need to solve everything now.
            """,

        "low_energy" =>
            """
            Support focus:
            - Lower expectations gently.
            - Encourage nervous system care.
            - Focus on small physical actions.
            - Avoid high-energy motivational language.
            """,

        "shame_spiral" =>
            """
            Support focus:
            - Reduce shame and self-judgement.
            - Explain that struggle is not failure.
            - Avoid fake positivity.
            - Encourage self-compassion without sounding cheesy.
            """,

        "celebration" =>
            """
            Support focus:
            - Celebrate progress warmly.
            - Reinforce momentum gently.
            - Avoid sounding overexcited or childish.
            """,

        _ =>
            """
            Support focus:
            - Be calm, practical, and emotionally safe.
            """
    };
}
}
