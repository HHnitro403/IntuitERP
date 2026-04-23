using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using IntuiERP.Avalonia.UI.Services;
using IntuiERP.Avalonia.UI.models;
using IntuiERP.Avalonia.UI.Helpers;
using IntuiERP.Avalonia.UI.Views.Search;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.Views;

public partial class MenuPage : UserControl
{
    private readonly PermissionService _permissionService;
    private readonly UserContext _userContext;
    private readonly VendaService _vendaService;
    private readonly CompraService _compraService;
    private readonly ProdutoService _produtoService;
    private readonly ClienteService _clienteService;
    private readonly VendedorService _vendedorService;
    private readonly FornecedorService _fornecedorService;
    private readonly CidadeService _cidadeService;
    private readonly UsuarioService _usuarioService;
    private readonly NpgsqlConnectionFactory _factory;
    
    public ObservableCollection<VendaDisplayModel> RecentOrders { get; } = new();

    public MenuPage()
    {
        InitializeComponent();

        _permissionService = new PermissionService();
        _userContext = UserContext.Instance;
        
        _factory = new NpgsqlConnectionFactory();
        var connection = _factory.CreateConnection(); // For services that take IDbConnection
        
        _vendaService = new VendaService(connection);
        _compraService = new CompraService(connection);
        _produtoService = new ProdutoService(connection);
        _clienteService = new ClienteService(connection);
        _vendedorService = new VendedorService(connection);
        _fornecedorService = new FornecedorService(connection);
        _cidadeService = new CidadeService(connection);
        _usuarioService = new UsuarioService(_factory);

        this.Loaded += OnLoaded;
        
        // Wire up buttons
        ProdutosBtn.Click += (s, e) => OnNavigationClicked("Produtos");
        CidadesBtn.Click += (s, e) => OnNavigationClicked("Cidades");
        ClientesBtn.Click += (s, e) => OnNavigationClicked("Clientes");
        FornecedoresBtn.Click += (s, e) => OnNavigationClicked("Fornecedores");
        UsuariosBtn.Click += (s, e) => OnNavigationClicked("Usuarios");
        VendedoresBtn.Click += (s, e) => OnNavigationClicked("Vendedores");
        ComprasBtn.Click += (s, e) => OnNavigationClicked("Compras");
        VendasBtn.Click += (s, e) => OnNavigationClicked("Vendas");
        ContasReceberBtn.Click += (s, e) => OnNavigationClicked("ContasReceber");
        ContasPagarBtn.Click += (s, e) => OnNavigationClicked("ContasPagar");
        EstoqueBtn.Click += (s, e) => OnNavigationClicked("Estoque");
        ReportsBtn.Click += (s, e) => OnNavigationClicked("Reports");
        
        ToggleThemeButton.Click += (s, e) => ToggleTheme();
        LogoutButton.Click += LogoutButton_Click;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        DataLabel.Text = DateTime.Now.ToString("dd/MM/yyyy");
        WelcomeLabel.Text = $"Welcome, {_userContext.CurrentUser?.Usuario ?? "User"}";
        
        await LoadDashboardData();
    }

    private async Task LoadDashboardData()
    {
        try
        {
            // Monthly Sales
            var salesResult = await GetMonthlyVendaComparisonDataAsync();
            TotalVendasMes.Text = salesResult.TotalAtual.ToString("C2");
            PorcentagemVendas.Text = (salesResult.Variacao >= 0 ? "+" : "") + $"{salesResult.Variacao:F2}%";
            PorcentagemVendas.Foreground = salesResult.Variacao >= 0 ? Brushes.Green : Brushes.Red;

            // Monthly Expenses
            var expensesResult = await GetMonthlyCompraComparisonDataAsync();
            TotalDespesasMes.Text = expensesResult.TotalAtual.ToString("C2");
            PorcentagemDespesas.Text = (expensesResult.Variacao >= 0 ? "+" : "") + $"{expensesResult.Variacao:F2}%";
            PorcentagemDespesas.Foreground = expensesResult.Variacao >= 0 ? Brushes.Green : Brushes.Red;

            // Inventory
            var stockResult = await GetInventoryStatsAsync();
            TotalProdutos.Text = stockResult.Total.ToString();
            ProdEstBaixo.Text = $"{stockResult.LowStock} produtos com estoque baixo";

            // Recent Orders
            await LoadRecentOrders();
        }
        catch (Exception ex)
        {
            await MessageBox.Show(NavigationHelper.GetWindow(this), ex.Message, "Error Loading Dashboard");
        }
    }

