using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using IntuiERP.Avalonia.UI.models;
using IntuiERP.Avalonia.UI.Services;
using IntuiERP.Avalonia.UI.Helpers;
using IntuiERP.Avalonia.UI.Views.Search;
using System;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.Views;

public partial class CadastroVendedor : UserControl
{
    private readonly VendedorService _vendedorService;
    private readonly int _vendedorId;

    public CadastroVendedor(int id = 0)
    {
        InitializeComponent();
        
        var factory = new NpgsqlConnectionFactory();
        _vendedorService = new VendedorService(factory.CreateConnection());
        _vendedorId = id;

        SalvarVendedorButton.Click += SalvarVendedorButton_Clicked;
        CancelarButton.Click += CancelarButton_Clicked;

        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (_vendedorId != 0)
        {
            HeaderLabel.Text = "Editar Vendedor";
            try
            {
                var vendedor = await _vendedorService.GetByIdAsync(_vendedorId);
                if (vendedor != null)
                {
                    NomeVendedorEntry.Text = vendedor.NomeVendedor;
                    TotalVendasEntry.Text = vendedor.totalvendas.ToString();
                    VendasFinalizadasEntry.Text = vendedor.vendasfinalizadas.ToString();
                    VendasCanceladasEntry.Text = vendedor.vendascanceladas.ToString();
                }
            }
            catch (Exception ex)
            {
                await MessageBox.Show(NavigationHelper.GetWindow(this), $"Erro ao carregar vendedor: {ex.Message}", "Erro");
            }
        }
    }

    private void ClearForm()
    {
        NomeVendedorEntry.Text = string.Empty;
        TotalVendasEntry.Text = "0";
        VendasFinalizadasEntry.Text = "0";
        VendasCanceladasEntry.Text = "0";
        NomeVendedorEntry.Focus();
    }

    private async void SalvarVendedorButton_Clicked(object? sender, RoutedEventArgs e)
    {
        var window = NavigationHelper.GetWindow(this);
        if (window == null) return;

        if (string.IsNullOrWhiteSpace(NomeVendedorEntry.Text))
        {
            await MessageBox.Show(window, "Por favor, preencha o Nome do Vendedor.", "Campo Obrigatório");
            NomeVendedorEntry.Focus();
            return;
        }

        var vendedor = new VendedorModel
        {
            NomeVendedor = NomeVendedorEntry.Text.Trim()
        };

        try
        {
            if (_vendedorId != 0) // Update
            {
                vendedor.CodVendedor = _vendedorId;
                var result = await _vendedorService.UpdateAsync(vendedor);

                if (result > 0)
                {
                    await MessageBox.Show(window, "Vendedor atualizado com sucesso!", "Sucesso");
                    NavigationHelper.NavigateTo(new VendedorSearch());
                }
            }
            else // Insert
            {
                int newVendedorId = await _vendedorService.InsertAsync(vendedor);

                if (newVendedorId > 0)
                {
                    await MessageBox.Show(window, "Vendedor cadastrado com sucesso!", "Sucesso");
                    NavigationHelper.NavigateTo(new VendedorSearch());
                }
                else
                {
                    await MessageBox.Show(window, "Não foi possível cadastrar o vendedor.", "Erro");
                }
            }
        }
        catch (Exception ex)
        {
            await MessageBox.Show(window, $"Ocorreu um erro ao salvar o vendedor: {ex.Message}", "Erro");
        }
    }

    private void CancelarButton_Clicked(object? sender, RoutedEventArgs e)
    {
        NavigationHelper.NavigateTo(new VendedorSearch());
    }
}
