using IntuitERP.models;
using IntuitERP.Services;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace IntuitERP.Viwes;

public partial class CadastrodeFornecedor : ContentPage
{

    private readonly FornecedorService _fornecedorService;
    private readonly CidadeService _cidadeService;
    private ObservableCollection<CidadeModel> _listaCidades;
    private readonly int _fornecedorId;
    public CadastrodeFornecedor(FornecedorService fornecedorService, CidadeService cidadeService, int fornecedorId = 0)
    {
        InitializeComponent();
        _fornecedorService = fornecedorService;
        _cidadeService = cidadeService;

        _listaCidades = new ObservableCollection<CidadeModel>();
        CidadePicker.ItemsSource = _listaCidades;
        CidadePicker.ItemDisplayBinding = new Binding("Nome"); // Display city name in Picker

        // Set default date for DataCadastroPicker
        DataCadastroPicker.Date = DateTime.Today;

        // Optional: Add TextChanged event for CNPJ and Telefone formatting
        CnpjEntry.TextChanged += CnpjEntry_TextChanged;
        TelefoneEntry.TextChanged += TelefoneEntry_TextChanged;

        _fornecedorId = fornecedorId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCidadesAsync();

        if (_fornecedorId != 0)
        {
            var fornecedor = await _fornecedorService.GetByIdAsync(_fornecedorId);
            if (fornecedor != null)
            {
                RazaoSocialEntry.Text = fornecedor.RazaoSocial;
                NomeFantasiaEntry.Text = fornecedor.NomeFantasia;
                CnpjEntry.Text = fornecedor.CNPJ;
                EmailEntry.Text = fornecedor.Email;
                TelefoneEntry.Text = fornecedor.Telefone;
                EnderecoEntry.Text = fornecedor.Endereco;
                CidadePicker.SelectedItem = _listaCidades.FirstOrDefault(c => c.CodCIdade == fornecedor.CodCidade);
                DataCadastroPicker.Date = (DateTime)fornecedor.DataCadastro;
                AtivoSwitch.IsToggled = (bool)fornecedor.Ativo;
            }

        }
    }

