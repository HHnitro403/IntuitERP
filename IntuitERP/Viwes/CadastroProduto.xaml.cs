using IntuitERP.models;
using IntuitERP.Services;
using IntuitERP.Viwes.Modals;
using System.Collections.ObjectModel;
using System.Globalization;

namespace IntuitERP.Viwes;

public partial class CadastroProduto : ContentPage
{
    private readonly ProdutoService _produtoService;
    private readonly FornecedorService _fornecedorService;
    private readonly PermissionService _permissionService;
    private ObservableCollection<FornecedorModel> _listaFornecedores;
    private readonly int _id;
    private int _CodFornecedor = 0; // Store the selected supplier code

    // Constructor for Dependency Injection (recommended)
    public CadastroProduto(ProdutoService produtoService, FornecedorService fornecedorService, int id = 0)
    {
        InitializeComponent();
        _produtoService = produtoService;
        _fornecedorService = fornecedorService;
        _permissionService = new PermissionService();

        _listaFornecedores = new ObservableCollection<FornecedorModel>();

        // Display a user-friendly name in the Picker, e.g., NomeFantasia or RazaoSocial

        // Set default date for DataCadastroPicker
        DataCadastroPicker.Date = DateTime.Today;

        _id = id;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Check if user has read permission
        if (!_permissionService.CanReadProduct())
        {
            await DisplayAlert("Acesso Negado",
                _permissionService.GetPermissionDeniedMessage("visualizar produtos"),
                "OK");
            await Navigation.PopAsync();
            return;
        }

        await LoadFornecedoresAsync();
        HeaderLabel.Text = this.Title;

        // Control visibility of Save button based on permissions
        if (_id != 0)
        {
            // Editing existing product - need update permission
            SalvarProdutoButton.IsVisible = _permissionService.CanUpdateProduct();
            if (!SalvarProdutoButton.IsVisible)
            {
                await DisplayAlert("Modo Somente Leitura",
                    "Você não tem permissão para editar produtos. A visualização é somente leitura.",
                    "OK");
            }

            var produto = await _produtoService.GetByIdAsync(_id);
            if (produto != null)
            {
                var fornecedor = await _fornecedorService.GetByIdAsync((int)produto.FornecedorP_ID);

                DescricaoProdutoEntry.Text = produto.Descricao;
                FornecedorDisplayField.Text = fornecedor.RazaoSocial ?? fornecedor.NomeFantasia;
                _CodFornecedor = (int)produto.FornecedorP_ID;
                EstoqueMinimoEntry.Text = produto.EstMinimo.ToString();
                TipoProdutoEntry.Text = produto.Tipo;
                CategoriaEntry.Text = produto.Categoria;
                PrecoUnitarioEntry.Text = produto.PrecoUnitario.ToString();
                DataCadastroPicker.Date = (DateTime)produto.DataCadastro;
                AtivoSwitch.IsToggled = (bool)produto.Ativo;

                // Disable form controls if no update permission
                if (!_permissionService.CanUpdateProduct())
                {
                    DisableFormControls();
                }
            }
        }
        else
        {
            // Creating new product - need create permission
            SalvarProdutoButton.IsVisible = _permissionService.CanCreateProduct();
            if (!SalvarProdutoButton.IsVisible)
            {
                await DisplayAlert("Acesso Negado",
                    _permissionService.GetPermissionDeniedMessage("criar produtos"),
                    "OK");
                await Navigation.PopAsync();
                return;
            }
        }
    }

    private void DisableFormControls()
    {
        DescricaoProdutoEntry.IsEnabled = false;
        CategoriaEntry.IsEnabled = false;
        PrecoUnitarioEntry.IsEnabled = false;
        SelectFornecedorButton.IsEnabled = false;
        EstoqueMinimoEntry.IsEnabled = false;
        TipoProdutoEntry.IsEnabled = false;
        DataCadastroPicker.IsEnabled = false;
        AtivoSwitch.IsEnabled = false;
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
                await DisplayAlert("Aten��o", "Nenhum fornecedor encontrado para carregar no seletor. Cadastre fornecedores primeiro.", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading suppliers: {ex.Message}");
            await DisplayAlert("Erro", "N�o foi poss�vel carregar a lista de fornecedores.", "OK");
        }
    }

