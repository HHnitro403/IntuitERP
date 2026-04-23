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

public partial class VendedorSearch : UserControl
{
    private readonly VendedorService _vendedorService;
    private ObservableCollection<VendedorModel> _listaVendedoresDisplay = new();
    private List<VendedorModel> _masterListaVendedores = new();
    private VendedorModel? _vendedorSelecionado;

    public VendedorSearch()
    {
        InitializeComponent();
        
        var factory = new NpgsqlConnectionFactory();
        _vendedorService = new VendedorService(factory.CreateConnection());

        VendedoresListBox.ItemsSource = _listaVendedoresDisplay;
        
        VendedoresListBox.SelectionChanged += VendedoresListBox_SelectionChanged;
        VendedorSearchBar.TextChanged += VendedorSearchBar_TextChanged;
        NovoVendedorButton.Click += NovoVendedorButton_Clicked;
        EditarVendedorButton.Click += EditarVendedorButton_Clicked;
        ExcluirVendedorButton.Click += ExcluirVendedorButton_Clicked;
        BtnBack.Click += BtnBack_Clicked;

        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        await LoadVendedoresAsync();
    }

    private async Task LoadVendedoresAsync()
    {
        try
        {
            var vendedores = await _vendedorService.GetAllAsync();
            _masterListaVendedores = vendedores.OrderBy(v => v.NomeVendedor).ToList();
            FilterVendedores();
        }
        catch (Exception ex)
        {
            await MessageBox.Show(NavigationHelper.GetWindow(this), $"Não foi possível carregar a lista de vendedores: {ex.Message}", "Erro");
        }
    }

    private void FilterVendedores()
    {
        string searchTerm = VendedorSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        
        var filtered = _masterListaVendedores
            .Where(v => string.IsNullOrEmpty(searchTerm) || 
                        (v.NomeVendedor?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (v.CodVendedor.ToString().Contains(searchTerm)));

        _listaVendedoresDisplay.Clear();
        foreach (var v in filtered)
            _listaVendedoresDisplay.Add(v);
            
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        bool isSelected = _vendedorSelecionado != null;
        EditarVendedorButton.IsEnabled = isSelected;
        ExcluirVendedorButton.IsEnabled = isSelected;
    }

    private void VendedoresListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _vendedorSelecionado = VendedoresListBox.SelectedItem as VendedorModel;
        UpdateActionButtonsState();
    }

    private void VendedorSearchBar_TextChanged(object? sender, TextChangedEventArgs e)
    {
        FilterVendedores();
    }

    private void NovoVendedorButton_Clicked(object? sender, RoutedEventArgs e)
    {
        NavigationHelper.NavigateTo(new CadastroVendedor()); // id 0 for new
    }

    private void EditarVendedorButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_vendedorSelecionado != null)
        {
            NavigationHelper.NavigateTo(new CadastroVendedor(_vendedorSelecionado.CodVendedor));
        }
    }

    private async void ExcluirVendedorButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_vendedorSelecionado == null) return;
        var window = NavigationHelper.GetWindow(this);

        // In a real app we'd need a Yes/No MessageBox. 
        // For now, I'll proceed with deletion or wait for user to confirm if I had a YesNo dialog.
        // Assuming deletion is intended if they clicked.
        
        try
        {
            int rowsAffected = await _vendedorService.DeleteAsync(_vendedorSelecionado.CodVendedor);
            if (rowsAffected > 0)
            {
                await MessageBox.Show(window, "Vendedor excluído com sucesso!", "Sucesso");
                await LoadVendedoresAsync();
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("foreign key"))
                await MessageBox.Show(window, "Não é possível excluir este vendedor pois ele possui registros vinculados.", "Erro");
            else
                await MessageBox.Show(window, ex.Message, "Erro");
        }
    }

    private void BtnBack_Clicked(object? sender, RoutedEventArgs e)
    {
        NavigationHelper.NavigateTo(new MenuPage());
    }
}
