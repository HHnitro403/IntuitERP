using IntuitERP.Config;
using IntuitERP.Services;
using System.Collections.ObjectModel;
using System.Data;

namespace IntuitERP.Viwes.Search;

public partial class CompraSearch : ContentPage
{
    // A simple class to hold combined data for display in the list
    public class CompraDisplayModel
    {
        public int CodCompra { get; set; }
        public DateTime? DataCompra { get; set; }
        public decimal? ValorTotal { get; set; }
        public string Status { get; set; }
        public string NomeFornecedor { get; set; }
        public string NomeVendedor { get; set; }
    }

    private readonly CompraService _compraService;
    private readonly ItemCompraService _itemCompraService;
    private readonly FornecedorService _fornecedorService;
    private readonly VendedorService _vendedorService;
    private readonly ProdutoService _produtoService;
    private readonly EstoqueService _estoqueService;

    public ObservableCollection<CompraDisplayModel> _listaComprasDisplay { get; set; }
    private List<CompraDisplayModel> _masterListaCompras;
    private CompraDisplayModel _compraSelecionada;

    public CompraSearch(CompraService compraService, ItemCompraService itemCompraService, FornecedorService fornecedorService, VendedorService vendedorService, ProdutoService produtoService, EstoqueService estoqueService)
    {
        InitializeComponent();

        _compraService = compraService ?? throw new ArgumentNullException(nameof(compraService));
        _itemCompraService = itemCompraService ?? throw new ArgumentNullException(nameof(itemCompraService));
        _fornecedorService = fornecedorService ?? throw new ArgumentNullException(nameof(fornecedorService));
        _vendedorService = vendedorService ?? throw new ArgumentNullException(nameof(vendedorService));
        _produtoService = produtoService ?? throw new ArgumentNullException(nameof(produtoService));
        _estoqueService = estoqueService ?? throw new ArgumentNullException(nameof(estoqueService));

        _listaComprasDisplay = new ObservableCollection<CompraDisplayModel>();
        _masterListaCompras = new List<CompraDisplayModel>();
        ComprasCollectionView.ItemsSource = _listaComprasDisplay;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _compraSelecionada = null;
        ComprasCollectionView.SelectedItem = null;
        await LoadComprasAsync();
        UpdateActionButtonsState();
    }

    private async Task LoadComprasAsync()
    {
        try
        {
            var compras = await _compraService.GetAllAsync();
            var fornecedores = await _fornecedorService.GetAllAsync();
            var vendedores = await _vendedorService.GetAllAsync();

            var fornecedoresDict = fornecedores.ToDictionary(f => f.CodFornecedor, f => f.NomeFantasia ?? f.RazaoSocial);
            var vendedoresDict = vendedores.ToDictionary(v => v.CodVendedor, v => v.NomeVendedor);

            _masterListaCompras.Clear();

            foreach (var compra in compras.OrderByDescending(c => c.data_compra).ThenByDescending(c => c.hora_compra))
            {
                _masterListaCompras.Add(new CompraDisplayModel
                {
                    CodCompra = compra.CodCompra,
                    DataCompra = compra.data_compra,
                    ValorTotal = compra.valor_total,
                    Status = GetStatusCompraString(compra.status_compra), // CORRECTED LOGIC
                    NomeFornecedor = compra.CodFornec.HasValue && fornecedoresDict.ContainsKey(compra.CodFornec.Value)
                                     ? fornecedoresDict[compra.CodFornec.Value]
                                     : "Fornecedor n�o encontrado",
                    NomeVendedor = compra.CodVendedor.HasValue && vendedoresDict.ContainsKey(compra.CodVendedor.Value)
                                   ? vendedoresDict[compra.CodVendedor.Value]
                                   : "Vendedor n�o encontrado"
                });
            }

            FilterCompras();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading purchases: {ex.ToString()}");
            await DisplayAlert("Erro", $"N�o foi poss�vel carregar a lista de compras: {ex.Message}", "OK");
        }
    }

