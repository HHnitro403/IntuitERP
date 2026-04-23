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

public partial class CadastroFornecedor : UserControl
{
    private readonly FornecedorService _fornecedorService;
    private readonly CidadeService _cidadeService;
    private readonly int _fornecedorId;
    private List<CidadeModel> _cidades = new();

    public CadastroFornecedor(int id = 0)
    {
        InitializeComponent();
        
        var factory = new NpgsqlConnectionFactory();
        var connection = factory.CreateConnection();
        _fornecedorService = new FornecedorService(connection);
        _cidadeService = new CidadeService(connection);
        _fornecedorId = id;

        DataCadastroPicker.SelectedDate = DateTime.Today;

        SalvarFornecedorButton.Click += SalvarFornecedorButton_Clicked;
        CancelarButton.Click += CancelarButton_Clicked;
        
        CnpjEntry.TextChanged += CnpjEntry_TextChanged;
        TelefoneEntry.TextChanged += TelefoneEntry_TextChanged;

        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        await LoadCidadesAsync();

        if (_fornecedorId != 0)
        {
            HeaderLabel.Text = "Editar Fornecedor";
            try
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
                    DataCadastroPicker.SelectedDate = fornecedor.DataCadastro;
                    AtivoSwitch.IsChecked = fornecedor.Ativo;

                    var selectedCidade = _cidades.FirstOrDefault(c => c.CodCIdade == fornecedor.CodCidade);
                    if (selectedCidade != null)
                    {
                        CidadeComboBox.SelectedItem = selectedCidade;
                    }
                }
            }
            catch (Exception ex)
            {
                await MessageBox.Show(NavigationHelper.GetWindow(this), $"Erro ao carregar fornecedor: {ex.Message}", "Erro");
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

    private async void SalvarFornecedorButton_Clicked(object? sender, RoutedEventArgs e)
    {
        var window = NavigationHelper.GetWindow(this);
        if (window == null) return;

        if (string.IsNullOrWhiteSpace(RazaoSocialEntry.Text))
        {
            await MessageBox.Show(window, "Por favor, preencha a Razão Social.", "Campo Obrigatório");
            return;
        }

        if (string.IsNullOrWhiteSpace(CnpjEntry.Text))
        {
            await MessageBox.Show(window, "Por favor, preencha o CNPJ.", "Campo Obrigatório");
            return;
        }

        if (CidadeComboBox.SelectedItem is not CidadeModel selectedCidade)
        {
            await MessageBox.Show(window, "Por favor, selecione uma Cidade.", "Campo Obrigatório");
            return;
        }

        var fornecedor = new FornecedorModel
        {
            RazaoSocial = RazaoSocialEntry.Text.Trim(),
            NomeFantasia = NomeFantasiaEntry.Text?.Trim(),
            CNPJ = SanitizeInput(CnpjEntry.Text),
            Email = EmailEntry.Text?.Trim(),
            Telefone = SanitizeInput(TelefoneEntry.Text),
            Endereco = EnderecoEntry.Text?.Trim(),
            CodCidade = selectedCidade.CodCIdade,
            DataCadastro = DataCadastroPicker.SelectedDate ?? DateTime.Now,
            Ativo = AtivoSwitch.IsChecked == true
        };

        try
        {
            if (_fornecedorId != 0)
            {
                fornecedor.CodFornecedor = _fornecedorId;
                var result = await _fornecedorService.UpdateAsync(fornecedor);
                if (result > 0)
                {
                    await MessageBox.Show(window, "Fornecedor atualizado com sucesso!", "Sucesso");
                    NavigationHelper.NavigateTo(new FornecedorSearch());
                }
            }
            else
            {
                int newId = await _fornecedorService.InsertAsync(fornecedor);
                if (newId > 0)
                {
                    await MessageBox.Show(window, "Fornecedor cadastrado com sucesso!", "Sucesso");
                    NavigationHelper.NavigateTo(new FornecedorSearch());
                }
            }
        }
        catch (Exception ex)
        {
            await MessageBox.Show(window, $"Ocorreu um erro ao salvar o fornecedor: {ex.Message}", "Erro");
        }
    }

    private void CancelarButton_Clicked(object? sender, RoutedEventArgs e)
    {
        NavigationHelper.NavigateTo(new FornecedorSearch());
    }

    private string SanitizeInput(string? input) => Regex.Replace(input ?? string.Empty, @"[^\d]", "");

    private void CnpjEntry_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            string digits = SanitizeInput(textBox.Text);
            if (digits.Length > 14) digits = digits.Substring(0, 14);
            
            string formatted = digits;
            if (digits.Length > 12) formatted = $"{digits.Substring(0, 2)}.{digits.Substring(2, 3)}.{digits.Substring(5, 3)}/{digits.Substring(8, 4)}-{digits.Substring(12)}";
            else if (digits.Length > 8) formatted = $"{digits.Substring(0, 2)}.{digits.Substring(2, 3)}.{digits.Substring(5, 3)}/{digits.Substring(8)}";
            else if (digits.Length > 5) formatted = $"{digits.Substring(0, 2)}.{digits.Substring(2, 3)}.{digits.Substring(5)}";
            else if (digits.Length > 2) formatted = $"{digits.Substring(0, 2)}.{digits.Substring(2)}";

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
