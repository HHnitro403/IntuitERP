using IntuitERP.Config;
using IntuitERP.Services;
using System.Collections.ObjectModel;
using System.Data;

namespace IntuitERP.Viwes.Search;

public partial class VendaSearch : ContentPage
{

    public class VendaDisplayModel
    {
        public int CodVenda { get; set; }
        public DateTime? DataVenda { get; set; }
        public decimal? ValorTotal { get; set; }
        public string Status { get; set; }
        public string NomeCliente { get; set; }
        public string NomeVendedor { get; set; }
    }

    private readonly VendaService _vendaService;
    private readonly ItemVendaService _itemVendaService;
    private readonly ClienteService _clienteService;
    private readonly VendedorService _vendedorService;
    private readonly ProdutoService _produtoService; // For navigating to CadastrodeVenda
    private readonly EstoqueService _estoqueService; // For navigating to CadastrodeVenda

    public ObservableCollection<VendaDisplayModel> _listaVendasDisplay { get; set; }
    private List<VendaDisplayModel> _masterListaVendas;
    private VendaDisplayModel _vendaSelecionada;
    public VendaSearch(VendaService vendaService, ItemVendaService itemVendaService, ClienteService clienteService, VendedorService vendedorService, ProdutoService produtoService, EstoqueService estoqueService)
    {
        InitializeComponent();

        _vendaService = vendaService ?? throw new ArgumentNullException(nameof(vendaService));
        _itemVendaService = itemVendaService ?? throw new ArgumentNullException(nameof(itemVendaService));
        _clienteService = clienteService ?? throw new ArgumentNullException(nameof(clienteService));
        _vendedorService = vendedorService ?? throw new ArgumentNullException(nameof(vendedorService));
        _produtoService = produtoService ?? throw new ArgumentNullException(nameof(produtoService));
        _estoqueService = estoqueService ?? throw new ArgumentNullException(nameof(estoqueService));

        _listaVendasDisplay = new ObservableCollection<VendaDisplayModel>();
        _masterListaVendas = new List<VendaDisplayModel>();
        VendasCollectionView.ItemsSource = _listaVendasDisplay;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _vendaSelecionada = null;
        VendasCollectionView.SelectedItem = null;
        await LoadVendasAsync();       
        UpdateActionButtonsState();
    }

    private async Task LoadVendasAsync()
    {
        try
        {
            // Load all necessary data sequentially to avoid connection issues
            var vendas = await _vendaService.GetAllAsync();
            var clientes = await _clienteService.GetAllAsync();
            var vendedores = await _vendedorService.GetAllAsync();

            // Create a lookup dictionary for faster access
            var clientesDict = clientes.ToDictionary(c => c.CodCliente, c => c.Nome);
            var vendedoresDict = vendedores.ToDictionary(v => v.CodVendedor, v => v.NomeVendedor);

            _masterListaVendas.Clear();

            foreach (var venda in vendas.OrderByDescending(v => v.data_venda).ThenByDescending(v => v.hora_venda))
            {
                _masterListaVendas.Add(new VendaDisplayModel
                {
                    CodVenda = venda.CodVenda,
                    DataVenda = venda.data_venda,
                    ValorTotal = venda.valor_total,
                    Status = GetStatusString(venda.status_venda),
                    NomeCliente = venda.CodCliente > 0 && clientesDict.ContainsKey(venda.CodCliente)
                                  ? clientesDict[key: venda.CodCliente]
                                  : "Cliente não encontrado",
                    NomeVendedor = venda.CodVendedor.HasValue && vendedoresDict.ContainsKey(venda.CodVendedor.Value)
                                   ? vendedoresDict[venda.CodVendedor.Value]
                                   : "Vendedor não encontrado"
                });
            }

            FilterVendas();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading sales: {ex.ToString()}");
            await DisplayAlert("Erro", $"Não foi possível carregar a lista de vendas: {ex.Message}", "OK");
        }
    }

    private string GetStatusString(byte? status)
    {
        switch (status)
        {
            case 0: return "Orçamento";
            case 1: return "Pendente";
            case 2: return "Faturada";
            case 3: return "Cancelada";
            default: return "Desconhecido";
        }
    }