    // CORRECTED: This method now handles both byte and string status values robustly.
    private string GetStatusCompraString(object statusValue)
    {
        if (statusValue == null) return "Desconhecido";

        // Try to parse the value as a byte/int
        if (byte.TryParse(statusValue.ToString(), out byte statusByte))
        {
            switch (statusByte)
            {
                case 0: return "Em Processamento";
                case 1: return "Pendente";
                case 2: return "Conclu�da";
                case 3: return "Cancelada";
                default: return "Desconhecido";
            }
        }

        // Fallback for string values, in case the model is still using them
        switch (statusValue.ToString())
        {
            case "Pendente": return "Pendente";
            case "Em Processamento": return "Em Processamento";
            case "Conclu�da": return "Conclu�da";
            case "Cancelada": return "Cancelada";
            default: return statusValue.ToString(); // Show the raw value if it's an unknown string
        }
    }

    private void FilterCompras()
    {
        string searchTerm = CompraSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        var previouslySelectedCode = _compraSelecionada?.CodCompra;

        _listaComprasDisplay.Clear();
        IEnumerable<CompraDisplayModel> filteredList;

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            filteredList = _masterListaCompras;
        }
        else
        {
            filteredList = _masterListaCompras.Where(c =>
                c.CodCompra.ToString().Contains(searchTerm) ||
                (c.NomeFornecedor?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                (c.NomeVendedor?.ToLowerInvariant().Contains(searchTerm) ?? false)
            );
        }

        foreach (var compra in filteredList)
        {
            _listaComprasDisplay.Add(compra);
        }

        if (previouslySelectedCode.HasValue)
        {
            var reselected = _listaComprasDisplay.FirstOrDefault(c => c.CodCompra == previouslySelectedCode.Value);
            if (reselected != null)
            {
                ComprasCollectionView.SelectedItem = reselected;
            }
            else
            {
                _compraSelecionada = null;
                ComprasCollectionView.SelectedItem = null;
            }
        }
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        bool isSelected = _compraSelecionada != null;
        bool canEdit = isSelected && _compraSelecionada.Status != "Conclu�da" && _compraSelecionada.Status != "Cancelada";
        bool canDelete = isSelected;
        bool canGerarConta = isSelected && _compraSelecionada.Status == "Concluída";

        if (!canEdit && isSelected)
        {
            DisplayAlert("Aten��o", "Compra: " + _compraSelecionada.CodCompra + " Ja faturada, N�o � possivel editar", "OK");
        }
        else
        {
            EditarCompraButton.IsEnabled = canEdit;
            ExcluirCompraButton.IsEnabled = canDelete;
        }

        GerarContaPagarButton.IsEnabled = canGerarConta;
    }

