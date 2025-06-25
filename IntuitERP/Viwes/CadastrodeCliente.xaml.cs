using IntuitERP.models;
using IntuitERP.Services;
using IntuitERP.Viwes.Modals;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace IntuitERP.Viwes;

public partial class CadastrodeCliente : ContentPage
{
    private readonly ClienteService _clienteService;
    private readonly CidadeService _cidadeService; // Uncomment if you implement a Cidade Picker
    private readonly int _id = 0;

    // Constructor for Dependency Injection (recommended)
    // Register ClienteService (and CidadeService if used) in MauiProgram.cs
    public CadastrodeCliente(ClienteService clienteService, CidadeService cidadeService, int id = 0)
    {
        InitializeComponent();
        _clienteService = clienteService;
        _cidadeService = cidadeService; // Uncomment if using CidadeService

        // Set default date for DataCadastroPicker (usually today for a new record)
        // and since it's disabled, this is more for visual consistency if it were enabled.
        DataCadastroPicker.Date = DateTime.Today;
        DataNascimentoPicker.Date = DateTime.Today.AddYears(-18); // Default to 18 years ago
        DataNascimentoPicker.MaximumDate = DateTime.Today; // Cannot be born in the future

        _id = id;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_id != 0)
        {
            var cliente = await _clienteService.GetByIdAsync(_id);
            var cidade = await _cidadeService.GetByIdAsync(cliente.CodCidade);

            NomeEntry.Text = cliente?.Nome ?? string.Empty;
            EmailEntry.Text = cliente?.Email ?? string.Empty;
            TelefoneEntry.Text = cliente?.Telefone ?? string.Empty;
            DataNascimentoPicker.Date = cliente?.DataNascimento ?? DateTime.Today.AddYears(-18);
            CpfEntry.Text = cliente?.CPF ?? string.Empty;
            EnderecoEntry.Text = cliente?.Endereco ?? string.Empty;
            NumeroEntry.Text = cliente?.Numero ?? string.Empty;
            BairroEntry.Text = cliente?.Bairro ?? string.Empty;
            CepEntry.Text = cliente?.CEP ?? string.Empty;
            //CodCidadeEntry.Text = cliente?.CodCidade.ToString() ?? string.Empty;
            DataCadastroPicker.Date = cliente?.DataCadastro ?? DateTime.Today;
            AtivoSwitch.IsToggled = cliente?.Ativo ?? true;

            NomeEntry.Focus();
        }
    }

    private void ClearForm()
    {
        NomeEntry.Text = string.Empty;
        EmailEntry.Text = string.Empty;
        TelefoneEntry.Text = string.Empty;
        DataNascimentoPicker.Date = DateTime.Today.AddYears(-18); // Reset to default
        CpfEntry.Text = string.Empty;
        EnderecoEntry.Text = string.Empty;
        NumeroEntry.Text = string.Empty;
        BairroEntry.Text = string.Empty;
        CepEntry.Text = string.Empty;
        // CodCidadeEntry.Text = string.Empty; // In a real app, you'd reset a Picker
        DataCadastroPicker.Date = DateTime.Today; // Reset, though it's disabled
        AtivoSwitch.IsToggled = true;

        NomeEntry.Focus();
    }

    // Event handler for Save button click

    private async void SalvarButton_Clicked(object sender, EventArgs e)
    {
        // --- Basic Validation ---
        if (string.IsNullOrWhiteSpace(NomeEntry.Text))
        {
            await DisplayAlert("Campo Obrigatório", "Por favor, preencha o Nome Completo.", "OK");
            NomeEntry.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || !IsValidEmail(EmailEntry.Text))
        {
            await DisplayAlert("Email Inválido", "Por favor, insira um email válido.", "OK");
            EmailEntry.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(CpfEntry.Text) || !IsValidCpf(CpfEntry.Text))
        {
            await DisplayAlert("CPF Inválido", "Por favor, insira um CPF válido (11 dígitos).", "OK");
            CpfEntry.Focus();
            return;
        }

        //if (string.IsNullOrWhiteSpace(CodCidadeEntry.Text) || !int.TryParse(CodCidadeEntry.Text, out _))
        //{
        //    await DisplayAlert("Código da Cidade Inválido", "Por favor, insira um código da cidade numérico válido.", "OK");
        //    CodCidadeEntry.Focus();
        //    return;
        //}
        // Add more validation as needed (e.g., for Telefone, CEP format, Endereco, etc.)

        // --- Create ClienteModel ---
        var Cliente = new ClienteModel
        {
            Nome = NomeEntry.Text.Trim(),
            Email = EmailEntry.Text.Trim(),
            Telefone = SanitizePhoneNumber(TelefoneEntry.Text),
            DataNascimento = DataNascimentoPicker.Date,
            CPF = SanitizeCpf(CpfEntry.Text),
            Endereco = EnderecoEntry.Text?.Trim(),
            Numero = NumeroEntry.Text?.Trim(),
            Bairro = BairroEntry.Text?.Trim(),
            CEP = SanitizeCep(CepEntry.Text),
            //  CodCidade = int.Parse(CodCidadeEntry.Text), // Already validated as int
            DataCadastro = DateTime.Now, // Service layer also defaults this if null
            Ativo = AtivoSwitch.IsToggled
            // DataUltimaCompra is typically not set when creating a new client
        };

        try
        {
            if (_id != 0)
            {
                Cliente.CodCliente = _id;
                var UpdateCliente = await _clienteService.UpdateAsync(Cliente);

                if (UpdateCliente != 0)
                {
                    await DisplayAlert("Sucesso", "Cliente alterado com sucesso!", "OK");
                    await Navigation.PopAsync();
                }
            }
            else
            {
                // Check if CPF or Email already exists (optional, but good practice)
                var existingByCpf = await _clienteService.GetByCPFAsync(Cliente.CPF);
                if (existingByCpf != null)
                {
                    await DisplayAlert("Duplicidade", $"Já existe um cliente cadastrado com o CPF: {Cliente.CPF}", "OK");
                    CpfEntry.Focus();
                    return;
                }

                var existingByEmail = await _clienteService.GetByEmailAsync(Cliente.Email);
                if (existingByEmail != null)
                {
                    await DisplayAlert("Duplicidade", $"Já existe um cliente cadastrado com o Email: {Cliente.Email}", "OK");
                    EmailEntry.Focus();
                    return;
                }

                int newClienteId = await _clienteService.InsertAsync(Cliente);

                if (newClienteId > 0)
                {
                    await DisplayAlert("Sucesso", "Cliente cadastrado com sucesso!", "OK");
                    ClearForm();
                    // Optionally, navigate away or update a list if this page is part of a larger flow
                    // await Navigation.PopAsync(); // Example: if this was a modal or pushed page
                }
                else
                {
                    await DisplayAlert("Erro", "Não foi possível cadastrar o cliente. Verifique os dados e tente novamente.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving client: {ex.Message}");
            // More specific error handling can be added here, e.g., for database constraint violations
            await DisplayAlert("Erro Inesperado", $"Ocorreu um erro ao salvar o cliente: {ex.Message}", "OK");
        }
    }

    private async void CancelarButton_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Cancelar Cadastro", "Tem certeza que deseja cancelar o cadastro? Todas as informações não salvas serão perdidas.", "Sim", "Não");
        if (confirm)
        {
            ClearForm();
            // Optionally navigate back if this page was pushed onto the navigation stack
            if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync();
            }
        }
    }

    #region Input Formatting and Validation Helpers

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        try
        {
            // Uses System.Net.Mail.MailAddress for basic validation
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private string SanitizeCpf(string cpf)
    {
        return Regex.Replace(cpf ?? string.Empty, @"[^\d]", ""); // Remove non-digits
    }

    private bool IsValidCpf(string cpf)
    {
        string cleanCpf = SanitizeCpf(cpf);
        if (string.IsNullOrWhiteSpace(cleanCpf) || cleanCpf.Length != 11)
            return false;

        // Basic check for all same digits (e.g., 111.111.111-11)
        if (cleanCpf.Distinct().Count() == 1)
            return false;

        // More complex CPF validation algorithm could be added here if needed
        // For this example, we are just checking length and non-repeating digits.
        return true;
    }

    private string SanitizePhoneNumber(string phone)
    {
        return Regex.Replace(phone ?? string.Empty, @"[^\d]", ""); // Remove non-digits
    }

    private string SanitizeCep(string cep)
    {
        return Regex.Replace(cep ?? string.Empty, @"[^\d]", ""); // Remove non-digits
    }

    // --- CPF Formatter ---
    private void CpfEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            string digitsOnly = SanitizeCpf(e.NewTextValue);

            if (digitsOnly.Length > 11)
            {
                digitsOnly = digitsOnly.Substring(0, 11);
            }

            string formattedCpf = digitsOnly;
            if (digitsOnly.Length > 9)
            {
                formattedCpf = $"{digitsOnly.Substring(0, 3)}.{digitsOnly.Substring(3, 3)}.{digitsOnly.Substring(6, 3)}-{digitsOnly.Substring(9)}";
            }
            else if (digitsOnly.Length > 6)
            {
                formattedCpf = $"{digitsOnly.Substring(0, 3)}.{digitsOnly.Substring(3, 3)}.{digitsOnly.Substring(6)}";
            }
            else if (digitsOnly.Length > 3)
            {
                formattedCpf = $"{digitsOnly.Substring(0, 3)}.{digitsOnly.Substring(3)}";
            }

            // Avoid infinite loop by checking if text actually changed
            if (entry.Text != formattedCpf)
            {
                entry.Text = formattedCpf;
                entry.CursorPosition = formattedCpf.Length; // Move cursor to end
            }
        }
    }

    // --- CEP Formatter ---
    private void CepEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            string digitsOnly = SanitizeCep(e.NewTextValue);

            if (digitsOnly.Length > 8)
            {
                digitsOnly = digitsOnly.Substring(0, 8);
            }

            string formattedCep = digitsOnly;
            if (digitsOnly.Length > 5)
            {
                formattedCep = $"{digitsOnly.Substring(0, 5)}-{digitsOnly.Substring(5)}";
            }

            if (entry.Text != formattedCep)
            {
                entry.Text = formattedCep;
                entry.CursorPosition = formattedCep.Length;
            }
        }
    }

    // --- Telefone Formatter (Simple Example: (XX) XXXXX-XXXX or (XX) XXXX-XXXX) ---
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
            else if (digitsOnly.Length > 7) // (XX) XXXX-X...
            {
                formattedTelefone = $"({digitsOnly.Substring(0, 2)}) {digitsOnly.Substring(2, digitsOnly.Length - 7)}-{digitsOnly.Substring(digitsOnly.Length - 4)}";
            }
            else if (digitsOnly.Length > 2) // (XX) ...
            {
                formattedTelefone = $"({digitsOnly.Substring(0, 2)}) {digitsOnly.Substring(2)}";
            }
            else if (digitsOnly.Length > 0) // (X...
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

    #endregion Input Formatting and Validation Helpers

    private async void SelectCidadeButton_Clicked(object sender, EventArgs e)
    {
    }
}