    private void ClearForm()
    {
        DescricaoProdutoEntry.Text = string.Empty;
        CategoriaEntry.Text = string.Empty;
        PrecoUnitarioEntry.Text = string.Empty;
        FornecedorDisplayField.Text = string.Empty; // Clear the FornecedorDisplayF
        EstoqueMinimoEntry.Text = string.Empty;
        TipoProdutoEntry.Text = string.Empty;
        DataCadastroPicker.Date = DateTime.Today;
        AtivoSwitch.IsToggled = true;

        DescricaoProdutoEntry.Focus();
    }

    private async void SalvarProdutoButton_Clicked(object sender, EventArgs e)
    {
        // Check permissions before saving
        try
        {
            if (_id != 0)
            {
                // Updating existing product
                _permissionService.RequireProductUpdate();
            }
            else
            {
                // Creating new product
                _permissionService.RequireProductCreate();
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            await DisplayAlert("Acesso Negado", ex.Message, "OK");
            return;
        }

        // --- Basic Validation ---
        if (string.IsNullOrWhiteSpace(DescricaoProdutoEntry.Text))
        {
            await DisplayAlert("Campo Obrigat�rio", "Por favor, preencha a Descri��o do Produto.", "OK");
            DescricaoProdutoEntry.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(CategoriaEntry.Text))
        {
            await DisplayAlert("Campo Obrigat�rio", "Por favor, preencha a Categoria.", "OK");
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
                await DisplayAlert("Pre�o Inv�lido", "Por favor, insira um Pre�o Unit�rio v�lido e maior que zero.", "OK");
                PrecoUnitarioEntry.Focus();
                return;
            }
        }

        if (FornecedorDisplayField.Text == string.Empty)
        {
            await DisplayAlert("Campo Obrigat�rio", "Por favor, selecione o Fornecedor Principal.", "OK");
            SelectFornecedorButton.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(EstoqueMinimoEntry.Text) ||
            !int.TryParse(EstoqueMinimoEntry.Text, out int estMinimo) || estMinimo < 0)
        {
            await DisplayAlert("Estoque M�nimo Inv�lido", "Por favor, insira um Estoque M�nimo v�lido (n�mero inteiro n�o negativo).", "OK");
            EstoqueMinimoEntry.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(TipoProdutoEntry.Text))
        {
            await DisplayAlert("Campo Obrigat�rio", "Por favor, preencha o Tipo do Produto.", "OK");
            TipoProdutoEntry.Focus();
            return;
        }

        // --- Create ProdutoModel ---
        var Produto = new ProdutoModel
        {
            Descricao = DescricaoProdutoEntry.Text.Trim(),
            Categoria = CategoriaEntry.Text.Trim(),
            PrecoUnitario = preco, // Use the parsed decimal value
            FornecedorP_ID = _CodFornecedor, // Get ID from selected supplier
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
                await DisplayAlert("Erro", "N�o foi poss�vel cadastrar o produto. Verifique os dados e tente novamente.", "OK");
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
        bool confirm = await DisplayAlert("Cancelar Cadastro", "Tem certeza que deseja cancelar o cadastro? Todas as informa��es n�o salvas ser�o perdidas.", "Sim", "N�o");
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

    private async void SelectFornecedorButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var fornecedorlist = await _fornecedorService.GetAllAsync();
            var selectedfornecedor = await ModalPicker.Show<FornecedorModel>(Navigation, "Selecione a Cidade", fornecedorlist);
            if (selectedfornecedor != null)
            {
                {
                    await DisplayAlert("Fornecedor Selecionado", selectedfornecedor.ToString(), "Ok");
                    FornecedorDisplayField.Text = string.Empty;
                    FornecedorDisplayField.Text = selectedfornecedor.RazaoSocial ?? selectedfornecedor.NomeFantasia;
                    _CodFornecedor = 0;
                    _CodFornecedor = selectedfornecedor.CodFornecedor; // Store the selected city code
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro Inesperado", $"Ocorreu um erro ao Selecionar a Cidade: {ex.Message}", "OK");
        }
    }
}