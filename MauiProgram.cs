using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using ADHDCompanionApp.Services.Interfaces;
using ADHDCompanionApp.Services.Implementations;
using ADHDCompanionApp.ViewModels;

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
            // Register ViewModels
            builder.Services.AddSingleton<MainViewModel>();

            // Register Services 
            builder.Services.AddSingleton<IUserProfileService, UserProfileService>();
            builder.Services.AddTransient<QuickSetupViewModel>();
            builder.Services.AddSingleton<ICheckInService, CheckInService>();
            builder.Services.AddTransient<TodayViewModel>();
            builder.Services.AddSingleton<ITaskService, TaskService>();
            builder.Services.AddSingleton<IWinService, WinService>();
            builder.Services.AddSingleton<ITruthBombService, TruthBombService>();
            builder.Services.AddSingleton<ISupportService, SupportService>();
            builder.Services.AddTransient<SupportViewModel>();
            builder.Services.AddTransient<ProgressViewModel>();
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<IArloService, ArloService>();
            builder.Services.AddTransient<ArloViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
    #endif

            return builder.Build();
        }
    }
}
