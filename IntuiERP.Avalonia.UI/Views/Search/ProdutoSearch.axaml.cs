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

public partial class ProdutoSearch : UserControl
{
    private readonly ProdutoService _produtoService;
    private ObservableCollection<ProdutoModel> _listaProdutosDisplay = new();
    private List<ProdutoModel> _masterListaProdutos = new();
    private ProdutoModel? _produtoSelecionado;

    public ProdutoSearch()
    {
        InitializeComponent();
        
        var factory = new NpgsqlConnectionFactory();
        var connection = factory.CreateConnection();
        _produtoService = new ProdutoService(connection);

        ProdutosListBox.ItemsSource = _listaProdutosDisplay;
        
        ProdutosListBox.SelectionChanged += ProdutosListBox_SelectionChanged;
        ProdutoSearchBar.TextChanged += ProdutoSearchBar_TextChanged;
        NovoProdutoButton.Click += NovoProdutoButton_Clicked;
        EditarProdutoButton.Click += EditarProdutoButton_Clicked;
        ExcluirProdutoButton.Click += ExcluirProdutoButton_Clicked;
        BtnBack.Click += BtnBack_Clicked;

        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        await LoadProdutosAsync();
    }

    private async Task LoadProdutosAsync()
    {
        try
        {
            var produtos = await _produtoService.GetAllAsync();
            _masterListaProdutos = produtos.OrderBy(p => p.Descricao).ToList();
            FilterProdutos();
        }
        catch (Exception ex)
        {
            await MessageBox.Show(NavigationHelper.GetWindow(this), $"Não foi possível carregar a lista de produtos: {ex.Message}", "Erro");
        }
    }

    private void FilterProdutos()
    {
        string searchTerm = ProdutoSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        
        var filtered = _masterListaProdutos
            .Where(p => string.IsNullOrEmpty(searchTerm) || 
                        (p.Descricao?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (p.Categoria?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (p.CodProduto.ToString().Contains(searchTerm)));

        _listaProdutosDisplay.Clear();
        foreach (var p in filtered)
            _listaProdutosDisplay.Add(p);
            
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        bool isSelected = _produtoSelecionado != null;
        EditarProdutoButton.IsEnabled = isSelected;
        ExcluirProdutoButton.IsEnabled = isSelected;
    }

    private void ProdutosListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _produtoSelecionado = ProdutosListBox.SelectedItem as ProdutoModel;
        UpdateActionButtonsState();
    }

    private void ProdutoSearchBar_TextChanged(object? sender, TextChangedEventArgs e)
    {
        FilterProdutos();
    }

    private void NovoProdutoButton_Clicked(object? sender, RoutedEventArgs e)
    {
        NavigationHelper.NavigateTo(new CadastroProduto());
    }

    private void EditarProdutoButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_produtoSelecionado != null)
        {
            NavigationHelper.NavigateTo(new CadastroProduto(_produtoSelecionado.CodProduto));
        }
    }

    private async void ExcluirProdutoButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_produtoSelecionado == null) return;
        var window = NavigationHelper.GetWindow(this);

        try
        {
            _produtoSelecionado.Ativo = false;
            int rowsAffected = await _produtoService.UpdateAsync(_produtoSelecionado);

            if (rowsAffected > 0)
            {
                await MessageBox.Show(window, "Produto excluído com sucesso!", "Sucesso");
                await LoadProdutosAsync();
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
