using IntuitERP.Config;
using IntuitERP.models;
using IntuitERP.Services;
using IntuitERP.Viwes.Reports;
using IntuitERP.Viwes.Search;
using System.Data;
using System.Threading.Tasks;

namespace IntuitERP.Viwes;

public partial class MaenuPage : ContentPage
{
    public MaenuPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        DataLabel.Text = DateTime.Now.ToString("dd/MM/yyyy");

        //calculate total sales this month

        try
        {
            //Venda
            var configurator = new Configurator();
            IDbConnection connection = configurator.GetMySqlConnection();
            var vendasService = new VendaService(connection);
            var result = await GetMonthlyVendaComparisonDataAsync(vendasService);

            TotalVendasMes.Text = result.TotalAtual.ToString("C2");

            if (result.Variacao >= 0)
            {
                // Handles gains and no-change scenarios
                PorcentagemVendas.Text = $"+{result.Variacao:F2}%";
                PorcentagemVendas.TextColor = Colors.Green;
            }
            else
            {
                // Handles loss scenarios
                PorcentagemVendas.Text = $"{result.Variacao:F2}%";
                PorcentagemVendas.TextColor = Colors.Red;
            }

            //Compra
            var comprasService = new CompraService(connection);
            var resultCompra = await GetMonthlyCompraComparisonDataAsync(comprasService);

            TotalDespesasMes.Text = resultCompra.TotalAtual.ToString("C2");

            if (resultCompra.Variacao >= 0)
            {
                // Handles gains and no-change scenarios
                PorcentagemDespesas.Text = $"+{resultCompra.Variacao:F2}%";
                PorcentagemDespesas.TextColor = Colors.Green;
            }
            else
            {
                // Handles loss scenarios
                PorcentagemDespesas.Text = $"{resultCompra.Variacao:F2}%";
                PorcentagemDespesas.TextColor = Colors.Red;
            }

            var produtoService = new ProdutoService(connection);
            var resultProduto = await GetTotalProdutosAsync(produtoService);
            TotalProdutos.Text = resultProduto.Totalprodutos.ToString();
            ProdEstBaixo.Text = resultProduto.totalprodutosnegativos.ToString();

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    public async Task<(int Totalprodutos, int totalprodutosestposi, int totalprodutosnegativos)> GetTotalProdutosAsync(ProdutoService produtoService)
    {
        var produtos = await produtoService.GetAllAsync();
        int totalprodutos = produtos.Count(p => p.SaldoEst >= 0);
        int totalprodutosestposi = produtos.Count(p => p.SaldoEst > p.EstMinimo);
        int totalprodutosnegativos = produtos.Count(p => p.SaldoEst < p.EstMinimo);
        return (totalprodutos, totalprodutosestposi, totalprodutosnegativos);
    }


    public async Task<(decimal TotalAnterior, decimal TotalAtual, decimal Variacao)> GetMonthlyVendaComparisonDataAsync(VendaService vendaRepository)
    {
        // === Part 1: Define Time Periods ===
        var today = new DateTime(2025, 6, 26);
        var inicioMesAtual = new DateTime(today.Year, today.Month, 1);
        var fimMesAtual = inicioMesAtual.AddMonths(1).AddDays(-1);
        var inicioMesAnterior = inicioMesAtual.AddMonths(-1);
        var fimMesAnterior = inicioMesAtual.AddDays(-1);

        // === Part 2: Use the existing GetAllAsync method ===
        var filtroMesAnterior = new VendaFilterModel { DataInicial = inicioMesAnterior, DataFinal = fimMesAnterior };
        var vendasMesAnterior = await vendaRepository.GetAllAsync(filtroMesAnterior);

        var filtroMesAtual = new VendaFilterModel { DataInicial = inicioMesAtual, DataFinal = fimMesAtual };
        var vendasMesAtual = await vendaRepository.GetAllAsync(filtroMesAtual);

        // === Part 3: Perform Calculations ===
        decimal totalMesAnterior = vendasMesAnterior.Sum(v => v.valor_total);
        decimal totalMesAtual = vendasMesAtual.Sum(v => v.valor_total);

        decimal variacaoPercentual = 0;
        if (totalMesAnterior > 0)
        {
            variacaoPercentual = ((totalMesAtual - totalMesAnterior) / totalMesAnterior) * 100;
        }
        else if (totalMesAtual > 0)
        {
            variacaoPercentual = 100;
        }
        return (totalMesAnterior, totalMesAtual, variacaoPercentual);
    }

    // This can live in a service, a reporting class, or even the code-behind
    public async Task<(decimal TotalAnterior, decimal TotalAtual, decimal Variacao)> GetMonthlyCompraComparisonDataAsync(CompraService compraRepository)
    {
        // Part 1: Define Time Periods
        var today = DateTime.Now; // Uses the actual current date
        var inicioMesAtual = new DateTime(today.Year, today.Month, 1);
        var fimMesAtual = inicioMesAtual.AddMonths(1).AddDays(-1);
        var inicioMesAnterior = inicioMesAtual.AddMonths(-1);
        var fimMesAnterior = inicioMesAtual.AddDays(-1);

        // Part 2: Use the existing GetAllComprasAsync method
        var filtroMesAnterior = new CompraFilterModel { DataInicial = inicioMesAnterior, DataFinal = fimMesAnterior };
        var comprasMesAnterior = await compraRepository.GetAllComprasAsync(filtroMesAnterior);

        var filtroMesAtual = new CompraFilterModel { DataInicial = inicioMesAtual, DataFinal = fimMesAtual };
        var comprasMesAtual = await compraRepository.GetAllComprasAsync(filtroMesAtual);

        // Part 3: Perform Calculations
        decimal totalMesAnterior = comprasMesAnterior.Sum(c => c.valor_total ?? 0m);
        decimal totalMesAtual = comprasMesAtual.Sum(c => c.valor_total ?? 0m);

        decimal variacaoPercentual = 0;
        if (totalMesAnterior > 0)
        {
            variacaoPercentual = ((totalMesAtual - totalMesAnterior) / totalMesAnterior) * 100;
        }
        else if (totalMesAtual > 0)
        {
            variacaoPercentual = 100;
        }

        // Part 4: Return the raw data
        return (totalMesAnterior, totalMesAtual, variacaoPercentual);
    }



    // Navigation methods for Cadastros
    private async void OnProdutosClicked(object sender, EventArgs e)
    {
        try
        {
            var configurator = new Configurator();
            IDbConnection connection = configurator.GetMySqlConnection();
            var produtosService = new ProdutoService(connection);
            var fornecedoresService = new FornecedorService(connection);
            var cadastroProdutoPage = new ProdutoSearch(produtosService, fornecedoresService);

            await Navigation.PushAsync(cadastroProdutoPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao abrir cadastro de produtos: {ex.Message}", "OK");
        }
    }

    private async void OnCidadesClicked(object sender, EventArgs e)
    {
        try
        {
            var configurator = new Configurator();
            IDbConnection connection = configurator.GetMySqlConnection();
            var cidadeService = new CidadeService(connection);
            var cadastroCidadePage = new CadastrodeCidade(cidadeService);
            await Navigation.PushAsync(cadastroCidadePage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao abrir cadastro de cidades: {ex.Message}", "OK");
        }
    }

    private async void OnClientesClicked(object sender, EventArgs e)
    {
        try
        {
            var configurator = new Configurator();
            IDbConnection connection = configurator.GetMySqlConnection();
            var clientesService = new ClienteService(connection); // Assuming ClientesService exists
            var cidadeService = new CidadeService(connection);
            var cadastroClientePage = new ClienteSearch(clientesService, cidadeService);
            await Navigation.PushAsync(cadastroClientePage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao abrir cadastro de clientes: {ex.Message}", "OK");
        }
    }

    private async void OnFornecedoresClicked(object sender, EventArgs e)
    {
        try
        {
            var configurator = new Configurator();
            IDbConnection connection = configurator.GetMySqlConnection();
            var fornecedoresService = new FornecedorService(connection); // Assuming FornecedoresService exists
            var cidadeService = new CidadeService(connection);
            var cadastroFornecedorPage = new FornecedorSearch(fornecedoresService, cidadeService); // Assuming CadastrodeFornecedor constructor takes FornecedoresService
            await Navigation.PushAsync(cadastroFornecedorPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao abrir cadastro de fornecedores: {ex.Message}", "OK");
        }
    }

    private async void OnUsuariosClicked(object sender, EventArgs e)
    {
        try
        {
            var configurator = new Configurator();
            IDbConnection connection = configurator.GetMySqlConnection();
            var usuarioService = new UsuarioService(connection); // Assuming UsuarioService exists
            var cadastroUsuarioPage = new UsuarioSearch(usuarioService); // Assuming CadastrodeUsuario constructor takes UsuarioService
            await Navigation.PushAsync(cadastroUsuarioPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao abrir cadastro de usuários: {ex.Message}", "OK");
        }
    }

    private async void OnVendedoresClicked(object sender, EventArgs e)
    {
        try
        {
            var configurator = new Configurator();
            IDbConnection connection = configurator.GetMySqlConnection();
            var vendedorService = new VendedorService(connection); // Assuming VendedorService exists
            var cadastroVendedorPage = new VendedorSearch(vendedorService); // Assuming CadastrodeVendedor constructor takes VendedorService
            await Navigation.PushAsync(cadastroVendedorPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao abrir cadastro de vendedores: {ex.Message}", "OK");
        }
    }

    // Navigation methods for Operações
    private async void OnComprasClicked(object sender, EventArgs e)
    {
        try
        {
            var configurator = new Configurator();
            IDbConnection connection = configurator.GetMySqlConnection();
            var comprasService = new CompraService(connection);
            var itemService = new ItemCompraService(connection);
            var fornecedorService = new FornecedorService(connection);
            var produtoService = new ProdutoService(connection);
            var vendedorService = new VendedorService(connection);
            var estoqueService = new EstoqueService(connection);
            var cadastroCompraPage = new CompraSearch(comprasService, itemService, fornecedorService, vendedorService, produtoService, estoqueService); // Adjust constructor as needed
            await Navigation.PushAsync(cadastroCompraPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao abrir cadastro de compras: {ex.Message}", "OK");
        }
    }

    private async void OnVendasClicked(object sender, EventArgs e)
    {
        try
        {
            var configurator = new Configurator();
            IDbConnection connection = configurator.GetMySqlConnection();
            var vendasService = new VendaService(connection);
            var itensVendaService = new ItemVendaService(connection);
            var clientesService = new ClienteService(connection);
            var produtosService = new ProdutoService(connection);
            var vendedorService = new VendedorService(connection);
            var estoqueService = new EstoqueService(connection);
            // Adjust the constructor of CadastrodeVenda according to its actual dependencies
            var cadastroVendaPage = new VendaSearch(vendasService, itensVendaService, clientesService, vendedorService, produtosService, estoqueService);
            await Navigation.PushAsync(cadastroVendaPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao abrir cadastro de vendas: {ex.Message}", "OK");
        }
    }

    // Navigation method for Estoque
    private async void OnEstoqueClicked(object sender, EventArgs e)
    {
        try
        {
            var configurator = new Configurator();
            IDbConnection connection = configurator.GetMySqlConnection();
            var estoqueService = new EstoqueService(connection); // Assuming EstoqueService exists
            var produtosService = new ProdutoService(connection);
            var cadastroEstoquePage = new CadstroEstoque(estoqueService, produtosService); // Assuming CadstroEstoque constructor takes EstoqueService
            await Navigation.PushAsync(cadastroEstoquePage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao abrir cadastro de estoque: {ex.Message}", "OK");
        }
    }

    private async void GeraRelatButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var configurator = new Configurator();
            IDbConnection connection = configurator.GetMySqlConnection();
            var relatorioService = new ReportsService(connection);
            var reportpage = new ReportsPage(relatorioService);
            await Navigation.PushAsync(reportpage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao gerar relatório: {ex.Message}", "OK");
        }
    }

    private void GeraRelatButtonSintec_Clicked(object sender, EventArgs e)
    {
    }
}