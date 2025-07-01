using BDConfigurator.Services;
using Microsoft.Extensions.Logging;


namespace BDConfigurator
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            //register services
            builder.Services.AddSingleton<DatabaseService>();
            // Register the Page (ViewModel is no longer needed)
            builder.Services.AddSingleton<MainPage>();


            return builder.Build();
        }
    }
}
