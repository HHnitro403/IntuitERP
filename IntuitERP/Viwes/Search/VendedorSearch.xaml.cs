using IntuitERP.Config;
using IntuitERP.models;
using IntuitERP.Services;
using System.Collections.ObjectModel;
using System.Data;

namespace IntuitERP.Viwes.Search;

public partial class VendedorSearch : ContentPage
{
    private readonly VendedorService _vendedorService;

    public ObservableCollection<VendedorModel> _listaVendedoresDisplay { get; set; }
    private List<VendedorModel> _masterListaVendedores;
    private VendedorModel _vendedorSelecionado;
    public VendedorSearch(VendedorService vendedorService)
    {
        InitializeComponent();

        if (vendedorService == null)
            throw new ArgumentNullException(nameof(vendedorService));

        _vendedorService = vendedorService;

        _listaVendedoresDisplay = new ObservableCollection<VendedorModel>();
        _masterListaVendedores = new List<VendedorModel>();
        VendedoresCollectionView.ItemsSource = _listaVendedoresDisplay;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadVendedoresAsync();
        _vendedorSelecionado = null;
        VendedoresCollectionView.SelectedItem = null;
        UpdateActionButtonsState();
    }

    private async Task LoadVendedoresAsync()
    {
        try
        {
            var vendedores = await _vendedorService.GetAllAsync();
            _masterListaVendedores = new List<VendedorModel>(vendedores.OrderBy(v => v.NomeVendedor));
            FilterVendedores();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading salespeople: {ex.ToString()}");
            await DisplayAlert("Erro", $"Não foi possível carregar a lista de vendedores: {ex.Message}", "OK");
        }
    }

    private void FilterVendedores()
    {
        string searchTerm = VendedorSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        var previouslySelectedCode = _vendedorSelecionado?.CodVendedor;

        _listaVendedoresDisplay.Clear();
        IEnumerable<VendedorModel> filteredList;

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            filteredList = _masterListaVendedores;
        }
        else
        {
            filteredList = _masterListaVendedores.Where(v =>
                v.NomeVendedor?.ToLowerInvariant().Contains(searchTerm) ?? false
            );
        }

        foreach (var vendedor in filteredList)
        {
            _listaVendedoresDisplay.Add(vendedor);
        }

        if (previouslySelectedCode.HasValue)
        {
            var reselected = _listaVendedoresDisplay.FirstOrDefault(v => v.CodVendedor == previouslySelectedCode.Value);
            if (reselected != null)
            {
                VendedoresCollectionView.SelectedItem = reselected;
            }
            else
            {
                _vendedorSelecionado = null;
                VendedoresCollectionView.SelectedItem = null;
            }
        }
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        bool isSelected = _vendedorSelecionado != null;
        EditarVendedorButton.IsEnabled = isSelected;
        ExcluirVendedorButton.IsEnabled = isSelected;
    }

    private void VendedoresCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _vendedorSelecionado = e.CurrentSelection.FirstOrDefault() as VendedorModel;
        UpdateActionButtonsState();
    }

    private void VendedorSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilterVendedores();
    }

    private async void NovoVendedorButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var configurator = new Configurator();
            IDbConnection newPageConnection = configurator.GetMySqlConnection();
            if (newPageConnection.State == ConnectionState.Closed) newPageConnection.Open();

            var vendedorServiceForNewPage = new VendedorService(newPageConnection);

            // Assuming CadastrodeVendedor's constructor can handle a null model for a new entry
            await Navigation.PushAsync(new CadastrodeVendedor(vendedorServiceForNewPage, 0));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error navigating to New Salesperson: {ex.ToString()}");
            await DisplayAlert("Erro", $"Não foi possível abrir a tela de cadastro: {ex.Message}", "OK");
        }
    }

    private async void EditarVendedorSelecionadoButton_Clicked(object sender, EventArgs e)
    {
        if (_vendedorSelecionado == null)
        {
            await DisplayAlert("Nenhum Vendedor Selecionado", "Por favor, selecione um vendedor para editar.", "OK");
            return;
        }

        try
        {
            var configurator = new Configurator();
            IDbConnection editPageConnection = configurator.GetMySqlConnection();
            if (editPageConnection.State == ConnectionState.Closed) editPageConnection.Open();

            var vendedorServiceForEditPage = new VendedorService(editPageConnection);

            // You'll need to adapt CadastrodeVendedor to accept a VendedorModel
            await Navigation.PushAsync(new CadastrodeVendedor(vendedorServiceForEditPage, _vendedorSelecionado.CodVendedor));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error navigating to Edit Salesperson: {ex.ToString()}");
            await DisplayAlert("Erro", $"Não foi possível abrir a tela de edição: {ex.Message}", "OK");
        }
    }

    private async void ExcluirVendedorSelecionadoButton_Clicked(object sender, EventArgs e)
    {
        if (_vendedorSelecionado == null)
        {
            await DisplayAlert("Nenhum Vendedor Selecionado", "Por favor, selecione um vendedor para excluir.", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Confirmar Exclusão",
            $"ATENÇÃO: Esta ação é PERMANENTE e não pode ser desfeita.\n\nTem certeza que deseja excluir o vendedor '{_vendedorSelecionado.NomeVendedor}'?",
            "Sim, Excluir Permanentemente", "Não");

        if (confirm)
        {
            try
            {
                int rowsAffected = await _vendedorService.DeleteAsync(_vendedorSelecionado.CodVendedor);

                if (rowsAffected > 0)
                {
                    await DisplayAlert("Sucesso", "Vendedor excluído permanentemente.", "OK");

                    _masterListaVendedores.RemoveAll(v => v.CodVendedor == _vendedorSelecionado.CodVendedor);
                    FilterVendedores();

                    _vendedorSelecionado = null;
                    VendedoresCollectionView.SelectedItem = null;
                    UpdateActionButtonsState();
                }
                else
                {
                    await DisplayAlert("Erro", "Não foi possível excluir o vendedor.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error excluding salesperson: {ex.ToString()}");
                // Check for foreign key constraint violation
                if (ex.Message.Contains("foreign key constraint fails"))
                {
                    await DisplayAlert("Erro de Exclusão", "Não é possível excluir este vendedor pois ele está associado a vendas ou outras operações.", "OK");
                }
                else
                {
                    await DisplayAlert("Erro", $"Ocorreu um erro ao excluir o vendedor: {ex.Message}", "OK");
                }
            }
        }
    }
}