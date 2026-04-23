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

public partial class ContasReceberSearch : UserControl
{
    private readonly ContaReceberService _contaReceberService;
    private readonly ParcelaReceberService _parcelaReceberService;
    private readonly VendaService _vendaService;
    private readonly SystemSettingsService _settingsService;
    private readonly ClienteService _clienteService;

    private ObservableCollection<ContaReceberModel> _listaContasDisplay = new();
    private List<ContaReceberModel> _masterListaContas = new();
    private ContaReceberModel? _contaSelecionada;

    public ContasReceberSearch()
    {
        InitializeComponent();
        
        var factory = new NpgsqlConnectionFactory();
        var connection = factory.CreateConnection();
        _contaReceberService = new ContaReceberService(connection);
        _parcelaReceberService = new ParcelaReceberService(connection, _contaReceberService);
        _vendaService = new VendaService(connection);
        _settingsService = new SystemSettingsService(connection);
        _clienteService = new ClienteService(connection);

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
            var contas = await _contaReceberService.GetAllAsync();
            var clientes = await _clienteService.GetAllAsync();
            var clientesDict = clientes.ToDictionary(c => c.CodCliente, c => c.Nome);

            foreach (var conta in contas)
            {
                if (clientesDict.TryGetValue(conta.CodCliente, out var nome))
                    conta.ClienteNome = nome;
            }

            _masterListaContas = contas.OrderByDescending(c => c.DataEmissao).ToList();
            FilterContas();
            UpdateDashboard();
        }
        catch (Exception ex)
        {
            await MessageBox.Show(NavigationHelper.GetWindow(this), $"Não foi possível carregar as contas: {ex.Message}", "Erro");
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
                        (c.ClienteNome?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (c.Id.ToString().Contains(searchTerm)) ||
                        (c.CodVenda.ToString().Contains(searchTerm)) ||
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
        _contaSelecionada = ContasListBox.SelectedItem as ContaReceberModel;
        UpdateActionButtonsState();
    }

    private void ContasSearchBar_TextChanged(object? sender, TextChangedEventArgs e)
    {
        FilterContas();
    }

    private async void NovaContaButton_Clicked(object? sender, RoutedEventArgs e)
    {
        await MessageBox.Show(NavigationHelper.GetWindow(this), "Módulo de Cadastro de Conta em desenvolvimento.", "Informação");
    }

    private async void VerParcelasButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_contaSelecionada != null)
            await MessageBox.Show(NavigationHelper.GetWindow(this), "Visualização de Parcelas em desenvolvimento.", "Informação");
    }

    private async void EditarContaButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_contaSelecionada != null)
            await MessageBox.Show(NavigationHelper.GetWindow(this), "Edição de Conta em desenvolvimento.", "Informação");
    }

    private async void ExcluirContaButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_contaSelecionada == null) return;
        var window = NavigationHelper.GetWindow(this);

        try
        {
            int rowsAffected = await _contaReceberService.DeleteAsync(_contaSelecionada.Id);
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
        NavigationHelper.NavigateTo(new MenuPage());
    }
}