    private async Task<(decimal TotalAnterior, decimal TotalAtual, decimal Variacao)> GetMonthlyVendaComparisonDataAsync()
    {
        var today = DateTime.Now;
        var inicioMesAtual = new DateTime(today.Year, today.Month, 1);
        var fimMesAtual = inicioMesAtual.AddMonths(1).AddDays(-1);
        var inicioMesAnterior = inicioMesAtual.AddMonths(-1);
        var fimMesAnterior = inicioMesAtual.AddDays(-1);

        var vendasAnterior = await _vendaService.GetAllAsync(new VendaFilterModel { DataInicial = inicioMesAnterior, DataFinal = fimMesAnterior });
        var vendasAtual = await _vendaService.GetAllAsync(new VendaFilterModel { DataInicial = inicioMesAtual, DataFinal = fimMesAtual });

        decimal totalAnterior = vendasAnterior.Sum(v => v.valor_total);
        decimal totalAtual = vendasAtual.Sum(v => v.valor_total);

        decimal variacao = totalAnterior > 0 ? ((totalAtual - totalAnterior) / totalAnterior) * 100 : (totalAtual > 0 ? 100 : 0);
        return (totalAnterior, totalAtual, variacao);
    }

    private async Task<(decimal TotalAnterior, decimal TotalAtual, decimal Variacao)> GetMonthlyCompraComparisonDataAsync()
    {
        var today = DateTime.Now;
        var inicioMesAtual = new DateTime(today.Year, today.Month, 1);
        var fimMesAtual = inicioMesAtual.AddMonths(1).AddDays(-1);
        var inicioMesAnterior = inicioMesAtual.AddMonths(-1);
        var fimMesAnterior = inicioMesAtual.AddDays(-1);

        var comprasAnterior = await _compraService.GetAllComprasAsync(new CompraFilterModel { DataInicial = inicioMesAnterior, DataFinal = fimMesAnterior });
        var comprasAtual = await _compraService.GetAllComprasAsync(new CompraFilterModel { DataInicial = inicioMesAtual, DataFinal = fimMesAtual });

        decimal totalAnterior = comprasAnterior.Sum(c => c.valor_total ?? 0m);
        decimal totalAtual = comprasAtual.Sum(c => c.valor_total ?? 0m);

        decimal variacao = totalAnterior > 0 ? ((totalAtual - totalAnterior) / totalAnterior) * 100 : (totalAtual > 0 ? 100 : 0);
        return (totalAnterior, totalAtual, variacao);
    }

    private async Task<(int Total, int LowStock)> GetInventoryStatsAsync()
    {
        var produtos = await _produtoService.GetAllAsync();
        int total = produtos.Count();
        int lowStock = produtos.Count(p => p.SaldoEst < p.EstMinimo);
        return (total, lowStock);
    }

    private async Task LoadRecentOrders()
    {
        try 
        {
            var recentVendas = await _vendaService.GetAllAsync();
            var clientes = await _clienteService.GetAllAsync();
            var vendedores = await _vendedorService.GetAllAsync();

            var clientesDict = clientes.ToDictionary(c => c.CodCliente, c => c.Nome);
            var vendedoresDict = vendedores.ToDictionary(v => v.CodVendedor, v => v.NomeVendedor);

            RecentOrders.Clear();
            var displayOrders = recentVendas
                .OrderByDescending(v => v.data_venda)
                .ThenByDescending(v => v.hora_venda)
                .Take(10)
                .Select(v => new VendaDisplayModel
                {
                    CodVenda = v.CodVenda,
                    DataVenda = v.data_venda,
                    ValorTotal = v.valor_total,
                    NomeCliente = v.CodCliente > 0 && clientesDict.TryGetValue(v.CodCliente, out var cNome) ? cNome : "Desconhecido",
                    NomeVendedor = v.CodVendedor.HasValue && vendedoresDict.TryGetValue(v.CodVendedor.Value, out var vNome) ? vNome : "Desconhecido"
                });

            foreach (var order in displayOrders)
                RecentOrders.Add(order);

            RecentOrdersList.ItemsSource = RecentOrders;
            EmptyRecentOrdersLabel.IsVisible = !RecentOrders.Any();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading recent orders: {ex.Message}");
        }
    }

