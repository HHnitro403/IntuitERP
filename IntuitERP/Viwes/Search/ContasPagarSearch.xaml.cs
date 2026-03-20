using IntuitERP.models;
using IntuitERP.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IntuitERP.Viwes.Search;

public partial class ContasPagarSearch : ContentPage, INotifyPropertyChanged
{
    private readonly ContaPagarService _contaService;
    private readonly ParcelaPagarService _parcelaService;
    private readonly CompraService _compraService;
    private readonly SystemSettingsService _settingsService;

    public ObservableCollection<ContaPagarModel> _listaContas { get; set; }
    private List<ContaPagarModel> _masterListaContas;
    private ContaPagarModel _contaSelecionada;

    // Dashboard properties
    private decimal _totalPendente;
    private decimal _totalVencido;
    private decimal _totalParcial;
    private decimal _totalPago;
    private int _countPendente;
    private int _countVencido;
    private int _countParcial;
    private int _countPago;

    public decimal TotalPendente
    {
        get => _totalPendente;
        set { _totalPendente = value; OnPropertyChanged(); }
    }

    public decimal TotalVencido
    {
        get => _totalVencido;
        set { _totalVencido = value; OnPropertyChanged(); }
    }

    public decimal TotalParcial
    {
        get => _totalParcial;
        set { _totalParcial = value; OnPropertyChanged(); }
    }

    public decimal TotalPago
    {
        get => _totalPago;
        set { _totalPago = value; OnPropertyChanged(); }
    }

    public int CountPendente
    {
        get => _countPendente;
        set { _countPendente = value; OnPropertyChanged(); }
    }

    public int CountVencido
    {
        get => _countVencido;
        set { _countVencido = value; OnPropertyChanged(); }
    }

    public int CountParcial
    {
        get => _countParcial;
        set { _countParcial = value; OnPropertyChanged(); }
    }

    public int CountPago
    {
        get => _countPago;
        set { _countPago = value; OnPropertyChanged(); }
    }

    public ContasPagarSearch(
        ContaPagarService contaService,
        ParcelaPagarService parcelaService,
        CompraService compraService,
        SystemSettingsService settingsService)
    {
        InitializeComponent();
        BindingContext = this;

        _contaService = contaService ?? throw new ArgumentNullException(nameof(contaService));
        _parcelaService = parcelaService ?? throw new ArgumentNullException(nameof(parcelaService));
        _compraService = compraService ?? throw new ArgumentNullException(nameof(compraService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        _listaContas = new ObservableCollection<ContaPagarModel>();
        _masterListaContas = new List<ContaPagarModel>();
        ContasCollectionView.ItemsSource = _listaContas;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _contaSelecionada = null;
        ContasCollectionView.SelectedItem = null;
        await LoadContasAsync();
        UpdateActionButtonsState();
    }

    private async Task LoadContasAsync()
    {
        try
        {
            var contas = await _contaService.GetAllAsync();

            _masterListaContas.Clear();
            _listaContas.Clear();

            foreach (var conta in contas.OrderByDescending(c => c.DataEmissao))
            {
                _masterListaContas.Add(conta);
                _listaContas.Add(conta);
            }

            // Update dashboard
            await UpdateDashboardAsync(contas);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar contas: {ex.Message}", "OK");
        }
    }

    private async Task UpdateDashboardAsync(List<ContaPagarModel> contas)
    {
        try
        {
            // Calculate summary from contas list
            TotalPendente = contas.Where(c => c.Status == "Pendente").Sum(c => c.ValorPendente);
            TotalVencido = contas.Where(c => c.Status == "Vencido").Sum(c => c.ValorPendente);
            TotalParcial = contas.Where(c => c.Status == "Parcial").Sum(c => c.ValorPendente);
            TotalPago = 0; // Paid accounts have ValorPendente = 0

            CountPendente = contas.Count(c => c.Status == "Pendente");
            CountVencido = contas.Count(c => c.Status == "Vencido");
            CountParcial = contas.Count(c => c.Status == "Parcial");
            CountPago = contas.Count(c => c.Status == "Pago");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao atualizar dashboard: {ex.Message}", "OK");
        }
    }

    private void ContasSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.ToLower() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            // Show all
            _listaContas.Clear();
            foreach (var conta in _masterListaContas)
            {
                _listaContas.Add(conta);
            }
        }
        else
        {
            // Filter
            _listaContas.Clear();
            var filtered = _masterListaContas.Where(c =>
                (c.FornecedorNome?.ToLower().Contains(searchText) ?? false) ||
                c.CodCompra.ToString().Contains(searchText) ||
                c.ValorTotal.ToString().Contains(searchText) ||
                c.Id.ToString().Contains(searchText)
            );

            foreach (var conta in filtered)
            {
                _listaContas.Add(conta);
            }
        }
    }

    private void ContasCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _contaSelecionada = e.CurrentSelection.FirstOrDefault() as ContaPagarModel;
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        bool hasSelection = _contaSelecionada != null;
        VerParcelasButton.IsEnabled = hasSelection;
        EditarContaButton.IsEnabled = hasSelection && _contaSelecionada?.Status != "Pago";
        ExcluirContaButton.IsEnabled = hasSelection && _contaSelecionada?.ValorPago == 0;
    }

    private async void NovaContaButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            await DisplayAlert("Aviso",
                "Para criar uma conta a pagar, acesse a tela de Compras e use o botão 'Gerar Conta a Pagar' em uma compra concluída.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro: {ex.Message}", "OK");
        }
    }

    private async void VerParcelasButton_Clicked(object sender, EventArgs e)
    {
        if (_contaSelecionada == null) return;

        try
        {
            var parcelas = await _parcelaService.GetByContaAsync(_contaSelecionada.Id);

            if (parcelas == null || parcelas.Count == 0)
            {
                await DisplayAlert("Aviso", "Esta conta não possui parcelas cadastradas.", "OK");
                return;
            }

            // Navigate to VerParcelasConta page (will be created separately for Pagar)
            await DisplayAlert("Em Desenvolvimento",
                "A visualização detalhada de parcelas a pagar está em desenvolvimento.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar parcelas: {ex.Message}", "OK");
        }
    }

    private async void EditarContaButton_Clicked(object sender, EventArgs e)
    {
        if (_contaSelecionada == null) return;

        try
        {
            // Will be implemented with CadastroContaPagar
            await DisplayAlert("Em Desenvolvimento",
                "A edição de contas a pagar está em desenvolvimento.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao editar conta: {ex.Message}", "OK");
        }
    }

    private async void ExcluirContaButton_Clicked(object sender, EventArgs e)
    {
        if (_contaSelecionada == null) return;

        try
        {
            bool confirm = await DisplayAlert(
                "Confirmar Exclusão",
                $"Deseja realmente excluir a conta #{_contaSelecionada.Id}?\n" +
                $"Fornecedor: {_contaSelecionada.FornecedorNome}\n" +
                $"Valor: R$ {_contaSelecionada.ValorTotal:N2}",
                "Sim", "Não");

            if (!confirm) return;

            await _contaService.DeleteAsync(_contaSelecionada.Id);
            await DisplayAlert("Sucesso", "Conta excluída com sucesso!", "OK");

            // Reload list
            await LoadContasAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao excluir conta: {ex.Message}", "OK");
        }
    }

    public new event PropertyChangedEventHandler PropertyChanged;
    protected new void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