    private void ComprasCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _compraSelecionada = e.CurrentSelection.FirstOrDefault() as CompraDisplayModel;
        UpdateActionButtonsState();
    }

    private void CompraSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilterCompras();
    }

    private async void NovaCompraButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var configurator = new Configurator();
            IDbConnection newPageConnection = configurator.GetMySqlConnection();
            if (newPageConnection.State == ConnectionState.Closed) newPageConnection.Open();

            var connectionFactory = new MySqlConnectionFactory();
            var transactionService = new TransactionService(connectionFactory);
            var compraService = new CompraService(newPageConnection);
            var itemCompraService = new ItemCompraService(newPageConnection);
            var fornecedorService = new FornecedorService(newPageConnection);
            var vendedorService = new VendedorService(newPageConnection);
            var produtoService = new ProdutoService(newPageConnection);
            var estoqueService = new EstoqueService(newPageConnection);

            await Navigation.PushAsync(new CadastrodeCompra(compraService, itemCompraService, fornecedorService, vendedorService, produtoService, estoqueService, transactionService, null));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"N�o foi poss�vel abrir a tela de nova compra: {ex.Message}", "OK");
        }
    }

    private async void EditarCompraSelecionadaButton_Clicked(object sender, EventArgs e)
    {
        if (_compraSelecionada == null)
        {
            await DisplayAlert("Nenhuma Compra Selecionada", "Selecione uma compra para editar.", "OK");
            return;
        }

        try
        {
            var configurator = new Configurator();
            IDbConnection editPageConnection = configurator.GetMySqlConnection();
            if (editPageConnection.State == ConnectionState.Closed) editPageConnection.Open();

            var connectionFactory = new MySqlConnectionFactory();
            var transactionService = new TransactionService(connectionFactory);
            var compraService = new CompraService(editPageConnection);
            var itemCompraService = new ItemCompraService(editPageConnection);
            var fornecedorService = new FornecedorService(editPageConnection);
            var vendedorService = new VendedorService(editPageConnection);
            var produtoService = new ProdutoService(editPageConnection);
            var estoqueService = new EstoqueService(editPageConnection);

            await Navigation.PushAsync(new CadastrodeCompra(compraService, itemCompraService, fornecedorService, vendedorService, produtoService, estoqueService, transactionService, _compraSelecionada.CodCompra));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"N�o foi poss�vel abrir a tela de edi��o: {ex.Message}", "OK");
        }
    }

    private async void ExcluirCompraSelecionadaButton_Clicked(object sender, EventArgs e)
    {
        if (_compraSelecionada == null)
        {
            await DisplayAlert("Nenhuma Compra Selecionada", "Selecione uma compra para excluir.", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Confirmar Exclus�o",
            $"ATEN��O: Esta a��o � PERMANENTE e n�o pode ser desfeita. Se a compra foi 'Conclu�da', o estoque N�O ser� revertido automaticamente.\n\nDeseja excluir a compra C�d: {_compraSelecionada.CodCompra}?",
            "Sim, Excluir Permanentemente", "N�o");

        if (confirm)
        {
            try
            {
                await _itemCompraService.DeleteByCompraAsync(_compraSelecionada.CodCompra);
                int rowsAffected = await _compraService.DeleteAsync(_compraSelecionada.CodCompra);

                if (rowsAffected > 0)
                {
                    await DisplayAlert("Sucesso", "Compra e seus itens foram exclu�dos permanentemente.", "OK");
                    await LoadComprasAsync();
                }
                else
                {
                    await DisplayAlert("Erro", "N�o foi poss�vel excluir o registro principal da compra (os itens podem ter sido exclu�dos).", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Ocorreu um erro ao excluir a compra: {ex.Message}", "OK");
            }
        }
    }

    private async void GerarContaPagarButton_Clicked(object sender, EventArgs e)
    {
        if (_compraSelecionada == null)
        {
            await DisplayAlert("Aviso", "Selecione uma compra para gerar a conta a pagar", "OK");
            return;
        }

        if (_compraSelecionada.Status != "Concluída")
        {
            await DisplayAlert("Aviso", "Apenas compras concluídas podem gerar contas a pagar.", "OK");
            return;
        }

        try
        {
            var connectionFactory = new MySqlConnectionFactory();
            var connection = connectionFactory.CreateConnection();
            var contaPagarService = new ContaPagarService(connection);

            // Check if conta already exists for this compra
            var contaExistente = await contaPagarService.GetByCompraAsync(_compraSelecionada.CodCompra);
            if (contaExistente != null)
            {
                await DisplayAlert("Aviso",
                    $"Já existe uma conta a pagar para esta compra (Conta #{contaExistente.Id})",
                    "OK");
                return;
            }

            // Get full compra object
            var compra = await _compraService.GetByIdAsync(_compraSelecionada.CodCompra);

            // Navigate to CadastroContaPagar
            var parcelaPagarService = new ParcelaPagarService(connection, contaPagarService);

            await Navigation.PushAsync(new CadastroContaPagar(
                contaPagarService,
                parcelaPagarService,
                _compraService,
                compra,
                null));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao gerar conta a pagar: {ex.Message}", "OK");
        }
    }
}