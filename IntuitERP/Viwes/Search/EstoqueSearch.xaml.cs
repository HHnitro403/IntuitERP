using IntuitERP.Config;
using IntuitERP.models;
using IntuitERP.Services;
using System.Collections.ObjectModel;
using System.Data;

namespace IntuitERP.Viwes.Search;

public partial class EstoqueSearch : ContentPage
{
    private readonly EstoqueService _estoqueService;
    private readonly ProdutoService _produtoService;

    public ObservableCollection<EstoqueDisplayModel> _listaEstoqueDisplay { get; set; }
    private List<EstoqueDisplayModel> _masterListaEstoque;
    private EstoqueDisplayModel _estoqueSelecionado;
    private char? _filtroTipoAtivo = null; // null = todos, 'E' = entrada, 'S' = saída

    public EstoqueSearch(EstoqueService estoqueService, ProdutoService produtoService)
    {
        InitializeComponent();

        if (estoqueService == null)
            throw new ArgumentNullException(nameof(estoqueService), "EstoqueService must be provided.");
        if (produtoService == null)
            throw new ArgumentNullException(nameof(produtoService), "ProdutoService must be provided.");

        _estoqueService = estoqueService;
        _produtoService = produtoService;

        _listaEstoqueDisplay = new ObservableCollection<EstoqueDisplayModel>();
        _masterListaEstoque = new List<EstoqueDisplayModel>();
        EstoqueCollectionView.ItemsSource = _listaEstoqueDisplay;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadEstoqueAsync();
        _estoqueSelecionado = null;
        EstoqueCollectionView.SelectedItem = null;
        UpdateActionButtonsState();
    }

    private async Task LoadEstoqueAsync()
    {
        if (_estoqueService == null || _produtoService == null)
        {
            await DisplayAlert("Erro", "Serviços não inicializados.", "OK");
            return;
        }

        try
        {
            var estoques = await _estoqueService.GetAllAsync();
            var produtos = await _produtoService.GetAllAsync();

            // Create a dictionary for fast lookup
            var produtosDict = produtos.ToDictionary(p => p.CodProduto, p => p.Descricao);

            _masterListaEstoque = estoques
                .OrderByDescending(e => e.Data)
                .ThenByDescending(e => e.CodEst)
                .Select(e => new EstoqueDisplayModel
                {
                    CodEst = e.CodEst,
                    CodProduto = e.CodProduto,
                    Tipo = e.Tipo ?? 'E',
                    TipoDescricao = e.Tipo == 'E' ? "ENTRADA" : "SAÍDA",
                    Qtd = e.Qtd ?? 0,
                    Data = e.Data,
                    ProdutoNome = produtosDict.ContainsKey(e.CodProduto)
                        ? produtosDict[e.CodProduto]
                        : "Produto não encontrado"
                })
                .ToList();

            FilterEstoque();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading stock movements: {ex.ToString()}");
            await DisplayAlert("Erro", $"Não foi possível carregar a lista de movimentações: {ex.Message}", "OK");
        }
    }

    private void FilterEstoque()
    {
        string searchTerm = EstoqueSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        var previouslySelectedEstoqueCode = _estoqueSelecionado?.CodEst;

        _listaEstoqueDisplay.Clear();
        IEnumerable<EstoqueDisplayModel> filteredList = _masterListaEstoque;

        // Filter by type (E/S/All)
        if (_filtroTipoAtivo.HasValue)
        {
            filteredList = filteredList.Where(e => e.Tipo == _filtroTipoAtivo.Value);
        }

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            filteredList = filteredList.Where(e =>
                (e.ProdutoNome?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                (e.TipoDescricao?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                (e.CodProduto.ToString().Contains(searchTerm)) ||
                (e.CodEst.ToString().Contains(searchTerm)) ||
                (e.Data.ToString("dd/MM/yyyy").Contains(searchTerm))
            );
        }

        foreach (var estoque in filteredList)
        {
            _listaEstoqueDisplay.Add(estoque);
        }

        if (previouslySelectedEstoqueCode.HasValue)
        {
            var reselectedEstoque = _listaEstoqueDisplay.FirstOrDefault(e => e.CodEst == previouslySelectedEstoqueCode.Value);
            if (reselectedEstoque != null)
            {
                EstoqueCollectionView.SelectedItem = reselectedEstoque;
            }
            else
            {
                _estoqueSelecionado = null;
                EstoqueCollectionView.SelectedItem = null;
            }
        }
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        bool isEstoqueSelected = _estoqueSelecionado != null;
        EditarEstoqueButton.IsEnabled = isEstoqueSelected;
        ExcluirEstoqueButton.IsEnabled = isEstoqueSelected;
    }

    private void UpdateFilterButtonsState()
    {
        // Reset all buttons to default state
        FiltroTodosButton.BackgroundColor = Color.FromArgb("#55A2D3");
        FiltroEntradaButton.BackgroundColor = Colors.DarkGray;
        FiltroSaidaButton.BackgroundColor = Colors.DarkGray;

        // Highlight active filter
        if (!_filtroTipoAtivo.HasValue)
        {
            FiltroTodosButton.BackgroundColor = Color.FromArgb("#1B4F83");
        }
        else if (_filtroTipoAtivo.Value == 'E')
        {
            FiltroEntradaButton.BackgroundColor = Color.FromArgb("#28A745");
        }
        else if (_filtroTipoAtivo.Value == 'S')
        {
            FiltroSaidaButton.BackgroundColor = Color.FromArgb("#DC3545");
        }
    }

    private void EstoqueCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _estoqueSelecionado = e.CurrentSelection.FirstOrDefault() as EstoqueDisplayModel;
        UpdateActionButtonsState();
    }

