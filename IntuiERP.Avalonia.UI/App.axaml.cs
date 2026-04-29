using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using IntuiERP.Avalonia.UI.Services;
using System;

namespace IntuiERP.Avalonia.UI
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            
            // Enable Dapper to match snake_case database columns to PascalCase properties
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            ConfigureServices();
        }

        private static void ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
            services.AddSingleton<PermissionService>();
            services.AddTransient<ProdutoService>(sp =>
                new ProdutoService(sp.GetRequiredService<IDbConnectionFactory>().CreateConnection()));
            services.AddTransient<FornecedorService>(sp =>
                new FornecedorService(sp.GetRequiredService<IDbConnectionFactory>().CreateConnection()));

            Services = services.BuildServiceProvider();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
