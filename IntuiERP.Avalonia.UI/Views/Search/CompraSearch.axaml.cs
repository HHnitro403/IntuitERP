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

public partial class CompraSearch : UserControl
{
    private readonly CompraService _compraService;
    private readonly ItemCompraService _itemCompraService;
    private readonly FornecedorService _fornecedorService;
    private readonly VendedorService _vendedorService;
    private readonly ProdutoService _produtoService;
    private readonly EstoqueService _estoqueService;

    private ObservableCollection<CompraDisplayModel> _listaComprasDisplay = new();
    private List<CompraDisplayModel> _masterListaCompras = new();
    private CompraDisplayModel? _compraSelecionada;

    public CompraSearch()
    {
        InitializeComponent();
        
        var factory = new NpgsqlConnectionFactory();
        var connection = factory.CreateConnection();
        _compraService = new CompraService(connection);
        _itemCompraService = new ItemCompraService(connection);
        _fornecedorService = new FornecedorService(connection);
        _vendedorService = new VendedorService(connection);
        _produtoService = new ProdutoService(connection);
        _estoqueService = new EstoqueService(connection);

        ComprasListBox.ItemsSource = _listaComprasDisplay;
        
        ComprasListBox.SelectionChanged += ComprasListBox_SelectionChanged;
        CompraSearchBar.TextChanged += CompraSearchBar_TextChanged;
        NovaCompraButton.Click += NovaCompraButton_Clicked;
        EditarCompraButton.Click += EditarCompraButton_Clicked;
        GerarContaPagarButton.Click += GerarContaPagarButton_Clicked;
        ExcluirCompraButton.Click += ExcluirCompraButton_Clicked;
        BtnBack.Click += BtnBack_Clicked;

        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        await LoadComprasAsync();
    }

    private async Task LoadComprasAsync()
    {
        try
        {
            var compras = await _compraService.GetAllAsync();
            var fornecedores = await _fornecedorService.GetAllAsync();
            var vendedores = await _vendedorService.GetAllAsync();

            var fornecedoresDict = fornecedores.ToDictionary(f => f.CodFornecedor, f => f.NomeFantasia ?? f.RazaoSocial);
            var vendedoresDict = vendedores.ToDictionary(v => v.CodVendedor, v => v.NomeVendedor);

            var displayCompras = compras.Select(c => new CompraDisplayModel
            {
                CodCompra = c.CodCompra,
                DataCompra = c.data_compra,
                ValorTotal = c.valor_total ?? 0m,
                Status = c.status_compra == 1 ? "FINALIZADA" : (c.status_compra == 0 ? "PENDENTE" : "CANCELADA"),
                NomeFornecedor = c.CodFornec.HasValue && fornecedoresDict.TryGetValue(c.CodFornec.Value, out var fNome) ? fNome : "Desconhecido",
                NomeVendedor = c.CodVendedor.HasValue && vendedoresDict.TryGetValue(c.CodVendedor.Value, out var vNome) ? vNome : "Desconhecido"
            });

            _masterListaCompras = displayCompras.OrderByDescending(c => c.DataCompra).ToList();
            FilterCompras();
        }
        catch (Exception ex)
        {
            if (VisualRoot is Window window)
                await MessageBox.Show(window, $"Não foi possível carregar as compras: {ex.Message}", "Erro");
        }
    }

    private void FilterCompras()
    {
        string searchTerm = CompraSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        
        var filtered = _masterListaCompras
            .Where(c => string.IsNullOrEmpty(searchTerm) || 
                        (c.NomeFornecedor?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (c.NomeVendedor?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (c.CodCompra.ToString().Contains(searchTerm)) ||
                        (c.ValorTotal.ToString().Contains(searchTerm)));

        _listaComprasDisplay.Clear();
        foreach (var c in filtered)
            _listaComprasDisplay.Add(c);
            
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        bool isSelected = _compraSelecionada != null;
        EditarCompraButton.IsEnabled = isSelected;
        GerarContaPagarButton.IsEnabled = isSelected && _compraSelecionada?.Status == "FINALIZADA";
        ExcluirCompraButton.IsEnabled = isSelected;
    }

    private void ComprasListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _compraSelecionada = ComprasListBox.SelectedItem as CompraDisplayModel;
        UpdateActionButtonsState();
    }

    private void CompraSearchBar_TextChanged(object? sender, TextChangedEventArgs e)
    {
        FilterCompras();
    }

    private async void NovaCompraButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is Window window)
            await MessageBox.Show(window, "Módulo de Cadastro de Compra em desenvolvimento.", "Informação");
    }

    private async void EditarCompraButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_compraSelecionada != null && VisualRoot is Window window)
            await MessageBox.Show(window, "Edição de Compra em desenvolvimento.", "Informação");
    }

    private async void GerarContaPagarButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_compraSelecionada != null && VisualRoot is Window window)
            await MessageBox.Show(window, "Módulo de Geração de Conta em desenvolvimento.", "Informação");
    }

    private async void ExcluirCompraButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_compraSelecionada == null || VisualRoot is not Window window) return;

        try
        {
            int rowsAffected = await _compraService.DeleteAsync(_compraSelecionada.CodCompra);
            if (rowsAffected > 0)
            {
                await MessageBox.Show(window, "Compra excluída com sucesso!", "Sucesso");
                await LoadComprasAsync();
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