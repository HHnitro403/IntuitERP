using IntuitERP.models;
using IntuitERP.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace IntuitERP.Viwes;

public partial class CadastroProduto : ContentPage
{
    private readonly ProdutoService _produtoService;
    private readonly FornecedorService _fornecedorService;
    private ObservableCollection<FornecedorModel> _listaFornecedores;
    private readonly int _id;

    // Constructor for Dependency Injection (recommended)
    public CadastroProduto(ProdutoService produtoService, FornecedorService fornecedorService, int id = 0)
    {
        InitializeComponent();
        _produtoService = produtoService;
        _fornecedorService = fornecedorService;

        _listaFornecedores = new ObservableCollection<FornecedorModel>();
        FornecedorPicker.ItemsSource = _listaFornecedores;
        // Display a user-friendly name in the Picker, e.g., NomeFantasia or RazaoSocial
        FornecedorPicker.ItemDisplayBinding = new Binding("NomeFantasia"); // Or RazaoSocial

        // Set default date for DataCadastroPicker
        DataCadastroPicker.Date = DateTime.Today;

        _id = id;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadFornecedoresAsync();
        HeaderLabel.Text = this.Title;
        if (_id != 0)
        {
            var produto = await _produtoService.GetByIdAsync(_id);
            if (produto != null)
            {
                DescricaoProdutoEntry.Text = produto.Descricao;
                FornecedorPicker.SelectedItem = produto.Fornecedor;
                EstoqueMinimoEntry.Text = produto.EstMinimo.ToString();
                TipoProdutoEntry.Text = produto.Tipo;
                CategoriaEntry.Text = produto.Categoria;
                PrecoUnitarioEntry.Text = produto.PrecoUnitario.ToString();
                DataCadastroPicker.Date = (DateTime)produto.DataCadastro;
                AtivoSwitch.IsToggled = (bool)produto.Ativo;
            }
        }
    }

