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

public partial class VendaSearch : UserControl
{
    private readonly VendaService _vendaService;
    private readonly ItemVendaService _itemVendaService;
    private readonly ClienteService _clienteService;
    private readonly VendedorService _vendedorService;
    private readonly ProdutoService _produtoService;
    private readonly EstoqueService _estoqueService;

    private ObservableCollection<VendaDisplayModel> _listaVendasDisplay = new();
    private List<VendaDisplayModel> _masterListaVendas = new();
    private VendaDisplayModel? _vendaSelecionada;

    public VendaSearch()
    {
        InitializeComponent();
        
        var factory = new NpgsqlConnectionFactory();
        var connection = factory.CreateConnection();
        _vendaService = new VendaService(connection);
        _itemVendaService = new ItemVendaService(connection);
        _clienteService = new ClienteService(connection);
        _vendedorService = new VendedorService(connection);
        _produtoService = new ProdutoService(connection);
        _estoqueService = new EstoqueService(connection);

        VendasListBox.ItemsSource = _listaVendasDisplay;
        
        VendasListBox.SelectionChanged += VendasListBox_SelectionChanged;
        VendaSearchBar.TextChanged += VendaSearchBar_TextChanged;
        NovaVendaButton.Click += NovaVendaButton_Clicked;
        EditarVendaButton.Click += EditarVendaButton_Clicked;
        GerarContaReceberButton.Click += GerarContaReceberButton_Clicked;
        ExcluirVendaButton.Click += ExcluirVendaButton_Clicked;
        BtnBack.Click += BtnBack_Clicked;

        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        await LoadVendasAsync();
    }

    private async Task LoadVendasAsync()
    {
        try
        {
            var vendas = await _vendaService.GetAllAsync();
            var clientes = await _clienteService.GetAllAsync();
            var vendedores = await _vendedorService.GetAllAsync();

            var clientesDict = clientes.ToDictionary(c => c.CodCliente, c => c.Nome);
            var vendedoresDict = vendedores.ToDictionary(v => v.CodVendedor, v => v.NomeVendedor);

            var displayVendas = vendas.Select(v => new VendaDisplayModel
            {
                CodVenda = v.CodVenda,
                DataVenda = v.data_venda,
                ValorTotal = v.valor_total,
                Status = v.status_venda == 1 ? "FINALIZADA" : (v.status_venda == 0 ? "PENDENTE" : "CANCELADA"),
                NomeCliente = v.CodCliente > 0 && clientesDict.TryGetValue(v.CodCliente, out var cNome) ? cNome : "Desconhecido",
                NomeVendedor = v.CodVendedor.HasValue && vendedoresDict.TryGetValue(v.CodVendedor.Value, out var vNome) ? vNome : "Desconhecido"
            });

            _masterListaVendas = displayVendas.OrderByDescending(v => v.DataVenda).ToList();
            FilterVendas();
        }
        catch (Exception ex)
        {
            await MessageBox.Show(NavigationHelper.GetWindow(this), $"Não foi possível carregar as vendas: {ex.Message}", "Erro");
        }
    }

    private void FilterVendas()
    {
        string searchTerm = VendaSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        
        var filtered = _masterListaVendas
            .Where(v => string.IsNullOrEmpty(searchTerm) || 
                        (v.NomeCliente?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (v.NomeVendedor?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (v.CodVenda.ToString().Contains(searchTerm)) ||
                        (v.ValorTotal.ToString().Contains(searchTerm)));

        _listaVendasDisplay.Clear();
        foreach (var v in filtered)
            _listaVendasDisplay.Add(v);
            
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        bool isSelected = _vendaSelecionada != null;
        EditarVendaButton.IsEnabled = isSelected;
        GerarContaReceberButton.IsEnabled = isSelected && _vendaSelecionada?.Status == "FINALIZADA";
        ExcluirVendaButton.IsEnabled = isSelected;
    }

    private void VendasListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _vendaSelecionada = VendasListBox.SelectedItem as VendaDisplayModel;
        UpdateActionButtonsState();
    }

    private void VendaSearchBar_TextChanged(object? sender, TextChangedEventArgs e)
    {
        FilterVendas();
    }

    private async void NovaVendaButton_Clicked(object? sender, RoutedEventArgs e)
    {
        await MessageBox.Show(NavigationHelper.GetWindow(this), "Módulo de Cadastro de Venda em desenvolvimento.", "Informação");
    }

    private async void EditarVendaButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_vendaSelecionada != null)
            await MessageBox.Show(NavigationHelper.GetWindow(this), "Edição de Venda em desenvolvimento.", "Informação");
    }

    private async void GerarContaReceberButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_vendaSelecionada != null)
            await MessageBox.Show(NavigationHelper.GetWindow(this), "Módulo de Geração de Conta em desenvolvimento.", "Informação");
    }

    private async void ExcluirVendaButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_vendaSelecionada == null) return;
        var window = NavigationHelper.GetWindow(this);

        try
        {
            int rowsAffected = await _vendaService.DeleteAsync(_vendaSelecionada.CodVenda);
            if (rowsAffected > 0)
            {
                await MessageBox.Show(window, "Venda excluída com sucesso!", "Sucesso");
                await LoadVendasAsync();
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
