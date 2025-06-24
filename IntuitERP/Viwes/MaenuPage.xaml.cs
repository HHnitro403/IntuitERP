using IntuitERP.Config;
using IntuitERP.Services;
using IntuitERP.Viwes.Search;
using System.Data;

namespace IntuitERP.Viwes;

public partial class MaenuPage : ContentPage
{


    public MaenuPage()
    {
        InitializeComponent();
        BindingContext = this;


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
            var cadastroCompraPage = new CompraSearch(comprasService, itemService, fornecedorService, vendedorService, produtoService,  estoqueService); // Adjust constructor as needed
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
    


}