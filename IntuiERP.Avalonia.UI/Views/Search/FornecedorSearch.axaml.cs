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

public partial class FornecedorSearch : UserControl
{
    private readonly FornecedorService _fornecedorService;
    private ObservableCollection<FornecedorModel> _listaFornecedoresDisplay = new();
    private List<FornecedorModel> _masterListaFornecedores = new();
    private FornecedorModel? _fornecedorSelecionado;

    public FornecedorSearch()
    {
        InitializeComponent();
        
        var factory = new NpgsqlConnectionFactory();
        var connection = factory.CreateConnection();
        _fornecedorService = new FornecedorService(connection);

        FornecedoresListBox.ItemsSource = _listaFornecedoresDisplay;
        
        FornecedoresListBox.SelectionChanged += FornecedoresListBox_SelectionChanged;
        FornecedorSearchBar.TextChanged += FornecedorSearchBar_TextChanged;
        NovoFornecedorButton.Click += NovoFornecedorButton_Clicked;
        EditarFornecedorButton.Click += EditarFornecedorButton_Clicked;
        ExcluirFornecedorButton.Click += ExcluirFornecedorButton_Clicked;
        BtnBack.Click += BtnBack_Clicked;

        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        await LoadFornecedoresAsync();
    }

    private async Task LoadFornecedoresAsync()
    {
        try
        {
            var fornecedores = await _fornecedorService.GetAllAsync();
            _masterListaFornecedores = fornecedores.OrderBy(f => f.NomeFantasia ?? f.RazaoSocial).ToList();
            FilterFornecedores();
        }
        catch (Exception ex)
        {
            if (VisualRoot is Window window)
                await MessageBox.Show(window, $"Não foi possível carregar a lista de fornecedores: {ex.Message}", "Erro");
        }
    }

    private void FilterFornecedores()
    {
        string searchTerm = FornecedorSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        
        var filtered = _masterListaFornecedores
            .Where(f => string.IsNullOrEmpty(searchTerm) || 
                        (f.NomeFantasia?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (f.RazaoSocial?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (f.CNPJ?.ToLowerInvariant().Contains(searchTerm) ?? false));

        _listaFornecedoresDisplay.Clear();
        foreach (var f in filtered)
            _listaFornecedoresDisplay.Add(f);
            
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        bool isSelected = _fornecedorSelecionado != null;
        EditarFornecedorButton.IsEnabled = isSelected;
        ExcluirFornecedorButton.IsEnabled = isSelected;
    }

    private void FornecedoresListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _fornecedorSelecionado = FornecedoresListBox.SelectedItem as FornecedorModel;
        UpdateActionButtonsState();
    }

    private void FornecedorSearchBar_TextChanged(object? sender, TextChangedEventArgs e)
    {
        FilterFornecedores();
    }

    private void NovoFornecedorButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is Window window)
        {
            window.Content = new CadastroFornecedor();
        }
    }

    private void EditarFornecedorButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_fornecedorSelecionado != null && VisualRoot is Window window)
        {
            window.Content = new CadastroFornecedor(_fornecedorSelecionado.CodFornecedor);
        }
    }

    private async void ExcluirFornecedorButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_fornecedorSelecionado == null || VisualRoot is not Window window) return;

        try
        {
            _fornecedorSelecionado.Ativo = false;
            int rowsAffected = await _fornecedorService.UpdateAsync(_fornecedorSelecionado);

            if (rowsAffected > 0)
            {
                await MessageBox.Show(window, "Fornecedor excluído com sucesso!", "Sucesso");
                await LoadFornecedoresAsync();
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