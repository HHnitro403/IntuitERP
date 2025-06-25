using IntuitERP.models;
using IntuitERP.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace IntuitERP.Viwes;

public partial class CadstroEstoque : ContentPage
{
    private readonly EstoqueService _estoqueService;
    private readonly ProdutoService _produtoService;
    private ObservableCollection<ProdutoModel> _listaProdutos;

    // Helper class for TipoMovimentacaoPicker
    public class TipoMovimentacaoItem
    {
        public string DisplayName { get; set; }
        public char Value { get; set; } // 'E' for Entrada, 'S' for Saída
    }
    private ObservableCollection<TipoMovimentacaoItem> _tiposMovimentacao;


    // Constructor for Dependency Injection (recommended)
    public CadstroEstoque(EstoqueService estoqueService, ProdutoService produtoService)
    {
        InitializeComponent();
        _estoqueService = estoqueService;
        _produtoService = produtoService;

        _listaProdutos = new ObservableCollection<ProdutoModel>();
        ProdutoPicker.ItemsSource = _listaProdutos;
        ProdutoPicker.ItemDisplayBinding = new Binding("Descricao"); // Display product description

        // Populate TipoMovimentacaoPicker
        _tiposMovimentacao = new ObservableCollection<TipoMovimentacaoItem>
            {
                new TipoMovimentacaoItem { DisplayName = "E - Entrada", Value = 'E' },
                new TipoMovimentacaoItem { DisplayName = "S - Saída", Value = 'S' }
            };
        TipoMovimentacaoPicker.ItemsSource = _tiposMovimentacao;
        TipoMovimentacaoPicker.ItemDisplayBinding = new Binding("DisplayName");


        // Set default date for DataMovimentacaoPicker
        DataMovimentacaoPicker.Date = DateTime.Today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProdutosAsync();
    }

