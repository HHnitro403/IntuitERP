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

public partial class UsuarioSearch : UserControl
{
    private readonly UsuarioService _usuarioService;
    private ObservableCollection<UsuarioModel> _listaUsuariosDisplay = new();
    private List<UsuarioModel> _masterListaUsuarios = new();
    private UsuarioModel? _usuarioSelecionado;

    public UsuarioSearch()
    {
        InitializeComponent();
        
        var factory = new NpgsqlConnectionFactory();
        _usuarioService = new UsuarioService(factory);

        UsuariosListBox.ItemsSource = _listaUsuariosDisplay;
        
        UsuariosListBox.SelectionChanged += UsuariosListBox_SelectionChanged;
        UsuarioSearchBar.TextChanged += UsuarioSearchBar_TextChanged;
        NovoUsuarioButton.Click += NovoUsuarioButton_Clicked;
        EditarUsuarioButton.Click += EditarUsuarioButton_Clicked;
        ExcluirUsuarioButton.Click += ExcluirUsuarioButton_Clicked;
        BtnBack.Click += BtnBack_Clicked;

        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        await LoadUsuariosAsync();
    }

    private async Task LoadUsuariosAsync()
    {
        try
        {
            var usuarios = await _usuarioService.GetAllAsync();
            _masterListaUsuarios = usuarios.OrderBy(u => u.Usuario).ToList();
            FilterUsuarios();
        }
        catch (Exception ex)
        {
            if (VisualRoot is Window window)
                await MessageBox.Show(window, $"Não foi possível carregar a lista de usuários: {ex.Message}", "Erro");
        }
    }

    private void FilterUsuarios()
    {
        string searchTerm = UsuarioSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        
        var filtered = _masterListaUsuarios
            .Where(u => string.IsNullOrEmpty(searchTerm) || 
                        (u.Usuario?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (u.CodUsuarios.ToString().Contains(searchTerm)));

        _listaUsuariosDisplay.Clear();
        foreach (var u in filtered)
            _listaUsuariosDisplay.Add(u);
            
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        bool isSelected = _usuarioSelecionado != null;
        EditarUsuarioButton.IsEnabled = isSelected;
        ExcluirUsuarioButton.IsEnabled = isSelected;
    }

    private void UsuariosListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _usuarioSelecionado = UsuariosListBox.SelectedItem as UsuarioModel;
        UpdateActionButtonsState();
    }

    private void UsuarioSearchBar_TextChanged(object? sender, TextChangedEventArgs e)
    {
        FilterUsuarios();
    }

    private void NovoUsuarioButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is Window window)
        {
            window.Content = new CadastroUsuario();
        }
    }

    private void EditarUsuarioButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_usuarioSelecionado != null && VisualRoot is Window window)
        {
            window.Content = new CadastroUsuario(_usuarioSelecionado.CodUsuarios);
        }
    }

    private async void ExcluirUsuarioButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (_usuarioSelecionado == null || VisualRoot is not Window window) return;
        
        try
        {
            int rowsAffected = await _usuarioService.DeleteAsync(_usuarioSelecionado.CodUsuarios);
            if (rowsAffected > 0)
            {
                await MessageBox.Show(window, "Usuário excluído com sucesso!", "Sucesso");
                await LoadUsuariosAsync();
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