    private void EstoqueSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilterEstoque();
    }

    private void FiltroTodos_Clicked(object sender, EventArgs e)
    {
        _filtroTipoAtivo = null;
        UpdateFilterButtonsState();
        FilterEstoque();
    }

    private void FiltroEntrada_Clicked(object sender, EventArgs e)
    {
        _filtroTipoAtivo = 'E';
        UpdateFilterButtonsState();
        FilterEstoque();
    }

    private void FiltroSaida_Clicked(object sender, EventArgs e)
    {
        _filtroTipoAtivo = 'S';
        UpdateFilterButtonsState();
        FilterEstoque();
    }

    private async void NovoEstoqueButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var configurator = new Configurator();
            IDbConnection newPageConnection = configurator.GetMySqlConnection();
            if (newPageConnection.State == ConnectionState.Closed) newPageConnection.Open();

            var estoqueServiceForNewPage = new EstoqueService(newPageConnection);
            var produtoServiceForNewPage = new ProdutoService(newPageConnection);
            var cadastroEstoquePage = new CadstroEstoque(estoqueServiceForNewPage, produtoServiceForNewPage, 0);
            cadastroEstoquePage.Title = "Nova Movimentação de Estoque";

            await Navigation.PushAsync(cadastroEstoquePage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error navigating to New Stock Movement: {ex.ToString()}");
            await DisplayAlert("Erro", $"Não foi possível abrir a tela de movimentação de estoque: {ex.Message}", "OK");
        }
    }

    private async void EditarEstoqueSelecionadoButton_Clicked(object sender, EventArgs e)
    {
        if (_estoqueSelecionado == null)
        {
            await DisplayAlert("Nenhuma Movimentação Selecionada", "Por favor, selecione uma movimentação da lista para editar.", "OK");
            return;
        }

        try
        {
            var configurator = new Configurator();
            IDbConnection editPageConnection = configurator.GetMySqlConnection();
            if (editPageConnection.State == ConnectionState.Closed) editPageConnection.Open();

            var estoqueServiceForEditPage = new EstoqueService(editPageConnection);
            var produtoServiceForEditPage = new ProdutoService(editPageConnection);
            var cadastroEstoquePage = new CadstroEstoque(estoqueServiceForEditPage, produtoServiceForEditPage, _estoqueSelecionado.CodEst);
            cadastroEstoquePage.Title = $"Editar Movimentação #{_estoqueSelecionado.CodEst}";

            await Navigation.PushAsync(cadastroEstoquePage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error navigating to Edit Stock Movement: {ex.ToString()}");
            await DisplayAlert("Erro", $"Não foi possível abrir a tela de edição: {ex.Message}", "OK");
        }
    }

    private async void ExcluirEstoqueSelecionadoButton_Clicked(object sender, EventArgs e)
    {
        if (_estoqueSelecionado == null)
        {
            await DisplayAlert("Nenhuma Movimentação Selecionada", "Por favor, selecione uma movimentação da lista para excluir.", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Confirmar Exclusão",
            $"Tem certeza que deseja excluir a movimentação #{_estoqueSelecionado.CodEst}?\n\n" +
            $"Produto: {_estoqueSelecionado.ProdutoNome}\n" +
            $"Tipo: {_estoqueSelecionado.TipoDescricao}\n" +
            $"Quantidade: {_estoqueSelecionado.Qtd:N2}\n\n" +
            $"ATENÇÃO: Esta ação irá reverter o estoque do produto!",
            "Sim, Excluir", "Não");

        if (confirm)
        {
            if (_estoqueService == null || _produtoService == null)
            {
                await DisplayAlert("Erro", "Serviços não inicializados.", "OK");
                return;
            }
            try
            {
                // First, reverse the stock movement
                int multiplier = _estoqueSelecionado.Tipo == 'E' ? -1 : 1;
                await _produtoService.AtualizarEstoqueAsync(
                    _estoqueSelecionado.CodProduto,
                    (int)(_estoqueSelecionado.Qtd * multiplier)
                );

                // Then delete the stock movement record
                int rowsAffected = await _estoqueService.DeleteAsync(_estoqueSelecionado.CodEst);

                if (rowsAffected > 0)
                {
                    await DisplayAlert("Sucesso", "Movimentação excluída e estoque revertido.", "OK");

                    // Remove from master list
                    var masterItem = _masterListaEstoque.FirstOrDefault(e => e.CodEst == _estoqueSelecionado.CodEst);
                    if (masterItem != null)
                    {
                        _masterListaEstoque.Remove(masterItem);
                    }

                    FilterEstoque(); // Refresh the display

                    _estoqueSelecionado = null;
                    EstoqueCollectionView.SelectedItem = null;
                    UpdateActionButtonsState();
                }
                else
                {
                    await DisplayAlert("Erro", "Não foi possível excluir a movimentação.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting stock movement: {ex.ToString()}");
                await DisplayAlert("Erro", $"Ocorreu um erro ao excluir a movimentação: {ex.Message}", "OK");
            }
        }
    }

    // Display model for the list view
    public class EstoqueDisplayModel
    {
        public int CodEst { get; set; }
        public int CodProduto { get; set; }
        public char Tipo { get; set; }
        public string TipoDescricao { get; set; }
        public decimal Qtd { get; set; }
        public DateTime Data { get; set; }
        public string ProdutoNome { get; set; }
    }
}
