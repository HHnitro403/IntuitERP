using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using IntuiERP.Avalonia.UI.models;
using IntuiERP.Avalonia.UI.Services;
using IntuiERP.Avalonia.UI.Helpers;
using IntuiERP.Avalonia.UI.Views.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.Views;

public partial class CadastroEstoque : UserControl
{
    private readonly EstoqueService _estoqueService;
    private readonly ProdutoService _produtoService;
    private List<ProdutoModel> _produtos = new();

    public CadastroEstoque()
    {
        InitializeComponent();
        
        var factory = new NpgsqlConnectionFactory();
        var connection = factory.CreateConnection();
        _estoqueService = new EstoqueService(connection);
        _produtoService = new ProdutoService(connection);

        DataMovimentacaoPicker.SelectedDate = DateTime.Today;

        SalvarButton.Click += SalvarButton_Clicked;
        CancelarButton.Click += CancelarButton_Clicked;

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
            var list = await _produtoService.GetAllAsync();
            _produtos = list.OrderBy(p => p.Descricao).ToList();
            ProdutoComboBox.ItemsSource = _produtos;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading products: {ex.Message}");
        }
    }

    private async void SalvarButton_Clicked(object? sender, RoutedEventArgs e)
    {
        var window = NavigationHelper.GetWindow(this);
        if (window == null) return;

        if (ProdutoComboBox.SelectedItem is not ProdutoModel selectedProduto)
        {
            await MessageBox.Show(window, "Por favor, selecione um Produto.", "Campo Obrigatório");
            return;
        }

        if (TipoMovimentacaoComboBox.SelectedItem is not ComboBoxItem selectedItem || selectedItem.Tag is not string tipo)
        {
            await MessageBox.Show(window, "Por favor, selecione o Tipo de Movimentação.", "Campo Obrigatório");
            return;
        }

        if (!decimal.TryParse(QuantidadeEntry.Text, out decimal qtd) || qtd <= 0)
        {
            await MessageBox.Show(window, "Quantidade inválida.", "Erro");
            return;
        }

        var estoque = new EstoqueModel
        {
            CodProduto = selectedProduto.CodProduto,
            Tipo = tipo[0],
            Qtd = (int)qtd,
            Data = DataMovimentacaoPicker.SelectedDate ?? DateTime.Now
        };

        try
        {
            int newId = await _estoqueService.InsertAsync(estoque);

            if (newId > 0)
            {
                await MessageBox.Show(window, "Movimentação registrada com sucesso!", "Sucesso");
                NavigationHelper.NavigateTo(new EstoqueSearch());
            }
            else
            {
                await MessageBox.Show(window, "Não foi possível registrar a movimentação.", "Erro");
            }
        }
        catch (Exception ex)
        {
            await MessageBox.Show(window, $"Ocorreu um erro ao salvar: {ex.Message}", "Erro");
        }
    }

    private void CancelarButton_Clicked(object? sender, RoutedEventArgs e)
    {
        NavigationHelper.NavigateTo(new EstoqueSearch());
    }
}