    private async Task LoadProdutosAsync()
    {
        try
        {
            var produtos = await _produtoService.GetAllAsync();
            _listaProdutos.Clear();
            if (produtos != null && produtos.Any())
            {
                // Sort products for better UX, e.g., by Descricao
                foreach (var produto in produtos.OrderBy(p => p.Descricao))
                {
                    _listaProdutos.Add(produto);
                }
            }
            else
            {
                await DisplayAlert("Atenção", "Nenhum produto encontrado para carregar no seletor. Cadastre produtos primeiro.", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading products: {ex.Message}");
            await DisplayAlert("Erro", "Não foi possível carregar a lista de produtos.", "OK");
        }
    }

    private void ClearForm()
    {
        ProdutoPicker.SelectedItem = null;
        TipoMovimentacaoPicker.SelectedItem = null;
        QuantidadeEntry.Text = string.Empty;
        DataMovimentacaoPicker.Date = DateTime.Today;
        ProdutoPicker.Focus();
    }

    private async void SalvarMovimentacaoButton_Clicked(object sender, EventArgs e)
    {
        // --- Basic Validation ---
        if (ProdutoPicker.SelectedItem == null)
        {
            await DisplayAlert("Campo Obrigatório", "Por favor, selecione o Produto.", "OK");
            ProdutoPicker.Focus();
            return;
        }

        if (TipoMovimentacaoPicker.SelectedItem == null)
        {
            await DisplayAlert("Campo Obrigatório", "Por favor, selecione o Tipo de Movimentação.", "OK");
            TipoMovimentacaoPicker.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(QuantidadeEntry.Text) ||
            !decimal.TryParse(QuantidadeEntry.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal qtd) ||
            qtd <= 0)
        {
            // Attempt to parse with InvariantCulture as a fallback if CurrentCulture fails (e.g. for '.')
            if (!decimal.TryParse(QuantidadeEntry.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out qtd) || qtd <= 0)
            {
                await DisplayAlert("Quantidade Inválida", "Por favor, insira uma Quantidade válida e maior que zero.", "OK");
                QuantidadeEntry.Focus();
                return;
            }
        }

        var selectedProduto = (ProdutoModel)ProdutoPicker.SelectedItem;
        var selectedTipoMovimentacaoItem = (TipoMovimentacaoItem)TipoMovimentacaoPicker.SelectedItem;
        char tipoMovimentacao = selectedTipoMovimentacaoItem.Value;

        // Additional validation for 'Saída' (Stock Out)
        if (tipoMovimentacao == 'S')
        {
            // Fetch current stock balance to ensure sufficient stock
            // The ProdutoModel might not have the most up-to-date SaldoEst if it was loaded once.
            // It's safer to re-fetch or ensure your service layer handles this check atomically.
            // For simplicity here, we'll assume selectedProduto.SaldoEst is reasonably current
            // or that a database trigger/constraint would prevent negative stock if not handled here.
            var produtoAtual = await _produtoService.GetByIdAsync(selectedProduto.CodProduto);
            if (produtoAtual == null || produtoAtual.SaldoEst < qtd)
            {
                await DisplayAlert("Estoque Insuficiente", $"Não há saldo suficiente para o produto '{selectedProduto.Descricao}'. Saldo atual: {produtoAtual?.SaldoEst ?? 0}", "OK");
                QuantidadeEntry.Focus();
                return;
            }
        }


        // --- Create EstoqueModel (Stock Movement Log) ---
        var novaMovimentacao = new EstoqueModel
        {
            CodProduto = selectedProduto.CodProduto,
            Tipo = tipoMovimentacao, // 'E' or 'S'
            Qtd = qtd,
            Data = DataMovimentacaoPicker.Date
        };

        try
        {
            // 1. Insert the stock movement log
            int newMovimentacaoId = await _estoqueService.InsertAsync(novaMovimentacao);

            if (newMovimentacaoId > 0)
            {
                // 2. Update the product's stock balance (SaldoEst in 'produto' table)
                decimal quantidadeParaAtualizarSaldo = (tipoMovimentacao == 'E') ? qtd : -qtd;

                // Update the product's stock balance

                int updateResult = await _produtoService.AtualizarEstoqueAsync(selectedProduto.CodProduto, (int)quantidadeParaAtualizarSaldo);

                if (updateResult > 0)
                {
                    await DisplayAlert("Sucesso", "Movimentação de estoque registrada e saldo do produto atualizado!", "OK");
                    ClearForm();
                    // Optionally, refresh product list if displayed elsewhere or navigate
                }
                else
                {
                    // This case is tricky: movement logged, but product balance update failed.
                    // Requires a compensation strategy (e.g., log this error, attempt to revert movement, or manual correction).
                    await DisplayAlert("Atenção", "Movimentação registrada, mas houve um erro ao atualizar o saldo do produto. Verifique o produto.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Erro", "Não foi possível registrar a movimentação de estoque.", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving stock movement: {ex.Message}");
            await DisplayAlert("Erro Inesperado", $"Ocorreu um erro ao salvar a movimentação: {ex.Message}", "OK");
        }
    }

    private async void CancelarMovimentacaoButton_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Cancelar Movimentação", "Tem certeza que deseja cancelar? As informações não salvas serão perdidas.", "Sim", "Não");
        if (confirm)
        {
            ClearForm();
            if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync();
            }
        }
    }

    private void QuantidadeEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            string text = e.NewTextValue;
            if (string.IsNullOrWhiteSpace(text)) return;

            char decimalSeparator = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            string validChars = "0123456789" + decimalSeparator;

            string cleanedText = new string(text.Where(c => validChars.Contains(c)).ToArray());

            int separatorCount = cleanedText.Count(c => c == decimalSeparator);
            if (separatorCount > 1)
            {
                int firstSeparatorIndex = cleanedText.IndexOf(decimalSeparator);
                cleanedText = cleanedText.Substring(0, firstSeparatorIndex + 1) + cleanedText.Substring(firstSeparatorIndex + 1).Replace(decimalSeparator.ToString(), "");
            }

            if (entry.Text != cleanedText)
            {
                entry.Text = cleanedText;
                entry.CursorPosition = cleanedText.Length;
            }
        }
    }
}