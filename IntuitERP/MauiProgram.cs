
using IntuitERP.Services;
using IntuitERP.Viwes;
using IntuitERP.Viwes.Reports;
using IntuitERP.Viwes.Search;
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

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // ===== PHASE 2: DEPENDENCY INJECTION SETUP =====
        // Proper service registration with connection management and lifecycle

        // Core Infrastructure Services (Singleton - one instance for app lifetime)
        builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
        builder.Services.AddSingleton<TransactionService>();
        builder.Services.AddSingleton<PasswordHashingService>();
        builder.Services.AddSingleton<PermissionService>();

        // Settings Services (Singleton for caching and session management)
        builder.Services.AddSingleton<SystemSettingsService>(sp =>
            new SystemSettingsService(sp.GetRequiredService<IDbConnectionFactory>().CreateConnection()));
        builder.Services.AddSingleton<UserSettingsService>(sp =>
            new UserSettingsService(sp.GetRequiredService<IDbConnectionFactory>().CreateConnection()));
        builder.Services.AddSingleton<SessionTimeoutService>(sp =>
            new SessionTimeoutService(
                sp.GetRequiredService<SystemSettingsService>(),
                UserContext.Instance));

        // Data Services (Transient - new instance per request for proper connection management)
        builder.Services.AddTransient<UsuarioService>(sp =>
            new UsuarioService(sp.GetRequiredService<IDbConnectionFactory>().CreateConnection()));
        builder.Services.AddTransient<ProdutoService>(sp =>
            new ProdutoService(sp.GetRequiredService<IDbConnectionFactory>().CreateConnection()));
        builder.Services.AddTransient<ClienteService>(sp =>
            new ClienteService(sp.GetRequiredService<IDbConnectionFactory>().CreateConnection()));
        builder.Services.AddTransient<FornecedorService>(sp =>
            new FornecedorService(sp.GetRequiredService<IDbConnectionFactory>().CreateConnection()));
        builder.Services.AddTransient<VendedorService>(sp =>
            new VendedorService(sp.GetRequiredService<IDbConnectionFactory>().CreateConnection()));
        builder.Services.AddTransient<VendaService>(sp =>
            new VendaService(sp.GetRequiredService<IDbConnectionFactory>().CreateConnection()));
        builder.Services.AddTransient<CompraService>(sp =>
            new CompraService(sp.GetRequiredService<IDbConnectionFactory>().CreateConnection()));
        builder.Services.AddTransient<CidadeService>(sp =>
            new CidadeService(sp.GetRequiredService<IDbConnectionFactory>().CreateConnection()));
        builder.Services.AddTransient<EstoqueService>(sp =>
            new EstoqueService(sp.GetRequiredService<IDbConnectionFactory>().CreateConnection()));
        builder.Services.AddTransient<ItemVendaService>(sp =>
            new ItemVendaService(sp.GetRequiredService<IDbConnectionFactory>().CreateConnection()));
        builder.Services.AddTransient<ItemCompraService>(sp =>
            new ItemCompraService(sp.GetRequiredService<IDbConnectionFactory>().CreateConnection()));

        // Financial Services (Contas a Pagar e Contas a Receber)
        builder.Services.AddTransient<ContaReceberService>(sp =>
            new ContaReceberService(sp.GetRequiredService<IDbConnectionFactory>().CreateConnection()));
        builder.Services.AddTransient<ParcelaReceberService>(sp =>
        {
            var connection = sp.GetRequiredService<IDbConnectionFactory>().CreateConnection();
            var contaService = new ContaReceberService(connection);
            return new ParcelaReceberService(connection, contaService);
        });
        builder.Services.AddTransient<ContaPagarService>(sp =>
            new ContaPagarService(sp.GetRequiredService<IDbConnectionFactory>().CreateConnection()));
        builder.Services.AddTransient<ParcelaPagarService>(sp =>
        {
            var connection = sp.GetRequiredService<IDbConnectionFactory>().CreateConnection();
            var contaService = new ContaPagarService(connection);
            return new ParcelaPagarService(connection, contaService);
        });

        // Report Services
        builder.Services.AddTransient<ReportsPage>();
        builder.Services.AddTransient<ReportsService>();
        builder.Services.AddSingleton<PdfReportService>();

        // Pages (Transient - new instance on each navigation)
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<MaenuPage>();

        return builder.Build();
    }
}