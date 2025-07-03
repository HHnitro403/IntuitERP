
using IntuitERP.Services;
using IntuitERP.Viwes.Reports;
using Microsoft.Extensions.Logging;

namespace IntuitERP;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var logPath = Path.Combine(desktopPath, "IntuitERP_CrashLog.txt");

        try
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

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            try
            {
                Exception ex = (Exception)args.ExceptionObject;
                string logPath = Path.Combine(FileSystem.Current.AppDataDirectory, "fatal_error.log");
                string errorText = $"FATAL CRASH on {DateTime.Now}\n\n{ex.ToString()}";
                File.WriteAllText(logPath, errorText);
            }
            catch
            {
                // Se até o log falhar, não há muito o que fazer.
            }
        };

        return builder.Build();

        }
        catch (Exception ex)
        {
            // Se QUALQUER erro ocorrer na inicialização, ele será capturado aqui
            File.WriteAllText(logPath, $"CRASH DURING STARTUP on {DateTime.Now}\n\n{ex.ToString()}");

            // Relança a exceção para que o app ainda feche, mas APÓS o log ter sido salvo
            throw;
        }
    }
}