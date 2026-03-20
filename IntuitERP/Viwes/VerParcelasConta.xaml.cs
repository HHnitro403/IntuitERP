using IntuitERP.models;
using IntuitERP.Services;
using IntuitERP.Config;
using System.Collections.ObjectModel;

namespace IntuitERP.Viwes;

public partial class VerParcelasConta : ContentPage
{
    private readonly ContaReceberService _contaService;
    private readonly ParcelaReceberService _parcelaService;
    private readonly SystemSettingsService _settingsService;
    private readonly VendaService _vendaService;

    private ContaReceberModel _conta;
    private ObservableCollection<ParcelaReceberModel> _parcelas;
    private ParcelaReceberModel _parcelaSelecionada;

    public VerParcelasConta(
        ContaReceberService contaService,
        ParcelaReceberService parcelaService,
        SystemSettingsService settingsService,
        VendaService vendaService,
        ContaReceberModel conta)
    {
        InitializeComponent();

        _contaService = contaService ?? throw new ArgumentNullException(nameof(contaService));
        _parcelaService = parcelaService ?? throw new ArgumentNullException(nameof(parcelaService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _vendaService = vendaService ?? throw new ArgumentNullException(nameof(vendaService));
        _conta = conta ?? throw new ArgumentNullException(nameof(conta));

        _parcelas = new ObservableCollection<ParcelaReceberModel>();
        ParcelasCollectionView.ItemsSource = _parcelas;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await LoadContaDetailsAsync();
            await LoadParcelasAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
            await Navigation.PopAsync();
        }
    }

    private async Task LoadContaDetailsAsync()
    {
        try
        {
            // Reload conta to get latest data
            _conta = await _contaService.GetByIdAsync(_conta.Id);

            // Update header
            TituloLabel.Text = $"Parcelas - Conta #{_conta.Id}";

            // Load venda details
            var venda = await _vendaService.GetByIdAsync(_conta.CodVenda);

            // Load cliente name
            var clienteService = new ClienteService(new MySqlConnectionFactory().CreateConnection());
            var cliente = await clienteService.GetByIdAsync(_conta.CodCliente);

            // Update summary
            ClienteLabel.Text = cliente?.Nome ?? "Não encontrado";
            VendaLabel.Text = $"#{_conta.CodVenda} - {venda?.data_venda:dd/MM/yyyy}";
            ValorTotalLabel.Text = $"R$ {_conta.ValorTotal:N2}";
            StatusLabel.Text = _conta.Status;
            StatusLabel.TextColor = _conta.GetStatusColor();
            ValorPagoLabel.Text = $"R$ {_conta.ValorPago:N2}";
            ValorPendenteLabel.Text = $"R$ {_conta.ValorPendente:N2}";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar detalhes da conta: {ex.Message}", "OK");
        }
    }

    private async Task LoadParcelasAsync()
    {
        try
        {
            var parcelas = await _parcelaService.GetByContaAsync(_conta.Id);

            _parcelas.Clear();
            foreach (var parcela in parcelas.OrderBy(p => p.NumeroParcela))
            {
                _parcelas.Add(parcela);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar parcelas: {ex.Message}", "OK");
        }
    }

    private void ParcelasCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _parcelaSelecionada = e.CurrentSelection.FirstOrDefault() as ParcelaReceberModel;
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        bool isSelected = _parcelaSelecionada != null;
        bool canPay = isSelected && !_parcelaSelecionada.IsPago && !_parcelaSelecionada.IsCancelado;

        RegistrarPagamentoButton.IsEnabled = canPay;
    }

    private async void RegistrarPagamentoButton_Clicked(object sender, EventArgs e)
    {
        if (_parcelaSelecionada == null)
        {
            await DisplayAlert("Aviso", "Selecione uma parcela para registrar o pagamento", "OK");
            return;
        }

        if (_parcelaSelecionada.IsPago)
        {
            await DisplayAlert("Aviso", "Esta parcela já está paga!", "OK");
            return;
        }

        if (_parcelaSelecionada.IsCancelado)
        {
            await DisplayAlert("Aviso", "Não é possível registrar pagamento em parcela cancelada", "OK");
            return;
        }

        try
        {
            var pagamentoPage = new RegistrarPagamentoParcela(
                _parcelaService,
                _settingsService,
                _parcelaSelecionada,
                _conta);

            await Navigation.PushAsync(pagamentoPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao abrir tela de pagamento: {ex.Message}", "OK");
        }
    }

    private async void AtualizarButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            await LoadContaDetailsAsync();
            await LoadParcelasAsync();

            // Clear selection
            ParcelasCollectionView.SelectedItem = null;
            _parcelaSelecionada = null;
            UpdateActionButtonsState();

            await DisplayAlert("Sucesso", "Dados atualizados com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao atualizar: {ex.Message}", "OK");
        }
    }

    private async void VoltarButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
