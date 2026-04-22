using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using IntuitERP.Services;
using IntuitERP.models;
using IntuitERP.Config;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace IntuitERP.Desktop.Views
{
    public partial class MainMenuView : UserControl
    {
        private TextBlock WelcomeLabel => this.FindControl<TextBlock>("WelcomeLabel")!;
        private TextBlock DataLabel => this.FindControl<TextBlock>("DataLabel")!;
        private TextBlock TotalVendasMes => this.FindControl<TextBlock>("TotalVendasMes")!;
        private TextBlock TotalDespesasMes => this.FindControl<TextBlock>("TotalDespesasMes")!;
        private TextBlock TotalProdutos => this.FindControl<TextBlock>("TotalProdutos")!;
        private ListBox RecentOrdersList => this.FindControl<ListBox>("RecentOrdersList")!;

        private readonly UserContext _userContext;
        public ObservableCollection<VendaDisplayModel> RecentOrders { get; set; } = new();

        public MainMenuView()
        {
            InitializeComponent();
            _userContext = UserContext.Instance;
            WelcomeLabel.Text = $"Bem-vindo, {_userContext.CurrentUser?.Nome ?? "Usuário"}";
            DataLabel.Text = DateTime.Now.ToString("dd/MM/yyyy");
            
            RecentOrdersList.ItemsSource = RecentOrders;
            
            // Wire events
            this.FindControl<Button>("ProdutosButton")!.Click += OnProdutosClick;
            this.FindControl<Button>("ClientesButton")!.Click += OnClientesClick;
            this.FindControl<Button>("FornecedoresButton")!.Click += OnFornecedoresClick;
            this.FindControl<Button>("UsuariosButton")!.Click += OnUsuariosClick;
            this.FindControl<Button>("ComprasButton")!.Click += OnComprasClick;
            this.FindControl<Button>("VendasButton")!.Click += OnVendasClick;
            this.FindControl<Button>("ContasReceberButton")!.Click += OnContasReceberClick;
            this.FindControl<Button>("ContasPagarButton")!.Click += OnContasPagarClick;

            LoadDashboardData();
        }

        private void InitializeComponent()
        {
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }

        private async void LoadDashboardData()
        {
            try
            {
                var configurator = new Configurator();
                IDbConnection connection = configurator.GetMySqlConnection();
                
                var vendasService = new VendaService(connection);
                var comprasService = new CompraService(connection);
                var produtoService = new ProdutoService(connection);
                var clienteService = new ClienteService(connection);
                var vendedorService = new VendedorService(connection);

                // Basic metrics
                var vendas = await vendasService.GetAllAsync(new VendaFilterModel { DataInicial = DateTime.Now.AddMonths(-1) });
                TotalVendasMes.Text = vendas.Sum(v => v.valor_total).ToString("C2");

                var compras = await comprasService.GetAllComprasAsync(new CompraFilterModel { DataInicial = DateTime.Now.AddMonths(-1) });
                TotalDespesasMes.Text = compras.Sum(c => c.valor_total ?? 0).ToString("C2");

                var produtos = await produtoService.GetAllAsync();
                TotalProdutos.Text = produtos.Count().ToString();

                // Recent Orders
                var allVendas = await vendasService.GetAllAsync(new VendaFilterModel());
                var clientes = await clienteService.GetAllAsync();
                var vendedores = await vendedorService.GetAllAsync();
                
                var clientesDict = clientes.ToDictionary(c => c.CodCliente, c => c.Nome);
                var vendedoresDict = vendedores.ToDictionary(v => v.CodVendedor, v => v.NomeVendedor);

                RecentOrders.Clear();
                foreach (var v in allVendas.OrderByDescending(x => x.data_venda).Take(5))
                {
                    RecentOrders.Add(new VendaDisplayModel
                    {
                        NomeCliente = v.CodCliente > 0 && clientesDict.ContainsKey(v.CodCliente) ? clientesDict[v.CodCliente]! : "N/A",
                        NomeVendedor = v.CodVendedor.HasValue && vendedoresDict.ContainsKey(v.CodVendedor.Value) ? vendedoresDict[v.CodVendedor.Value]! : "N/A",
                        ValorTotal = v.valor_total,
                        DataVenda = v.data_venda ?? DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dashboard: {ex.Message}");
            }
        }

        // Navigation Handlers (Placeholders)
        private void OnProdutosClick(object sender, RoutedEventArgs e) { }
        private void OnClientesClick(object sender, RoutedEventArgs e) { }
        private void OnFornecedoresClick(object sender, RoutedEventArgs e) { }
        private void OnUsuariosClick(object sender, RoutedEventArgs e) { }
        private void OnComprasClick(object sender, RoutedEventArgs e) { }
        private void OnVendasClick(object sender, RoutedEventArgs e) { }
        private void OnContasReceberClick(object sender, RoutedEventArgs e) { }
        private void OnContasPagarClick(object sender, RoutedEventArgs e) { }

        // Helper model for display
        public class VendaDisplayModel
        {
            public string NomeCliente { get; set; } = "";
            public string NomeVendedor { get; set; } = "";
            public decimal ValorTotal { get; set; }
            public DateTime DataVenda { get; set; }
        }
    }
}

