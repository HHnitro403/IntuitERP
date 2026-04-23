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

public partial class ClienteSearch : UserControl
{
    private readonly ClienteService _clienteService;
    private ObservableCollection<ClienteModel> _listaClientesDisplay = new();
    private List<ClienteModel> _masterListaClientes = new();
    private ClienteModel? _clienteSelecionado;

    public ClienteSearch()
    {
        InitializeComponent();
        
        var factory = new NpgsqlConnectionFactory();
        var connection = factory.CreateConnection();
        _clienteService = new ClienteService(connection);

        ClientesListBox.ItemsSource = _listaClientesDisplay;
        
        ClientesListBox.SelectionChanged += ClientesListBox_SelectionChanged;
        ClienteSearchBar.TextChanged += ClienteSearchBar_TextChanged;
        NovoClienteButton.Click += NovoClienteButton_Clicked;
        EditarClienteButton.Click += EditarClienteButton_Clicked;
        ExcluirClienteButton.Click += ExcluirClienteButton_Clicked;
        BtnBack.Click += BtnBack_Clicked;

        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        await LoadClientesAsync();
    }

    private async Task LoadClientesAsync()
    {
        try
        {
            var clientes = await _clienteService.GetAllAsync();
            _masterListaClientes = clientes.OrderBy(c => c.Nome).ToList();
            FilterClientes();
        }
        catch (Exception ex)
        {
            if (VisualRoot is Window window)
                await MessageBox.Show(window, $"Não foi possível carregar a lista de clientes: {ex.Message}", "Erro");
        }
    }

    private void FilterClientes()
    {
        string searchTerm = ClienteSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        
        var filtered = _masterListaClientes
            .Where(c => string.IsNullOrEmpty(searchTerm) || 
                        (c.Nome?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (c.Email?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (c.CPF?.ToLowerInvariant().Contains(searchTerm) ?? false));

        _listaClientesDisplay.Clear();
        foreach (var c in filtered)
            _listaClientesDisplay.Add(c);
            
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        bool isSelected = _clienteSelecionado != null;
        EditarClienteButton.IsEnabled = isSelected;
        ExcluirClienteButton.IsEnabled = isSelected;
    }

    private void ClientesListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _clienteSelecionado = ClientesListBox.SelectedItem as ClienteModel;
        UpdateActionButtonsState();
    }

    private void ClienteSearchBar_TextChanged(object? sender, TextChangedEventArgs e)
    {
        FilterClientes();
    }

    private void NovoClienteButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is Window window)
        {
            window.Content = new CadastroCliente();
        }
    }

    private void EditarClienteButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_clienteSelecionado != null && VisualRoot is Window window)
        {
            window.Content = new CadastroCliente(_clienteSelecionado.CodCliente);
        }
    }

    private async void ExcluirClienteButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_clienteSelecionado == null || VisualRoot is not Window window) return;

        try
        {
            _clienteSelecionado.Ativo = false;
            int rowsAffected = await _clienteService.UpdateAsync(_clienteSelecionado);

            if (rowsAffected > 0)
            {
                await MessageBox.Show(window, "Cliente excluído com sucesso!", "Sucesso");
                await LoadClientesAsync();
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