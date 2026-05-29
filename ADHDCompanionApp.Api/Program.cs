using ADHDCompanionApp.Api.Services;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Arlo services and options
builder.Services.AddSingleton<ArloSafetyService>();
builder.Services.AddSingleton<ArloPromptBuilder>();
builder.Services.AddSingleton<ArloAiService>();
builder.Services.Configure<ArloAiOptions>(
builder.Configuration.GetSection("ArloAi"));
builder.Services.AddSingleton<ArloResponseModeDetector>();
builder.Services.Configure<ArloPromptOptions>(builder.Configuration.GetSection("ArloPrompt"));

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("ArloAiPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            reply = "I’m still here. Let’s pause for a moment so we don’t overload things. Try again shortly."
        }, token);
    };
});

var app = builder.Build();

app.UseRateLimiter();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
