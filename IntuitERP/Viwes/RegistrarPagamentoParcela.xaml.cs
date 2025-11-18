using IntuitERP.models;
using IntuitERP.Services;
using System.Globalization;

namespace IntuitERP.Viwes;

public partial class RegistrarPagamentoParcela : ContentPage
{
    private readonly ParcelaReceberService _parcelaService;
    private readonly SystemSettingsService _settingsService;
    private ParcelaReceberModel _parcela;
    private ContaReceberModel _conta;

    private decimal _jurosCalculado;
    private decimal _multaCalculada;
    private decimal _descontoAtual;
    private decimal _valorTotalAPagar;

    public RegistrarPagamentoParcela(
        ParcelaReceberService parcelaService,
        SystemSettingsService settingsService,
        ParcelaReceberModel parcela,
        ContaReceberModel conta)
    {
        InitializeComponent();

        _parcelaService = parcelaService ?? throw new ArgumentNullException(nameof(parcelaService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _parcela = parcela ?? throw new ArgumentNullException(nameof(parcela));
        _conta = conta ?? throw new ArgumentNullException(nameof(conta));

        // Set default payment date to today
        DataPagamentoPicker.Date = DateTime.Today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await LoadParcelaDetailsAsync();
            await CalculateJurosMultaAsync();
            UpdateTotalDisplay();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
            await Navigation.PopAsync();
        }
    }

    private async Task LoadParcelaDetailsAsync()
    {
        // Update header
        TituloLabel.Text = $"Registrar Pagamento - Parcela #{_parcela.NumeroParcela}";
        ContaLabel.Text = $"Conta a Receber #{_conta.Id}";

        // Parcela information
        VencimentoLabel.Text = _parcela.DataVencimento.ToString("dd/MM/yyyy");
        StatusLabel.Text = _parcela.Status;
        StatusLabel.TextColor = _parcela.Status switch
        {
            "Pago" => Colors.Green,
            "Vencido" => Colors.Red,
            "Pendente" => Colors.Gray,
            _ => Colors.Gray
        };

        ValorOriginalLabel.Text = $"R$ {_parcela.ValorParcela:N2}";
        DiasAtrasoLabel.Text = _parcela.DiasAtraso > 0
            ? $"{_parcela.DiasAtraso} dias"
            : "No prazo";
        DiasAtrasoLabel.TextColor = _parcela.DiasAtraso > 0 ? Colors.Red : Colors.Green;

        ValorJaPagoLabel.Text = $"R$ {_parcela.ValorPago:N2}";

        // Check if already paid
        if (_parcela.IsPago)
        {
            await DisplayAlert("Aviso", "Esta parcela já está paga!", "OK");
            await Navigation.PopAsync();
        }
    }

    private async Task CalculateJurosMultaAsync()
    {
        try
        {
            // Get settings
            var jurosMensal = await _settingsService.GetSettingAsync<decimal>("juros_mensal_padrao");
            var multaPadrao = await _settingsService.GetSettingAsync<decimal>("multa_padrao");
            var carenciaDias = await _settingsService.GetSettingAsync<int>("carencia_juros_dias");

            // Calculate if overdue
            if (_parcela.IsVencida && !_parcela.IsPago)
            {
                var (juros, multa) = await _parcelaService.CalcularJurosMultaAsync(
                    _parcela.Id,
                    jurosMensal,
                    multaPadrao,
                    carenciaDias);

                _jurosCalculado = juros;
                _multaCalculada = multa;
            }
            else
            {
                _jurosCalculado = 0;
                _multaCalculada = 0;
            }

            // Update display
            JurosLabel.Text = $"R$ {_jurosCalculado:N2}";
            MultaLabel.Text = $"R$ {_multaCalculada:N2}";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Aviso",
                $"Não foi possível calcular juros/multa. Usando valores padrão.\n{ex.Message}",
                "OK");
            _jurosCalculado = 0;
            _multaCalculada = 0;
        }
    }

