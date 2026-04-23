using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using IntuiERP.Avalonia.UI.models;
using IntuiERP.Avalonia.UI.Services;
using IntuiERP.Avalonia.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.Views.Search;

public partial class ContasPagarSearch : UserControl
{
    private readonly ContaPagarService _contaPagarService;
    private readonly ParcelaPagarService _parcelaPagarService;
    private readonly CompraService _compraService;
    private readonly SystemSettingsService _settingsService;
    private readonly FornecedorService _fornecedorService;

    private ObservableCollection<ContaPagarModel> _listaContasDisplay = new();
    private List<ContaPagarModel> _masterListaContas = new();
    private ContaPagarModel? _contaSelecionada;

    public ContasPagarSearch()
    {
        InitializeComponent();
        
        var factory = new NpgsqlConnectionFactory();
        var connection = factory.CreateConnection();
        _contaPagarService = new ContaPagarService(connection);
        _parcelaPagarService = new ParcelaPagarService(connection, _contaPagarService);
        _compraService = new CompraService(connection);
        _settingsService = new SystemSettingsService(connection);
        _fornecedorService = new FornecedorService(connection);

        ContasListBox.ItemsSource = _listaContasDisplay;
        
        ContasListBox.SelectionChanged += ContasListBox_SelectionChanged;
        ContasSearchBar.TextChanged += ContasSearchBar_TextChanged;
        NovaContaButton.Click += NovaContaButton_Clicked;
        VerParcelasButton.Click += VerParcelasButton_Clicked;
        EditarContaButton.Click += EditarContaButton_Clicked;
        ExcluirContaButton.Click += ExcluirContaButton_Clicked;
        BtnBack.Click += BtnBack_Clicked;

        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        await LoadContasAsync();
    }

    private async Task LoadContasAsync()
    {
        try
        {
            var contas = await _contaPagarService.GetAllAsync();
            var fornecedores = await _fornecedorService.GetAllAsync();
            var fornecedoresDict = fornecedores.ToDictionary(f => f.CodFornecedor, f => f.NomeFantasia ?? f.RazaoSocial);

            foreach (var conta in contas)
            {
                if (fornecedoresDict.TryGetValue(conta.CodFornecedor, out var fNome))
                    conta.FornecedorNome = fNome;
            }

            _masterListaContas = contas.OrderByDescending(c => c.DataEmissao).ToList();
            FilterContas();
            UpdateDashboard();
        }
        catch (Exception ex)
        {
            if (VisualRoot is Window window)
                await MessageBox.Show(window, $"Não foi possível carregar as contas: {ex.Message}", "Erro");
        }
    }

    private void UpdateDashboard()
    {
        var pendentes = _masterListaContas.Where(c => c.Status == "PENDENTE").ToList();
        var vencidas = _masterListaContas.Where(c => c.Status == "VENCIDA").ToList();
        var parciais = _masterListaContas.Where(c => c.Status == "PARCIAL").ToList();
        var pagas = _masterListaContas.Where(c => c.Status == "PAGO").ToList();

        TotalPendenteLabel.Text = pendentes.Sum(c => c.ValorPendente).ToString("C2");
        CountPendenteLabel.Text = $"{pendentes.Count} contas";

        TotalVencidoLabel.Text = vencidas.Sum(c => c.ValorPendente).ToString("C2");
        CountVencidoLabel.Text = $"{vencidas.Count} contas";

        TotalParcialLabel.Text = parciais.Sum(c => c.ValorPendente).ToString("C2");
        CountParcialLabel.Text = $"{parciais.Count} contas";

        TotalPagoLabel.Text = pagas.Sum(c => c.ValorTotal).ToString("C2");
        CountPagoLabel.Text = $"{pagas.Count} contas";
    }

    private void FilterContas()
    {
        string searchTerm = ContasSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        
        var filtered = _masterListaContas
            .Where(c => string.IsNullOrEmpty(searchTerm) || 
                        (c.FornecedorNome?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (c.Id.ToString().Contains(searchTerm)) ||
                        (c.CodCompra.ToString().Contains(searchTerm)) ||
                        (c.ValorTotal.ToString().Contains(searchTerm)));

        _listaContasDisplay.Clear();
        foreach (var c in filtered)
            _listaContasDisplay.Add(c);
            
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        bool isSelected = _contaSelecionada != null;
        VerParcelasButton.IsEnabled = isSelected;
        EditarContaButton.IsEnabled = isSelected;
        ExcluirContaButton.IsEnabled = isSelected;
    }

    private void ContasListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _contaSelecionada = ContasListBox.SelectedItem as ContaPagarModel;
        UpdateActionButtonsState();
    }

    private void ContasSearchBar_TextChanged(object? sender, TextChangedEventArgs e)
    {
        FilterContas();
    }

    private async void NovaContaButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is Window window)
            await MessageBox.Show(window, "Módulo de Cadastro de Conta em desenvolvimento.", "Informação");
    }

    private async void VerParcelasButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_contaSelecionada != null && VisualRoot is Window window)
            await MessageBox.Show(window, "Visualização de Parcelas em desenvolvimento.", "Informação");
    }

    private async void EditarContaButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_contaSelecionada != null && VisualRoot is Window window)
            await MessageBox.Show(window, "Edição de Conta em desenvolvimento.", "Informação");
    }

    private async void ExcluirContaButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_contaSelecionada == null || VisualRoot is not Window window) return;

        try
        {
            int rowsAffected = await _contaPagarService.DeleteAsync(_contaSelecionada.Id);
            if (rowsAffected > 0)
            {
                await MessageBox.Show(window, "Conta excluída com sucesso!", "Sucesso");
                await LoadContasAsync();
            }
        }
        catch (Exception ex)
        {
            await MessageBox.Show(window, ex.Message, "Erro");
        }
    }

    private void BtnBack_Clicked(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is Window window)
        {
            window.Content = new MenuPage();
        }
    }
}