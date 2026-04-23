using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using IntuiERP.Avalonia.UI.models;
using IntuiERP.Avalonia.UI.Services;
using IntuiERP.Avalonia.UI.Helpers;
using IntuiERP.Avalonia.UI.Views.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.Views;

public partial class CadastroCliente : UserControl
{
    private readonly ClienteService _clienteService;
    private readonly CidadeService _cidadeService;
    private readonly int _clienteId;
    private List<CidadeModel> _cidades = new();

    public CadastroCliente(int id = 0)
    {
        InitializeComponent();
        
        var factory = new NpgsqlConnectionFactory();
        var connection = factory.CreateConnection();
        _clienteService = new ClienteService(connection);
        _cidadeService = new CidadeService(connection);
        _clienteId = id;

        DataCadastroPicker.SelectedDate = DateTime.Today;
        DataNascimentoPicker.SelectedDate = DateTime.Today.AddYears(-18);

        SalvarButton.Click += SalvarButton_Clicked;
        CancelarButton.Click += CancelarButton_Clicked;
        
        // Formatter event handlers (Avalonia uses TextChanged)
        CpfEntry.TextChanged += CpfEntry_TextChanged;
        CepEntry.TextChanged += CepEntry_TextChanged;
        TelefoneEntry.TextChanged += TelefoneEntry_TextChanged;

        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        await LoadCidadesAsync();

        if (_clienteId != 0)
        {
            HeaderLabel.Text = "Editar Cliente";
            try
            {
                var cliente = await _clienteService.GetByIdAsync(_clienteId);
                if (cliente != null)
                {
                    NomeEntry.Text = cliente.Nome;
                    EmailEntry.Text = cliente.Email;
                    TelefoneEntry.Text = cliente.Telefone;
                    DataNascimentoPicker.SelectedDate = cliente.DataNascimento;
                    CpfEntry.Text = cliente.CPF;
                    EnderecoEntry.Text = cliente.Endereco;
                    NumeroEntry.Text = cliente.Numero;
                    BairroEntry.Text = cliente.Bairro;
                    CepEntry.Text = cliente.CEP;
                    DataCadastroPicker.SelectedDate = cliente.DataCadastro;
                    AtivoSwitch.IsChecked = cliente.Ativo;

                    // Select correct city in ComboBox
                    var selectedCidade = _cidades.FirstOrDefault(c => c.CodCIdade == cliente.CodCidade);
                    if (selectedCidade != null)
                    {
                        CidadeComboBox.SelectedItem = selectedCidade;
                    }
                }
            }
            catch (Exception ex)
            {
                await MessageBox.Show(NavigationHelper.GetWindow(this), $"Erro ao carregar cliente: {ex.Message}", "Erro");
            }
        }
    }

    private async Task LoadCidadesAsync()
    {
        try
        {
            var list = await _cidadeService.GetAllAsync();
            _cidades = list.OrderBy(c => c.Cidade).ToList();
            CidadeComboBox.ItemsSource = _cidades;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading cities: {ex.Message}");
        }
    }

    private async void SalvarButton_Clicked(object? sender, RoutedEventArgs e)
    {
        var window = NavigationHelper.GetWindow(this);
        if (window == null) return;

        if (string.IsNullOrWhiteSpace(NomeEntry.Text))
        {
            await MessageBox.Show(window, "Por favor, preencha o Nome Completo.", "Campo Obrigatório");
            return;
        }

        if (CidadeComboBox.SelectedItem is not CidadeModel selectedCidade)
        {
            await MessageBox.Show(window, "Por favor, selecione uma Cidade.", "Campo Obrigatório");
            return;
        }

        var cliente = new ClienteModel
        {
            Nome = NomeEntry.Text.Trim(),
            Email = EmailEntry.Text?.Trim(),
            Telefone = SanitizeInput(TelefoneEntry.Text),
            DataNascimento = DataNascimentoPicker.SelectedDate ?? DateTime.Today,
            CPF = SanitizeInput(CpfEntry.Text),
            Endereco = EnderecoEntry.Text?.Trim(),
            Numero = NumeroEntry.Text?.Trim(),
            Bairro = BairroEntry.Text?.Trim(),
            CEP = SanitizeInput(CepEntry.Text),
            CodCidade = selectedCidade.CodCIdade,
            DataCadastro = DataCadastroPicker.SelectedDate ?? DateTime.Now,
            Ativo = AtivoSwitch.IsChecked == true
        };

        try
        {
            if (_clienteId != 0)
            {
                cliente.CodCliente = _clienteId;
                var result = await _clienteService.UpdateAsync(cliente);
                if (result > 0)
                {
                    await MessageBox.Show(window, "Cliente atualizado com sucesso!", "Sucesso");
                    NavigationHelper.NavigateTo(new ClienteSearch());
                }
            }
            else
            {
                int newId = await _clienteService.InsertAsync(cliente);
                if (newId > 0)
                {
                    await MessageBox.Show(window, "Cliente cadastrado com sucesso!", "Sucesso");
                    NavigationHelper.NavigateTo(new ClienteSearch());
                }
            }
        }
        catch (Exception ex)
        {
            await MessageBox.Show(window, $"Ocorreu um erro ao salvar o cliente: {ex.Message}", "Erro");
        }
    }

    private void CancelarButton_Clicked(object? sender, RoutedEventArgs e)
    {
        NavigationHelper.NavigateTo(new ClienteSearch());
    }

    private string SanitizeInput(string? input) => Regex.Replace(input ?? string.Empty, @"[^\d]", "");

    private void CpfEntry_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            string digits = SanitizeInput(textBox.Text);
            if (digits.Length > 11) digits = digits.Substring(0, 11);
            
            string formatted = digits;
            if (digits.Length > 9) formatted = $"{digits.Substring(0, 3)}.{digits.Substring(3, 3)}.{digits.Substring(6, 3)}-{digits.Substring(9)}";
            else if (digits.Length > 6) formatted = $"{digits.Substring(0, 3)}.{digits.Substring(3, 3)}.{digits.Substring(6)}";
            else if (digits.Length > 3) formatted = $"{digits.Substring(0, 3)}.{digits.Substring(3)}";

            if (textBox.Text != formatted)
            {
                textBox.Text = formatted;
                textBox.CaretIndex = formatted.Length;
            }
        }
    }

    private void CepEntry_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            string digits = SanitizeInput(textBox.Text);
            if (digits.Length > 8) digits = digits.Substring(0, 8);
            
            string formatted = digits;
            if (digits.Length > 5) formatted = $"{digits.Substring(0, 5)}-{digits.Substring(5)}";

            if (textBox.Text != formatted)
            {
                textBox.Text = formatted;
                textBox.CaretIndex = formatted.Length;
            }
        }
    }

    private void TelefoneEntry_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            string digits = SanitizeInput(textBox.Text);
            if (digits.Length > 11) digits = digits.Substring(0, 11);
            
            string formatted = digits;
            if (digits.Length == 11) formatted = $"({digits.Substring(0, 2)}) {digits.Substring(2, 5)}-{digits.Substring(7, 4)}";
            else if (digits.Length == 10) formatted = $"({digits.Substring(0, 2)}) {digits.Substring(2, 4)}-{digits.Substring(6, 4)}";
            else if (digits.Length > 2) formatted = $"({digits.Substring(0, 2)}) {digits.Substring(2)}";
            else if (digits.Length > 0) formatted = $"({digits}";

            if (textBox.Text != formatted)
            {
                textBox.Text = formatted;
                textBox.CaretIndex = formatted.Length;
            }
        }
    }
}