    private void UpdateTotalDisplay()
    {
        // Calculate total considering already paid amount
        decimal valorRestante = _parcela.ValorParcela - _parcela.ValorPago;
        _valorTotalAPagar = valorRestante + _jurosCalculado + _multaCalculada - _descontoAtual;

        // Ensure not negative
        if (_valorTotalAPagar < 0)
            _valorTotalAPagar = 0;

        ValorTotalLabel.Text = $"R$ {_valorTotalAPagar:N2}";

        // Pre-fill payment amount with total
        if (string.IsNullOrEmpty(ValorPagamentoEntry.Text))
        {
            ValorPagamentoEntry.Text = _valorTotalAPagar.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));
        }
    }

    private void DescontoEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                _descontoAtual = 0;
            }
            else
            {
                // Parse using pt-BR culture
                string cleanValue = e.NewTextValue.Replace(".", "").Replace(",", ".");
                if (decimal.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal desconto))
                {
                    _descontoAtual = desconto;
                }
            }

            UpdateTotalDisplay();
        }
        catch
        {
            _descontoAtual = 0;
        }
    }

    private void ValorPagamentoEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                PagamentoParcialLabel.IsVisible = false;
                return;
            }

            // Parse using pt-BR culture
            string cleanValue = e.NewTextValue.Replace(".", "").Replace(",", ".");
            if (decimal.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valorPagamento))
            {
                // Check if partial payment
                if (valorPagamento < _valorTotalAPagar && valorPagamento > 0)
                {
                    decimal restante = _valorTotalAPagar - valorPagamento;
                    PagamentoParcialLabel.Text = $"Pagamento parcial - restará R$ {restante:N2}";
                    PagamentoParcialLabel.IsVisible = true;
                }
                else if (valorPagamento > _valorTotalAPagar)
                {
                    PagamentoParcialLabel.Text = $"Valor excede o total a pagar em R$ {(valorPagamento - _valorTotalAPagar):N2}";
                    PagamentoParcialLabel.TextColor = Colors.Red;
                    PagamentoParcialLabel.IsVisible = true;
                }
                else
                {
                    PagamentoParcialLabel.IsVisible = false;
                }
            }
        }
        catch
        {
            PagamentoParcialLabel.IsVisible = false;
        }
    }

    private async void SalvarButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Validate forma pagamento
            if (FormaPagamentoPicker.SelectedItem == null)
            {
                await DisplayAlert("Aviso", "Selecione a forma de pagamento", "OK");
                return;
            }

            // Validate valor pagamento
            if (string.IsNullOrWhiteSpace(ValorPagamentoEntry.Text))
            {
                await DisplayAlert("Aviso", "Informe o valor do pagamento", "OK");
                return;
            }

            string cleanValue = ValorPagamentoEntry.Text.Replace(".", "").Replace(",", ".");
            if (!decimal.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valorPagamento))
            {
                await DisplayAlert("Erro", "Valor de pagamento inválido", "OK");
                return;
            }

            if (valorPagamento <= 0)
            {
                await DisplayAlert("Erro", "O valor do pagamento deve ser maior que zero", "OK");
                return;
            }

            // Confirm if exceeds total
            if (valorPagamento > _valorTotalAPagar)
            {
                bool confirm = await DisplayAlert("Confirmar",
                    $"O valor informado (R$ {valorPagamento:N2}) excede o total a pagar (R$ {_valorTotalAPagar:N2}).\n\n" +
                    "Deseja continuar mesmo assim?",
                    "Sim", "Não");

                if (!confirm)
                    return;
            }

            // Confirm partial payment
            if (valorPagamento < _valorTotalAPagar)
            {
                decimal restante = _valorTotalAPagar - valorPagamento;
                bool confirm = await DisplayAlert("Pagamento Parcial",
                    $"Você está registrando um pagamento parcial de R$ {valorPagamento:N2}.\n" +
                    $"Restará R$ {restante:N2} a pagar.\n\n" +
                    "Confirma o pagamento parcial?",
                    "Sim", "Não");

                if (!confirm)
                    return;
            }

            // Register payment
            DateTime dataPagamento = DataPagamentoPicker.Date;
            string formaPagamento = FormaPagamentoPicker.SelectedItem.ToString();
            string observacoes = ObservacoesEditor.Text;

            await _parcelaService.RegistrarPagamentoAsync(
                _parcela.Id,
                valorPagamento,
                dataPagamento,
                formaPagamento,
                _jurosCalculado,
                _multaCalculada,
                _descontoAtual,
                observacoes);

            await DisplayAlert("Sucesso",
                $"Pagamento de R$ {valorPagamento:N2} registrado com sucesso!",
                "OK");

            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao registrar pagamento: {ex.Message}", "OK");
        }
    }

    private async void CancelarButton_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirmar",
            "Deseja cancelar o registro de pagamento?",
            "Sim", "Não");

        if (confirm)
        {
            await Navigation.PopAsync();
        }
    }
}