    private async void OnNavigationClicked(string destination)
    {
        try
        {
            UserControl? nextPage = null;

            switch (destination)
            {
                case "Produtos":
                    if (!_permissionService.CanReadProduct())
                    {
                        await MessageBox.Show(NavigationHelper.GetWindow(this), _permissionService.GetPermissionDeniedMessage("acessar produtos"), "Acesso Negado");
                        return;
                    }
                    nextPage = new ProdutoSearch();
                    break;

                case "Cidades":
                    nextPage = new CadastroCidade(); 
                    break;

                case "Clientes":
                    if (!_permissionService.CanReadClient())
                    {
                        await MessageBox.Show(NavigationHelper.GetWindow(this), _permissionService.GetPermissionDeniedMessage("acessar clientes"), "Acesso Negado");
                        return;
                    }
                    nextPage = new ClienteSearch();
                    break;

                case "Fornecedores":
                    if (!_permissionService.CanReadSupplier())
                    {
                        await MessageBox.Show(NavigationHelper.GetWindow(this), _permissionService.GetPermissionDeniedMessage("acessar fornecedores"), "Acesso Negado");
                        return;
                    }
                    nextPage = new FornecedorSearch();
                    break;

                case "Usuarios":
                    if (!_permissionService.IsAdministrator())
                    {
                        await MessageBox.Show(NavigationHelper.GetWindow(this), "Apenas administradores podem gerenciar usuários do sistema.", "Acesso Negado");
                        return;
                    }
                    nextPage = new UsuarioSearch();
                    break;

                case "Vendedores":
                    if (!_permissionService.CanReadSeller())
                    {
                        await MessageBox.Show(NavigationHelper.GetWindow(this), _permissionService.GetPermissionDeniedMessage("acessar vendedores"), "Acesso Negado");
                        return;
                    }
                    nextPage = new VendedorSearch();
                    break;

                case "Compras":
                    if (!_permissionService.CanReadSupplier()) // Purchases use supplier permission
                    {
                        await MessageBox.Show(NavigationHelper.GetWindow(this), _permissionService.GetPermissionDeniedMessage("acessar compras"), "Acesso Negado");
                        return;
                    }
                    nextPage = new CompraSearch();
                    break;

                case "Vendas":
                    if (!_permissionService.CanReadSale())
                    {
                        await MessageBox.Show(NavigationHelper.GetWindow(this), _permissionService.GetPermissionDeniedMessage("acessar vendas"), "Acesso Negado");
                        return;
                    }
                    nextPage = new VendaSearch();
                    break;

                case "ContasReceber":
                    nextPage = new ContasReceberSearch();
                    break;

                case "ContasPagar":
                    nextPage = new ContasPagarSearch();
                    break;

                case "Estoque":
                    nextPage = new EstoqueSearch();
                    break;

                case "Reports":
                    if (!_permissionService.CanGenerateReports())
                    {
                        await MessageBox.Show(NavigationHelper.GetWindow(this), _permissionService.GetPermissionDeniedMessage("gerar relatórios"), "Acesso Negado");
                        return;
                    }
                    await MessageBox.Show(NavigationHelper.GetWindow(this), "Módulo de Relatórios em desenvolvimento.", "Informação");
                    break;
            }

            if (nextPage != null)
            {
                NavigationHelper.NavigateTo(nextPage);
            }
        }
        catch (Exception ex)
        {
            await MessageBox.Show(NavigationHelper.GetWindow(this), $"Falha ao abrir tela: {ex.Message}", "Erro");
        }
    }

    private void ToggleTheme()
    {
        var app = Application.Current;
        if (app != null)
        {
            var currentTheme = app.ActualThemeVariant;
            app.RequestedThemeVariant = currentTheme == global::Avalonia.Styling.ThemeVariant.Dark 
                ? global::Avalonia.Styling.ThemeVariant.Light 
                : global::Avalonia.Styling.ThemeVariant.Dark;
        }
    }

    private void LogoutButton_Click(object? sender, RoutedEventArgs e)
    {
        _userContext.ClearCurrentUser();
        NavigationHelper.NavigateTo(new MainWindow()); // Return to Login
    }
}