    private async Task LoadCidadesAsync()
    {
        try
        {
            var cidades = await _cidadeService.GetAllAsync();
            _listaCidades.Clear();
            if (cidades != null && cidades.Any())
            {
                // Sort cities by name for better UX
                foreach (var cidade in cidades.OrderBy(c => c.Cidade))
                {
                    _listaCidades.Add(cidade);
                }
                // Optionally pre-select an item or leave it to the Picker's Title
                // CidadePicker.SelectedIndex = 0; // If you want to select the first one by default
            }
            else
            {
                await DisplayAlert("Atenção", "Nenhuma cidade encontrada para carregar no seletor. Cadastre cidades primeiro.", "OK");
                // Consider disabling the form or parts of it if no cities are available
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading cities: {ex.Message}");
            await DisplayAlert("Erro", "Não foi possível carregar a lista de cidades.", "OK");
        }
    }
    private void ClearForm()
    {
        RazaoSocialEntry.Text = string.Empty;
        NomeFantasiaEntry.Text = string.Empty;
        CnpjEntry.Text = string.Empty;
        EmailEntry.Text = string.Empty;
        TelefoneEntry.Text = string.Empty;
        EnderecoEntry.Text = string.Empty;
        CidadePicker.SelectedItem = null; // Clear selection
        DataCadastroPicker.Date = DateTime.Today; // Reset
        AtivoSwitch.IsToggled = true;

        RazaoSocialEntry.Focus();
    }

    private async void SalvarFornecedorButton_Clicked(object sender, EventArgs e)
    {
        // --- Basic Validation ---
        if (string.IsNullOrWhiteSpace(RazaoSocialEntry.Text))
        {
            await DisplayAlert("Campo Obrigatório", "Por favor, preencha a Razão Social.", "OK");
            RazaoSocialEntry.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(CnpjEntry.Text) || !IsValidCnpj(CnpjEntry.Text))
        {
            await DisplayAlert("CNPJ Inválido", "Por favor, insira um CNPJ válido (14 dígitos).", "OK");
            CnpjEntry.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || !IsValidEmail(EmailEntry.Text))
        {
            await DisplayAlert("Email Inválido", "Por favor, insira um email válido.", "OK");
            EmailEntry.Focus();
            return;
        }

        if (CidadePicker.SelectedItem == null)
        {
            await DisplayAlert("Campo Obrigatório", "Por favor, selecione uma cidade.", "OK");
            CidadePicker.Focus();
            return;
        }
        // Add more validation as needed (Telefone, Endereço)

        var selectedCidade = (CidadeModel)CidadePicker.SelectedItem;

        // --- Create FornecedorModel ---
        var Fornecedor = new FornecedorModel
        {
            RazaoSocial = RazaoSocialEntry.Text.Trim(),
            NomeFantasia = NomeFantasiaEntry.Text?.Trim(), // Nome Fantasia can be optional
            CNPJ = SanitizeCnpj(CnpjEntry.Text),
            Email = EmailEntry.Text.Trim(),
            Telefone = SanitizePhoneNumber(TelefoneEntry.Text),
            Endereco = EnderecoEntry.Text?.Trim(),
            CodCidade = selectedCidade.CodCIdade, // Get ID from selected city object
            DataCadastro = DateTime.Now, // Service layer also defaults this
            Ativo = AtivoSwitch.IsToggled
            // DataUltimaCompra is typically not set when creating a new supplier
        };

        try
        {
            if (_fornecedorId != 0)
            {
                Fornecedor.CodFornecedor = _fornecedorId; // Update
                var newFornecedor = await _fornecedorService.UpdateAsync(Fornecedor); // Use the UpdateAsync method from your Forn

                if (newFornecedor != 0) await DisplayAlert("Sucesso", "Fornecedor alterado com sucesso!", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                // Check if CNPJ already exists
                var existingByCnpj = await _fornecedorService.GetByCNPJAsync(Fornecedor.CNPJ);
                if (existingByCnpj != null)
                {
                    await DisplayAlert("Duplicidade", $"Já existe um fornecedor cadastrado com o CNPJ: {FormatCnpj(Fornecedor.CNPJ)}", "OK");
                    CnpjEntry.Focus();
                    return;
                }

                int newFornecedorId = await _fornecedorService.InsertAsync(Fornecedor);

                if (newFornecedorId > 0)
                {
                    await DisplayAlert("Sucesso", "Fornecedor cadastrado com sucesso!", "OK");
                    ClearForm();
                    // Optionally, navigate away or update a list
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Erro", "Não foi possível cadastrar o fornecedor. Verifique os dados e tente novamente.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving supplier: {ex.Message}");
            await DisplayAlert("Erro Inesperado", $"Ocorreu um erro ao salvar o fornecedor: {ex.Message}", "OK");
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

    #region Input Formatting and Validation Helpers

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch { return false; }
    }

    private string SanitizeCnpj(string cnpj)
    {
        return Regex.Replace(cnpj ?? string.Empty, @"[^\d]", ""); // Remove non-digits
    }

    private string FormatCnpj(string cnpjDigits) // Helper to show formatted CNPJ in messages
    {
        if (string.IsNullOrWhiteSpace(cnpjDigits) || cnpjDigits.Length != 14) return cnpjDigits;
        return $"{cnpjDigits.Substring(0, 2)}.{cnpjDigits.Substring(2, 3)}.{cnpjDigits.Substring(5, 3)}/{cnpjDigits.Substring(8, 4)}-{cnpjDigits.Substring(12, 2)}";
    }


    private bool IsValidCnpj(string cnpj)
    {
        string cleanCnpj = SanitizeCnpj(cnpj);
        if (string.IsNullOrWhiteSpace(cleanCnpj) || cleanCnpj.Length != 14)
            return false;

        // Basic check for all same digits (e.g., 00.000.000/0000-00)
        if (cleanCnpj.Distinct().Count() == 1)
            return false;

        // More complex CNPJ validation algorithm could be added here if needed.
        // For this example, we are just checking length and non-repeating digits.
        return true;
    }

    private string SanitizePhoneNumber(string phone)
    {
        return Regex.Replace(phone ?? string.Empty, @"[^\d]", ""); // Remove non-digits
    }

    // --- CNPJ Formatter ---
    private void CnpjEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            string digitsOnly = SanitizeCnpj(e.NewTextValue);

            if (digitsOnly.Length > 14)
            {
                digitsOnly = digitsOnly.Substring(0, 14);
            }

            string formattedCnpj = digitsOnly;
            // 00.000.000/0000-00
            if (digitsOnly.Length > 12)
            {
                formattedCnpj = $"{digitsOnly.Substring(0, 2)}.{digitsOnly.Substring(2, 3)}.{digitsOnly.Substring(5, 3)}/{digitsOnly.Substring(8, 4)}-{digitsOnly.Substring(12)}";
            }
            else if (digitsOnly.Length > 8)
            {
                formattedCnpj = $"{digitsOnly.Substring(0, 2)}.{digitsOnly.Substring(2, 3)}.{digitsOnly.Substring(5, 3)}/{digitsOnly.Substring(8)}";
            }
            else if (digitsOnly.Length > 5)
            {
                formattedCnpj = $"{digitsOnly.Substring(0, 2)}.{digitsOnly.Substring(2, 3)}.{digitsOnly.Substring(5)}";
            }
            else if (digitsOnly.Length > 2)
            {
                formattedCnpj = $"{digitsOnly.Substring(0, 2)}.{digitsOnly.Substring(2)}";
            }

            if (entry.Text != formattedCnpj)
            {
                entry.Text = formattedCnpj;
                entry.CursorPosition = formattedCnpj.Length;
            }
        }
    }

    // --- Telefone Formatter (Example) ---
    private void TelefoneEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            string digitsOnly = SanitizePhoneNumber(e.NewTextValue);
            string formattedTelefone = digitsOnly;

            if (digitsOnly.Length > 11) digitsOnly = digitsOnly.Substring(0, 11);

            if (digitsOnly.Length == 11) // Celular: (XX) XXXXX-XXXX
            {
                formattedTelefone = $"({digitsOnly.Substring(0, 2)}) {digitsOnly.Substring(2, 5)}-{digitsOnly.Substring(7, 4)}";
            }
            else if (digitsOnly.Length == 10) // Fixo: (XX) XXXX-XXXX
            {
                formattedTelefone = $"({digitsOnly.Substring(0, 2)}) {digitsOnly.Substring(2, 4)}-{digitsOnly.Substring(6, 4)}";
            }
            else if (digitsOnly.Length > 7)
            {
                // Partial formatting for fixed or mobile as user types
                if (digitsOnly.Length > 6 && (digitsOnly.Length - 2 - 4) > 0)
                { // Check if enough digits for hyphen
                    formattedTelefone = $"({digitsOnly.Substring(0, 2)}) {digitsOnly.Substring(2, digitsOnly.Length - 6)}-{digitsOnly.Substring(digitsOnly.Length - 4)}";
                }
                else if (digitsOnly.Length > 2)
                {
                    formattedTelefone = $"({digitsOnly.Substring(0, 2)}) {digitsOnly.Substring(2)}";
                }
            }
            else if (digitsOnly.Length > 2)
            {
                formattedTelefone = $"({digitsOnly.Substring(0, 2)}) {digitsOnly.Substring(2)}";
            }
            else if (digitsOnly.Length > 0)
            {
                formattedTelefone = $"({digitsOnly}";
            }

            if (entry.Text != formattedTelefone)
            {
                entry.Text = formattedTelefone;
                entry.CursorPosition = formattedTelefone.Length;
            }
        }
    }

    #endregion
}