using IntuitERP.Config;
using IntuitERP.models;
using IntuitERP.Services;
using System.Collections.ObjectModel;
using System.Data;

namespace IntuitERP.Viwes.Search;

public partial class FornecedorSearch : ContentPage
{
    private readonly FornecedorService _fornecedorService;
    private readonly CidadeService _cidadeService; // Needed for navigating to CadastrodeFornecedor

    public ObservableCollection<FornecedorModel> _listaFornecedoresDisplay { get; set; }
    private List<FornecedorModel> _masterListaFornecedores;
    private FornecedorModel _fornecedorSelecionado;

    public FornecedorSearch(FornecedorService fornecedorService, CidadeService cidadeService)
    {
        InitializeComponent();

        if (fornecedorService == null)
            throw new ArgumentNullException(nameof(fornecedorService));
        if (cidadeService == null)
            throw new ArgumentNullException(nameof(cidadeService));

        _fornecedorService = fornecedorService;
        _cidadeService = cidadeService;

        _listaFornecedoresDisplay = new ObservableCollection<FornecedorModel>();
        _masterListaFornecedores = new List<FornecedorModel>();
        FornecedoresCollectionView.ItemsSource = _listaFornecedoresDisplay;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadFornecedoresAsync();
        _fornecedorSelecionado = null;
        FornecedoresCollectionView.SelectedItem = null;
        UpdateActionButtonsState();
    }

    private async Task LoadFornecedoresAsync()
    {
        try
        {
            var fornecedores = await _fornecedorService.GetAllAsync();
            _masterListaFornecedores = new List<FornecedorModel>(fornecedores.OrderBy(f => f.NomeFantasia ?? f.RazaoSocial));
            FilterFornecedores();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading suppliers: {ex.ToString()}");
            await DisplayAlert("Erro", $"Não foi possível carregar a lista de fornecedores: {ex.Message}", "OK");
        }
    }

    private void FilterFornecedores()
    {
        string searchTerm = FornecedorSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        var previouslySelectedCode = _fornecedorSelecionado?.CodFornecedor;

        _listaFornecedoresDisplay.Clear();
        IEnumerable<FornecedorModel> filteredList;

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            filteredList = _masterListaFornecedores;
        }
        else
        {
            filteredList = _masterListaFornecedores.Where(f =>
                (f.RazaoSocial?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                (f.NomeFantasia?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                (f.CNPJ?.ToLowerInvariant().Contains(searchTerm) ?? false)
            );
        }

        foreach (var fornecedor in filteredList)
        {
            _listaFornecedoresDisplay.Add(fornecedor);
        }

        if (previouslySelectedCode.HasValue)
        {
            var reselected = _listaFornecedoresDisplay.FirstOrDefault(f => f.CodFornecedor == previouslySelectedCode.Value);
            if (reselected != null)
            {
                FornecedoresCollectionView.SelectedItem = reselected;
            }
            else
            {
                _fornecedorSelecionado = null;
                FornecedoresCollectionView.SelectedItem = null;
            }
        }
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        bool isSelected = _fornecedorSelecionado != null;
        EditarFornecedorButton.IsEnabled = isSelected;
        ExcluirFornecedorButton.IsEnabled = isSelected;
    }

    private void FornecedoresCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _fornecedorSelecionado = e.CurrentSelection.FirstOrDefault() as FornecedorModel;
        UpdateActionButtonsState();
    }

    private void FornecedorSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilterFornecedores();
    }

    private async void NovoFornecedorButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var configurator = new Configurator();
            IDbConnection newPageConnection = configurator.GetMySqlConnection();
            if (newPageConnection.State == ConnectionState.Closed) newPageConnection.Open();

            var fornecedorServiceForNewPage = new FornecedorService(newPageConnection);
            var cidadeServiceForNewPage = new CidadeService(newPageConnection);

            // Assuming CadastrodeFornecedor constructor can handle a null model for new entries
            await Navigation.PushAsync(new CadastrodeFornecedor(fornecedorServiceForNewPage, cidadeServiceForNewPage));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error navigating to New Supplier: {ex.ToString()}");
            await DisplayAlert("Erro", $"Não foi possível abrir a tela de cadastro: {ex.Message}", "OK");
        }
    }

    private async void EditarFornecedorSelecionadoButton_Clicked(object sender, EventArgs e)
    {
        if (_fornecedorSelecionado == null)
        {
            await DisplayAlert("Nenhum Fornecedor Selecionado", "Por favor, selecione um fornecedor para editar.", "OK");
            return;
        }

        try
        {
            var configurator = new Configurator();
            IDbConnection editPageConnection = configurator.GetMySqlConnection();
            if (editPageConnection.State == ConnectionState.Closed) editPageConnection.Open();

            var fornecedorServiceForEditPage = new FornecedorService(editPageConnection);
            var cidadeServiceForEditPage = new CidadeService(editPageConnection);

            // You'll need to adapt CadastrodeFornecedor to accept a FornecedorModel
            // and populate its fields for editing.
            await Navigation.PushAsync(new CadastrodeFornecedor(fornecedorServiceForEditPage, cidadeServiceForEditPage, _fornecedorSelecionado.CodFornecedor));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error navigating to Edit Supplier: {ex.ToString()}");
            await DisplayAlert("Erro", $"Não foi possível abrir a tela de edição: {ex.Message}", "OK");
        }
    }

    private async void ExcluirFornecedorSelecionadoButton_Clicked(object sender, EventArgs e)
    {
        if (_fornecedorSelecionado == null)
        {
            await DisplayAlert("Nenhum Fornecedor Selecionado", "Por favor, selecione um fornecedor para excluir.", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Confirmar Exclusão",
            $"Tem certeza que deseja marcar o fornecedor '{_fornecedorSelecionado.NomeFantasia}' como excluído?",
            "Sim, Excluir", "Não");

        if (confirm)
        {
            try
            {
                _fornecedorSelecionado.Ativo = false; // Soft delete
                int rowsAffected = await _fornecedorService.UpdateAsync(_fornecedorSelecionado);

                if (rowsAffected > 0)
                {
                    await DisplayAlert("Sucesso", "Fornecedor marcado como excluído.", "OK");

                    var masterItem = _masterListaFornecedores.FirstOrDefault(f => f.CodFornecedor == _fornecedorSelecionado.CodFornecedor);
                    if (masterItem != null) masterItem.Ativo = false;

                    FilterFornecedores();

                    _fornecedorSelecionado = null;
                    FornecedoresCollectionView.SelectedItem = null;
                    UpdateActionButtonsState();
                }
                else
                {
                    _fornecedorSelecionado.Ativo = true; // Revert optimistic update
                    await DisplayAlert("Erro", "Não foi possível atualizar o status do fornecedor.", "OK");
                }
            }
            catch (Exception ex)
            {
                if (_fornecedorSelecionado != null) _fornecedorSelecionado.Ativo = true;
                Console.WriteLine($"Error excluding supplier: {ex.ToString()}");
                await DisplayAlert("Erro", $"Ocorreu um erro ao excluir o fornecedor: {ex.Message}", "OK");
            }
        }
    }

}