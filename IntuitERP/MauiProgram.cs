
using IntuitERP.Services;
using IntuitERP.Viwes.Reports;
using Microsoft.Extensions.Logging;

namespace IntuitERP;

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

        SQLitePCL.Batteries_V2.Init();
        builder.Services.AddTransient<ReportsPage>();
        builder.Services.AddTransient<ReportsService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

       
        

        return builder.Build();

    }
}