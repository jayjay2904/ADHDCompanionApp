using ADHDCompanionApp.Services;
using ADHDCompanionApp.Services.Implementations;
using ADHDCompanionApp.Services.Interfaces;
using ADHDCompanionApp.ViewModels;
using ADHDCompanionApp.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

#if ANDROID
using ADHDCompanionApp.Platforms.Android.Services;
using Microsoft.Maui.Handlers;
#endif

namespace ADHDCompanionApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if ANDROID
            EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
            });
#endif

            // Register Services
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<IUserProfileService, UserProfileService>();
            builder.Services.AddSingleton<ICheckInService, CheckInService>();
            builder.Services.AddSingleton<ITaskService, TaskService>();
            builder.Services.AddSingleton<IWinService, WinService>();
            builder.Services.AddSingleton<ITruthBombService, TruthBombService>();
            builder.Services.AddSingleton<ISupportService, SupportService>();
            builder.Services.AddSingleton<IArloService, ArloService>();
            builder.Services.AddSingleton<IReminderEngine, ReminderEngine>();
            

            builder.Services.AddSingleton(new HttpClient
            {
                BaseAddress = new Uri("http://192.168.4.221:5276/"),
                Timeout = TimeSpan.FromSeconds(45)
            });

            builder.Services.AddSingleton<IArloAiClient, BackendArloAiClient>();

            // Register ViewModels
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddTransient<QuickSetupViewModel>();
            builder.Services.AddTransient<TodayViewModel>();
            builder.Services.AddTransient<SupportViewModel>();
            builder.Services.AddTransient<ProgressViewModel>();
            builder.Services.AddTransient<ArloViewModel>();
            builder.Services.AddTransient<PreferencesViewModel>();
            builder.Services.AddTransient<ProgressSummaryViewModel>();
            

            // Register Pages
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<QuickSetupPage>();
            builder.Services.AddTransient<PreferencesPage>();
            builder.Services.AddTransient<ProgressSummaryPopup>();


#if ANDROID
            builder.Services.AddSingleton<IPlatformReminderScheduler, AndroidReminderScheduler>();
            builder.Services.AddSingleton<ISpeechToTextService, AndroidSpeechToTextService>();
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();


        }
    }
}