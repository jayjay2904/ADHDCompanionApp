using ADHDCompanionApp.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenAI;
using System.Diagnostics;


namespace ADHDCompanionApp.Api.Controllers;



[ApiController]
[Route("api/[controller]")]
public class ArloController : ControllerBase
{
    private readonly ArloSafetyService _safetyService;
    private readonly ArloPromptBuilder _promptBuilder;
    private readonly ArloAiService _arloAiService;
    private readonly ArloResponseModeDetector _modeDetector;
    public ArloController(
    ArloSafetyService safetyService,
    ArloPromptBuilder promptBuilder,
    ArloAiService arloAiService,
    ArloResponseModeDetector modeDetector)
    {
        _safetyService = safetyService;
        _promptBuilder = promptBuilder;
        _arloAiService = arloAiService;
        _modeDetector = modeDetector;
    }

    [HttpPost("chat")]
    [EnableRateLimiting("ArloAiPolicy")]
    public async Task<IActionResult> Chat([FromBody] ArloChatRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new ArloChatResponse
            {
                Reply = "I’m here, but I didn’t receive a message."
            });
        }

        request.Message = request.Message.Trim();

        if (request.Message.Length > 500)
        {
            return BadRequest(new ArloChatResponse
            {
                Reply = "That’s a lot to hold at once. Try sending me the short version first."
            });
        }

        request.OpenTasks = (request.OpenTasks ?? new List<string>())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Take(3)
            .ToList();

        request.RecentModes = (request.RecentModes ?? new List<string>())
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Take(3)
            .ToList();
        Debug.WriteLine($"Recent modes received: {string.Join(", ", request.RecentModes)}");

        if (_safetyService.ContainsCrisisLanguage(request.Message))
        {
            return Ok(new ArloChatResponse
            {
                Reply = _safetyService.GetCrisisReply()
            });
        }

        var responseMode = _modeDetector.Detect(request.Message);
        Debug.WriteLine($"Arlo response mode detected: {responseMode}");

        var prompt = _promptBuilder.Build(request, responseMode);

        var reply = await _arloAiService.GetReplyAsync(prompt);

        if (string.IsNullOrWhiteSpace(reply))
        {
            reply = "I’m here with you. We don’t need to fix everything right now. Let’s make this smaller: what is the next small thing you can do from where you are?";
        }

        return Ok(new ArloChatResponse
        {
            Reply = reply
        });
    }

    [HttpPost("ai-test")]
    public async Task<IActionResult> AiTest()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return BadRequest("OpenAI API key is missing.");
        }

        var client = new OpenAIClient(apiKey);

        var response = await client.GetChatClient("gpt-4o-mini")
            .CompleteChatAsync("Say hello as Arlo in one calm sentence.");

        return Ok(response.Value.Content[0].Text);
    }
    
}


public class ArloChatRequest
{
    public string Message { get; set; } = string.Empty;

    public string EmotionalContext { get; set; } = string.Empty;

    public List<string> OpenTasks { get; set; } = new();
    public List<string> RecentModes { get; set; } = new();
    public List<string> RecentChat { get; set; } = new();
    public bool ReminderIntentDetected { get; set; }
}
public class ArloChatResponse
{
    public string Reply { get; set; } = string.Empty;
}