    private async Task LoadFornecedoresAsync()
    {
        try
        {
            var fornecedores = await _fornecedorService.GetAllAsync();
            _listaFornecedores.Clear();
            if (fornecedores != null && fornecedores.Any())
            {
                // Sort suppliers for better UX, e.g., by NomeFantasia or RazaoSocial
                foreach (var fornecedor in fornecedores.OrderBy(f => f.NomeFantasia ?? f.RazaoSocial))
                {
                    _listaFornecedores.Add(fornecedor);
                }
            }
            else
            {
                await DisplayAlert("Atenção", "Nenhum fornecedor encontrado para carregar no seletor. Cadastre fornecedores primeiro.", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading suppliers: {ex.Message}");
            await DisplayAlert("Erro", "Não foi possível carregar a lista de fornecedores.", "OK");
        }
    }

    private void ClearForm()
    {
        DescricaoProdutoEntry.Text = string.Empty;
        CategoriaEntry.Text = string.Empty;
        PrecoUnitarioEntry.Text = string.Empty;
        FornecedorPicker.SelectedItem = null;
        EstoqueMinimoEntry.Text = string.Empty;
        TipoProdutoEntry.Text = string.Empty;
        DataCadastroPicker.Date = DateTime.Today;
        AtivoSwitch.IsToggled = true;

        DescricaoProdutoEntry.Focus();
    }

    private async void SalvarProdutoButton_Clicked(object sender, EventArgs e)
    {
        // --- Basic Validation ---
        if (string.IsNullOrWhiteSpace(DescricaoProdutoEntry.Text))
        {
            await DisplayAlert("Campo Obrigatório", "Por favor, preencha a Descrição do Produto.", "OK");
            DescricaoProdutoEntry.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(CategoriaEntry.Text))
        {
            await DisplayAlert("Campo Obrigatório", "Por favor, preencha a Categoria.", "OK");
            CategoriaEntry.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(PrecoUnitarioEntry.Text) ||
            !decimal.TryParse(PrecoUnitarioEntry.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal preco) ||
            preco <= 0)
        {
            // Attempt to parse with InvariantCulture as a fallback if CurrentCulture fails (e.g. for '.')
            if (!decimal.TryParse(PrecoUnitarioEntry.Text, NumberStyles.Currency, CultureInfo.InvariantCulture, out preco) || preco <= 0)
            {
                await DisplayAlert("Preço Inválido", "Por favor, insira um Preço Unitário válido e maior que zero.", "OK");
                PrecoUnitarioEntry.Focus();
                return;
            }
        }

        if (FornecedorPicker.SelectedItem == null)
        {
            await DisplayAlert("Campo Obrigatório", "Por favor, selecione o Fornecedor Principal.", "OK");
            FornecedorPicker.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(EstoqueMinimoEntry.Text) ||
            !int.TryParse(EstoqueMinimoEntry.Text, out int estMinimo) || estMinimo < 0)
        {
            await DisplayAlert("Estoque Mínimo Inválido", "Por favor, insira um Estoque Mínimo válido (número inteiro não negativo).", "OK");
            EstoqueMinimoEntry.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(TipoProdutoEntry.Text))
        {
            await DisplayAlert("Campo Obrigatório", "Por favor, preencha o Tipo do Produto.", "OK");
            TipoProdutoEntry.Focus();
            return;
        }

        var selectedFornecedor = (FornecedorModel)FornecedorPicker.SelectedItem;

        // --- Create ProdutoModel ---
        var Produto = new ProdutoModel
        {
            Descricao = DescricaoProdutoEntry.Text.Trim(),
            Categoria = CategoriaEntry.Text.Trim(),
            PrecoUnitario = preco, // Use the parsed decimal value
            FornecedorP_ID = selectedFornecedor.CodFornecedor, // Get ID from selected supplier
            EstMinimo = estMinimo,
            Tipo = TipoProdutoEntry.Text.Trim(),
            DataCadastro = DateTime.Now, // Service layer also defaults this
            Ativo = AtivoSwitch.IsToggled,
            SaldoEst = 0 // Initial stock balance is typically 0, managed by stock entries
                         // EstoqueID and VarianteID are not set here as per XAML comments
        };

        try
        {
            if (_id > 0)
            {
                Produto.CodProduto = _id;
                var result = await _produtoService.UpdateAsync(Produto);

                if (result > 0)
                {
                    await DisplayAlert("Sucesso", "Produto alterado com sucesso!", "OK");
                    await Navigation.PopAsync();
                }
            }

            int newProdutoId = await _produtoService.InsertAsync(Produto);

            if (newProdutoId > 0)
            {
                await DisplayAlert("Sucesso", "Produto cadastrado com sucesso!", "OK");
                ClearForm();
                // Optionally, navigate away or update a list
                // await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Erro", "Não foi possível cadastrar o produto. Verifique os dados e tente novamente.", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving product: {ex.Message}");
            await DisplayAlert("Erro Inesperado", $"Ocorreu um erro ao salvar o produto: {ex.Message}", "OK");
        }
    }

    private async void CancelarButton_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Cancelar Cadastro", "Tem certeza que deseja cancelar o cadastro? Todas as informações não salvas serão perdidas.", "Sim", "Não");
        if (confirm)
        {
            ClearForm();
            if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync();
            }
        }
    }

    private void PrecoUnitarioEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            // Basic attempt to guide user towards decimal format.
            // More sophisticated masking or behavior might be needed for robust currency input.
            string text = e.NewTextValue;
            if (string.IsNullOrWhiteSpace(text)) return;

            // Remove non-digit and non-decimal separator characters, allowing only one separator
            char decimalSeparator = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            string validChars = "0123456789" + decimalSeparator;

            string cleanedText = new string(text.Where(c => validChars.Contains(c)).ToArray());

            // Ensure only one decimal separator
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

    private void SalvarProdutoButton_Clicked_1(object sender, EventArgs e)
    {
    }
}