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

public partial class EstoqueSearch : UserControl
{
    private readonly EstoqueService _estoqueService;
    private readonly ProdutoService _produtoService;
    private ObservableCollection<EstoqueModel> _listaEstoqueDisplay = new();
    private List<EstoqueModel> _masterListaEstoque = new();
    private EstoqueModel? _movimentacaoSelecionada;

    public EstoqueSearch()
    {
        InitializeComponent();
        
        var factory = new NpgsqlConnectionFactory();
        var connection = factory.CreateConnection();
        _estoqueService = new EstoqueService(connection);
        _produtoService = new ProdutoService(connection);

        EstoqueListBox.ItemsSource = _listaEstoqueDisplay;
        
        EstoqueListBox.SelectionChanged += EstoqueListBox_SelectionChanged;
        EstoqueSearchBar.TextChanged += EstoqueSearchBar_TextChanged;
        NovaMovimentacaoButton.Click += NovaMovimentacaoButton_Clicked;
        ExcluirMovimentacaoButton.Click += ExcluirMovimentacaoButton_Clicked;
        BtnBack.Click += BtnBack_Clicked;

        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        await LoadEstoqueAsync();
    }

    private async Task LoadEstoqueAsync()
    {
        try
        {
            var estoque = await _estoqueService.GetAllAsync();
            var produtos = await _produtoService.GetAllAsync();
            var produtosDict = produtos.ToDictionary(p => p.CodProduto, p => p.Descricao);

            foreach (var item in estoque)
            {
                if (produtosDict.TryGetValue(item.CodProduto, out var nome))
                    item.ProdutoNome = nome;
                
                item.TipoDescricao = item.Tipo == 'E' ? "ENTRADA" : "SAÍDA";
            }

            _masterListaEstoque = estoque.OrderByDescending(e => e.Data).ThenByDescending(e => e.CodEst).ToList();
            FilterEstoque();
        }
        catch (Exception ex)
        {
            if (VisualRoot is Window window)
                await MessageBox.Show(window, $"Não foi possível carregar o histórico de estoque: {ex.Message}", "Erro");
        }
    }

    private void FilterEstoque()
    {
        string searchTerm = EstoqueSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        
        var filtered = _masterListaEstoque
            .Where(e => string.IsNullOrEmpty(searchTerm) || 
                        (e.ProdutoNome?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (e.CodProduto.ToString().Contains(searchTerm)) ||
                        (e.CodEst.ToString().Contains(searchTerm)));

        _listaEstoqueDisplay.Clear();
        foreach (var e in filtered)
            _listaEstoqueDisplay.Add(e);
            
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        ExcluirMovimentacaoButton.IsEnabled = _movimentacaoSelecionada != null;
    }

    private void EstoqueListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _movimentacaoSelecionada = EstoqueListBox.SelectedItem as EstoqueModel;
        UpdateActionButtonsState();
    }

    private void EstoqueSearchBar_TextChanged(object? sender, TextChangedEventArgs e)
    {
        FilterEstoque();
    }

    private void NovaMovimentacaoButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is Window window)
        {
            window.Content = new CadastroEstoque();
        }
    }

    private async void ExcluirMovimentacaoButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_movimentacaoSelecionada == null || VisualRoot is not Window window) return;

        try
        {
            int rowsAffected = await _estoqueService.DeleteAsync(_movimentacaoSelecionada.CodEst);

            if (rowsAffected > 0)
            {
                await MessageBox.Show(window, "Movimentação excluída com sucesso!", "Sucesso");
                await LoadEstoqueAsync();
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