    private void FilterVendas()
    {
        string searchTerm = VendaSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        var previouslySelectedCode = _vendaSelecionada?.CodVenda;

        _listaVendasDisplay.Clear();
        IEnumerable<VendaDisplayModel> filteredList;

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            filteredList = _masterListaVendas;
        }
        else
        {
            filteredList = _masterListaVendas.Where(v =>
                v.CodVenda.ToString().Contains(searchTerm) ||
                (v.NomeCliente?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                (v.NomeVendedor?.ToLowerInvariant().Contains(searchTerm) ?? false)
            );
        }

        foreach (var venda in filteredList)
        {
            _listaVendasDisplay.Add(venda);
        }

        if (previouslySelectedCode.HasValue)
        {
            var reselected = _listaVendasDisplay.FirstOrDefault(v => v.CodVenda == previouslySelectedCode.Value);
            if (reselected != null)
            {
                VendasCollectionView.SelectedItem = reselected;
            }
            else
            {
                _vendaSelecionada = null;
                VendasCollectionView.SelectedItem = null;
            }
        }
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        bool isSelected = _vendaSelecionada != null;
        // You may want to prevent editing/deleting of already Faturada/Cancelada sales
        bool canEdit = isSelected && _vendaSelecionada.Status != "Faturada" && _vendaSelecionada.Status != "Cancelada";
        bool canDelete = isSelected; // Or add similar logic for deletion

        if (isSelected && !canEdit)
        {
            DisplayAlert("Atenção", "Esta venda está faturada ou cancelada e não pode ser editada.", "Ok");
        }
    }

    private void VendasCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _vendaSelecionada = e.CurrentSelection.FirstOrDefault() as VendaDisplayModel;
        UpdateActionButtonsState();
    }

    private void VendaSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilterVendas();
    }

    private async void NovaVendaButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var configurator = new Configurator();
            IDbConnection newPageConnection = configurator.GetMySqlConnection();
            if (newPageConnection.State == ConnectionState.Closed) newPageConnection.Open();

            var vendaService = new VendaService(newPageConnection);
            var itemVendaService = new ItemVendaService(newPageConnection);
            var clienteService = new ClienteService(newPageConnection);
            var vendedorService = new VendedorService(newPageConnection);
            var produtoService = new ProdutoService(newPageConnection);
            var estoqueService = new EstoqueService(newPageConnection);

            await Navigation.PushAsync(new CadastrodeVenda(vendaService, itemVendaService, clienteService, vendedorService, produtoService, estoqueService));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível abrir a tela de nova venda: {ex.Message}", "OK");
        }
    }

    private async void EditarVendaSelecionadaButton_Clicked(object sender, EventArgs e)
    {
        if (_vendaSelecionada == null)
        {
            await DisplayAlert("Nenhuma Venda Selecionada", "Selecione uma venda para editar.", "OK");
            return;
        }

        try
        {
            var configurator = new Configurator();
            IDbConnection editPageConnection = configurator.GetMySqlConnection();
            if (editPageConnection.State == ConnectionState.Closed) editPageConnection.Open();

            var vendaService = new VendaService(editPageConnection);
            var itemVendaService = new ItemVendaService(editPageConnection);
            var clienteService = new ClienteService(editPageConnection);
            var vendedorService = new VendedorService(editPageConnection);
            var produtoService = new ProdutoService(editPageConnection);
            var estoqueService = new EstoqueService(editPageConnection);

            // You will need to adapt CadastrodeVenda to accept a CodVenda and load existing data
            await Navigation.PushAsync(new CadastrodeVenda(vendaService, itemVendaService, clienteService, vendedorService, produtoService, estoqueService, _vendaSelecionada.CodVenda));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível abrir a tela de edição: {ex.Message}", "OK");
        }
    }

    private async void ExcluirVendaSelecionadaButton_Clicked(object sender, EventArgs e)
    {
        if (_vendaSelecionada == null)
        {
            await DisplayAlert("Nenhuma Venda Selecionada", "Selecione uma venda para excluir.", "OK");
            return;
        }

        // Note: Deleting a 'Faturada' sale without reversing stock movements can cause data inconsistency.
        // This logic performs a hard delete of the sale and its items.
        bool confirm = await DisplayAlert("Confirmar Exclusão",
            $"ATENÇÃO: Esta ação é PERMANENTE e não pode ser desfeita. Se a venda foi Faturada, o estoque NÃO será revertido automaticamente.\n\nDeseja excluir a venda Cód: {_vendaSelecionada.CodVenda}?",
            "Sim, Excluir Permanentemente", "Não");

        if (confirm)
        {
            try
            {
                // First, delete all associated items from itensvenda
                await _itemVendaService.DeleteByVendaAsync(_vendaSelecionada.CodVenda);

                // Then, delete the main venda record
                int rowsAffected = await _vendaService.DeleteAsync(_vendaSelecionada.CodVenda);

                if (rowsAffected > 0)
                {
                    await DisplayAlert("Sucesso", "Venda e seus itens foram excluídos permanentemente.", "OK");
                    await LoadVendasAsync(); // Refresh the list
                }
                else
                {
                    await DisplayAlert("Erro", "Não foi possível excluir o registro principal da venda (os itens podem ter sido excluídos).", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Ocorreu um erro ao excluir a venda: {ex.Message}", "OK");
            }
        }
    }
}