using IntuitERP.models;
using IntuitERP.Services;
using IntuitERP.Config;
using System.Collections.ObjectModel;

namespace IntuitERP.Viwes;

public partial class CadastroContaReceber : ContentPage
{
    private readonly ContaReceberService _contaService;
    private readonly ParcelaReceberService _parcelaService;
    private readonly VendaService _vendaService;

    private ContaReceberModel _conta;
    private VendaModel _venda;
    private ObservableCollection<ParcelaReceberModel> _parcelas;
    private bool _isEditMode;

    public CadastroContaReceber(
        ContaReceberService contaService,
        ParcelaReceberService parcelaService,
        VendaService vendaService,
        VendaModel venda = null,
        ContaReceberModel conta = null)
    {
        InitializeComponent();

        _contaService = contaService ?? throw new ArgumentNullException(nameof(contaService));
        _parcelaService = parcelaService ?? throw new ArgumentNullException(nameof(parcelaService));
        _vendaService = vendaService ?? throw new ArgumentNullException(nameof(vendaService));

        _venda = venda;
        _conta = conta;
        _isEditMode = conta != null;

        _parcelas = new ObservableCollection<ParcelaReceberModel>();
        ParcelasCollectionView.ItemsSource = _parcelas;

        // Set default date to 30 days from now
        PrimeiraParcelaDatePicker.Date = DateTime.Today.AddDays(30);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            if (_isEditMode)
            {
                await LoadExistingContaAsync();
            }
            else if (_venda != null)
            {
                await LoadFromVendaAsync();
            }
            else
            {
                await DisplayAlert("Erro", "Nenhuma venda selecionada. Use esta tela através de uma venda faturada.", "OK");
                await Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
            await Navigation.PopAsync();
        }
    }

    private async Task LoadFromVendaAsync()
    {
        TituloLabel.Text = "Nova Conta a Receber";

        _conta = new ContaReceberModel
        {
            CodVenda = _venda.CodVenda,
            CodCliente = _venda.CodCliente,
            DataEmissao = _venda.data_venda ?? DateTime.Today,
            ValorTotal = _venda.valor_total,
            ValorPago = 0,
            ValorPendente = _venda.valor_total,
            NumParcelas = 1,
            Status = "Pendente"
        };

        // Load cliente name
        var clienteService = new ClienteService(new MySqlConnectionFactory().CreateConnection());
        var cliente = await clienteService.GetByIdAsync(_venda.CodCliente);

        VendaLabel.Text = $"#{_venda.CodVenda} - {_venda.data_venda:dd/MM/yyyy}";
        ClienteLabel.Text = cliente?.Nome ?? "Não encontrado";
        ValorTotalLabel.Text = $"R$ {_venda.valor_total:N2}";

        // Default to 1 parcela
        NumParcelasPicker.SelectedIndex = 0;
    }

    private async Task LoadExistingContaAsync()
    {
        TituloLabel.Text = $"Editar Conta #{_conta.Id}";

        // Load venda
        _venda = await _vendaService.GetByIdAsync(_conta.CodVenda);

        // Load cliente name
        var clienteService = new ClienteService(new MySqlConnectionFactory().CreateConnection());
        var cliente = await clienteService.GetByIdAsync(_conta.CodCliente);

        VendaLabel.Text = $"#{_conta.CodVenda} - {_conta.DataEmissao:dd/MM/yyyy}";
        ClienteLabel.Text = cliente?.Nome ?? "Não encontrado";
        ValorTotalLabel.Text = $"R$ {_conta.ValorTotal:N2}";

        // Load existing parcelas
        var parcelas = await _parcelaService.GetByContaAsync(_conta.Id);
        _parcelas.Clear();
        foreach (var parcela in parcelas)
        {
            _parcelas.Add(parcela);
        }

        // Set num parcelas
        NumParcelasPicker.SelectedItem = _conta.NumParcelas.ToString();

        // Load observações
        ObservacoesEditor.Text = _conta.Observacoes;
    }

    private void NumParcelasPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Auto-generate when selection changes if not in edit mode
        if (!_isEditMode && NumParcelasPicker.SelectedIndex >= 0)
        {
            // Could auto-generate here, but let's keep manual button click
        }
    }

    private void GerarParcelasButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (NumParcelasPicker.SelectedItem == null)
            {
                DisplayAlert("Aviso", "Selecione o número de parcelas", "OK");
                return;
            }

            int numParcelas = int.Parse(NumParcelasPicker.SelectedItem.ToString());
            int intervaloDias = 30;

            if (!string.IsNullOrEmpty(IntervaloDiasEntry.Text) && int.TryParse(IntervaloDiasEntry.Text, out int intervalo))
            {
                intervaloDias = intervalo;
            }

            DateTime primeiraData = PrimeiraParcelaDatePicker.Date;

            // Generate parcelas (conta.Id = 0 for new, will be updated on save)
            var parcelas = _parcelaService.GerarParcelasIguais(
                _conta?.Id ?? 0,
                _conta?.ValorTotal ?? 0,
                numParcelas,
                primeiraData,
                intervaloDias);

            // Update UI
            _parcelas.Clear();
            foreach (var parcela in parcelas)
            {
                _parcelas.Add(parcela);
            }

            DisplayAlert("Sucesso", $"{numParcelas} parcela(s) gerada(s) com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            DisplayAlert("Erro", $"Erro ao gerar parcelas: {ex.Message}", "OK");
        }
    }

    private async void SalvarButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (_parcelas.Count == 0)
            {
                await DisplayAlert("Aviso", "Gere as parcelas antes de salvar", "OK");
                return;
            }

            // Validate total
            decimal totalParcelas = _parcelas.Sum(p => p.ValorParcela);
            if (Math.Abs(totalParcelas - _conta.ValorTotal) > 0.01m)
            {
                await DisplayAlert("Erro",
                    $"A soma das parcelas (R$ {totalParcelas:N2}) não corresponde ao valor total (R$ {_conta.ValorTotal:N2})",
                    "OK");
                return;
            }

            // Update conta
            _conta.NumParcelas = _parcelas.Count;
            _conta.Observacoes = ObservacoesEditor.Text;

            int contaId;

            if (_isEditMode)
            {
                // Update existing
                await _contaService.UpdateAsync(_conta);
                contaId = _conta.Id;

                // Delete old parcelas and insert new ones
                var oldParcelas = await _parcelaService.GetByContaAsync(contaId);
                foreach (var old in oldParcelas)
                {
                    await _parcelaService.DeleteAsync(old.Id);
                }
            }
            else
            {
                // Insert new
                contaId = await _contaService.InsertAsync(_conta);
            }

            // Update parcela cod_conta_receber
            foreach (var parcela in _parcelas)
            {
                parcela.CodContaReceber = contaId;
            }

            // Insert parcelas
            await _parcelaService.InsertBatchAsync(_parcelas.ToList());

            await DisplayAlert("Sucesso", "Conta a receber salva com sucesso!", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao salvar: {ex.Message}", "OK");
        }
    }

    private async void CancelarButton_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirmar", "Deseja cancelar? As alterações serão perdidas.", "Sim", "Não");
        if (confirm)
        {
            await Navigation.PopAsync();
        }
    }
}
