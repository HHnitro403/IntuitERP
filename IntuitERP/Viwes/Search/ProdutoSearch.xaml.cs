using IntuitERP.Config;
using IntuitERP.models;
using IntuitERP.Services;
using System.Collections.ObjectModel;
using System.Data;

namespace IntuitERP.Viwes.Search;

public partial class ProdutoSearch : ContentPage
{
    private readonly ProdutoService _produtoService;
    private readonly FornecedorService _fornecedorService; // Needed for navigating to CadastroProduto

    public ObservableCollection<ProdutoModel> _listaProdutosDisplay { get; set; }
    private List<ProdutoModel> _masterListaProdutos;
    private ProdutoModel _produtoSelecionado;
    public ProdutoSearch(ProdutoService produtoService, FornecedorService fornecedorService)
    {
        InitializeComponent();

        if (produtoService == null)
            throw new ArgumentNullException(nameof(produtoService), "ProdutoService must be provided.");
        if (fornecedorService == null)
            throw new ArgumentNullException(nameof(fornecedorService), "FornecedorService must be provided for navigation.");

        _produtoService = produtoService;
        _fornecedorService = fornecedorService;

        _listaProdutosDisplay = new ObservableCollection<ProdutoModel>();
        _masterListaProdutos = new List<ProdutoModel>();
        ProdutosCollectionView.ItemsSource = _listaProdutosDisplay;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProdutosAsync();
        _produtoSelecionado = null;
        ProdutosCollectionView.SelectedItem = null;
        UpdateActionButtonsState();
    }

    private async Task LoadProdutosAsync()
    {
        if (_produtoService == null)
        {
            await DisplayAlert("Erro", "Serviço de produto não inicializado.", "OK");
            return;
        }

        try
        {
            var produtos = await _produtoService.GetAllAsync();
            _masterListaProdutos = new List<ProdutoModel>(produtos.OrderBy(p => p.Descricao));
            FilterProdutos();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading products: {ex.ToString()}");
            await DisplayAlert("Erro", $"Não foi possível carregar a lista de produtos: {ex.Message}", "OK");
        }
    }

    private void FilterProdutos()
    {
        string searchTerm = ProdutoSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        var previouslySelectedProdutoCode = _produtoSelecionado?.CodProduto;

        _listaProdutosDisplay.Clear();
        IEnumerable<ProdutoModel> filteredList;

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            filteredList = _masterListaProdutos;
        }
        else
        {
            filteredList = _masterListaProdutos.Where(p =>
                (p.Descricao?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                (p.Categoria?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                (p.Tipo?.ToLowerInvariant().Contains(searchTerm) ?? false)
            );
        }

        foreach (var produto in filteredList)
        {
            _listaProdutosDisplay.Add(produto);
        }

        if (previouslySelectedProdutoCode.HasValue)
        {
            var reselectedProduto = _listaProdutosDisplay.FirstOrDefault(p => p.CodProduto == previouslySelectedProdutoCode.Value);
            if (reselectedProduto != null)
            {
                ProdutosCollectionView.SelectedItem = reselectedProduto;
            }
            else
            {
                _produtoSelecionado = null;
                ProdutosCollectionView.SelectedItem = null;
            }
        }
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        bool isProdutoSelected = _produtoSelecionado != null;
        EditarProdutoButton.IsEnabled = isProdutoSelected;
        ExcluirProdutoButton.IsEnabled = isProdutoSelected;
    }

    private void ProdutosCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _produtoSelecionado = e.CurrentSelection.FirstOrDefault() as ProdutoModel;
        UpdateActionButtonsState();
    }

    private void ProdutoSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilterProdutos();
    }

    private async void NovoProdutoButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var configurator = new Configurator();
            IDbConnection newPageConnection = configurator.GetMySqlConnection();
            if (newPageConnection.State == ConnectionState.Closed) newPageConnection.Open();

            var produtoServiceForNewPage = new ProdutoService(newPageConnection);
            var fornecedorServiceForNewPage = new FornecedorService(newPageConnection);

            // Pass 0 or no ID for a new product, assuming CadastroProduto handles this
            await Navigation.PushAsync(new CadastroProduto(produtoServiceForNewPage, fornecedorServiceForNewPage, 0));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error navigating to New Product: {ex.ToString()}");
            await DisplayAlert("Erro", $"Não foi possível abrir a tela de cadastro de produto: {ex.Message}", "OK");
        }
    }

    private async void EditarProdutoSelecionadoButton_Clicked(object sender, EventArgs e)
    {
        if (_produtoSelecionado == null)
        {
            await DisplayAlert("Nenhum Produto Selecionado", "Por favor, selecione um produto da lista para editar.", "OK");
            return;
        }

        try
        {
            var configurator = new Configurator();
            IDbConnection editPageConnection = configurator.GetMySqlConnection();
            if (editPageConnection.State == ConnectionState.Closed) editPageConnection.Open();

            var produtoServiceForEditPage = new ProdutoService(editPageConnection);
            var fornecedorServiceForEditPage = new FornecedorService(editPageConnection);

            // Pass the ID of the selected product to CadastroProduto
            await Navigation.PushAsync(new CadastroProduto(produtoServiceForEditPage, fornecedorServiceForEditPage, _produtoSelecionado.CodProduto));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error navigating to Edit Product: {ex.ToString()}");
            await DisplayAlert("Erro", $"Não foi possível abrir a tela de edição de produto: {ex.Message}", "OK");
        }
    }

    private async void ExcluirProdutoSelecionadoButton_Clicked(object sender, EventArgs e)
    {
        if (_produtoSelecionado == null)
        {
            await DisplayAlert("Nenhum Produto Selecionado", "Por favor, selecione um produto da lista para excluir.", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Confirmar Exclusão",
            $"Tem certeza que deseja marcar o produto '{_produtoSelecionado.Descricao}' como excluído? (Esta ação não remove o registro, apenas o inativa)",
            "Sim, Excluir", "Não");

        if (confirm)
        {
            if (_produtoService == null)
            {
                await DisplayAlert("Erro", "Serviço de produto não inicializado.", "OK");
                return;
            }
            try
            {
                _produtoSelecionado.Ativo = false; // Soft delete
                int rowsAffected = await _produtoService.UpdateAsync(_produtoSelecionado);

                if (rowsAffected > 0)
                {
                    await DisplayAlert("Sucesso", "Produto marcado como excluído.", "OK");

                    var masterItem = _masterListaProdutos.FirstOrDefault(p => p.CodProduto == _produtoSelecionado.CodProduto);
                    if (masterItem != null) masterItem.Ativo = false;

                    FilterProdutos(); // Refreshes the display

                    _produtoSelecionado = null;
                    ProdutosCollectionView.SelectedItem = null; // Deselect
                    UpdateActionButtonsState();
                }
                else
                {
                    _produtoSelecionado.Ativo = true; // Revert optimistic update
                    await DisplayAlert("Erro", "Não foi possível atualizar o status do produto.", "OK");
                }
            }
            catch (Exception ex)
            {
                if (_produtoSelecionado != null) _produtoSelecionado.Ativo = true; // Revert
                Console.WriteLine($"Error excluding product: {ex.ToString()}");
                await DisplayAlert("Erro", $"Ocorreu um erro ao excluir o produto: {ex.Message}", "OK");
            }
        }
    }
}