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
using System.Globalization;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.Views;

public partial class CadastroProduto : UserControl
{
    private readonly ProdutoService _produtoService;
    private readonly FornecedorService _fornecedorService;
    private readonly PermissionService _permissionService;
    private readonly int _produtoId;
    private List<FornecedorModel> _fornecedores = new();

    public CadastroProduto(int id = 0)
    {
        InitializeComponent();
        
        var factory = new NpgsqlConnectionFactory();
        var connection = factory.CreateConnection();
        _produtoService = new ProdutoService(connection);
        _fornecedorService = new FornecedorService(connection);
        _permissionService = new PermissionService();
        _produtoId = id;

        DataCadastroPicker.SelectedDate = DateTime.Today;

        SalvarProdutoButton.Click += SalvarProdutoButton_Clicked;
        CancelarButton.Click += CancelarButton_Clicked;

        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (!_permissionService.CanReadProduct())
        {
            await MessageBox.Show(NavigationHelper.GetWindow(this), _permissionService.GetPermissionDeniedMessage("visualizar produtos"), "Acesso Negado");
            NavigationHelper.NavigateTo(new MenuPage());
            return;
        }

        await LoadFornecedoresAsync();

        if (_produtoId != 0)
        {
            HeaderLabel.Text = "Editar Produto";
            try
            {
                var produto = await _produtoService.GetByIdAsync(_produtoId);
                if (produto != null)
                {
                    DescricaoProdutoEntry.Text = produto.Descricao;
                    CategoriaEntry.Text = produto.Categoria;
                    TipoProdutoEntry.Text = produto.Tipo;
                    PrecoUnitarioEntry.Text = produto.PrecoUnitario?.ToString("F2");
                    EstoqueMinimoEntry.Text = produto.EstMinimo.ToString();
                    DataCadastroPicker.SelectedDate = produto.DataCadastro;
                    AtivoSwitch.IsChecked = produto.Ativo;

                    var selectedFornecedor = _fornecedores.FirstOrDefault(f => f.CodFornecedor == produto.FornecedorP_ID);
                    if (selectedFornecedor != null)
                    {
                        FornecedorComboBox.SelectedItem = selectedFornecedor;
                    }

                    if (!_permissionService.CanUpdateProduct())
                    {
                        DisableForm();
                        SalvarProdutoButton.IsVisible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                await MessageBox.Show(NavigationHelper.GetWindow(this), $"Erro ao carregar produto: {ex.Message}", "Erro");
            }
        }
        else if (!_permissionService.CanCreateProduct())
        {
            DisableForm();
            SalvarProdutoButton.IsVisible = false;
        }
    }

    private void DisableForm()
    {
        DescricaoProdutoEntry.IsEnabled = false;
        CategoriaEntry.IsEnabled = false;
        TipoProdutoEntry.IsEnabled = false;
        PrecoUnitarioEntry.IsEnabled = false;
        EstoqueMinimoEntry.IsEnabled = false;
        FornecedorComboBox.IsEnabled = false;
        AtivoSwitch.IsEnabled = false;
    }

    private async Task LoadFornecedoresAsync()
    {
        try
        {
            var list = await _fornecedorService.GetAllAsync();
            _fornecedores = list.OrderBy(f => f.NomeFantasia ?? f.RazaoSocial).ToList();
            FornecedorComboBox.ItemsSource = _fornecedores;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading suppliers: {ex.Message}");
        }
    }

    private async void SalvarProdutoButton_Clicked(object? sender, RoutedEventArgs e)
    {
        var window = NavigationHelper.GetWindow(this);
        if (window == null) return;

        try
        {
            if (_produtoId != 0) _permissionService.RequireProductUpdate();
            else _permissionService.RequireProductCreate();
        }
        catch (UnauthorizedAccessException ex)
        {
            await MessageBox.Show(window, ex.Message, "Acesso Negado");
            return;
        }

        if (string.IsNullOrWhiteSpace(DescricaoProdutoEntry.Text))
        {
            await MessageBox.Show(window, "Por favor, preencha a Descrição do Produto.", "Campo Obrigatório");
            return;
        }

        if (!decimal.TryParse(PrecoUnitarioEntry.Text, out decimal preco))
        {
            await MessageBox.Show(window, "Preço Unitário inválido.", "Erro");
            return;
        }

        if (FornecedorComboBox.SelectedItem is not FornecedorModel selectedFornecedor)
        {
            await MessageBox.Show(window, "Por favor, selecione um Fornecedor.", "Campo Obrigatório");
            return;
        }

        var produto = new ProdutoModel
        {
            Descricao = DescricaoProdutoEntry.Text.Trim(),
            Categoria = CategoriaEntry.Text?.Trim(),
            Tipo = TipoProdutoEntry.Text?.Trim(),
            PrecoUnitario = preco,
            EstMinimo = int.TryParse(EstoqueMinimoEntry.Text, out int estMin) ? estMin : 0,
            FornecedorP_ID = selectedFornecedor.CodFornecedor,
            DataCadastro = DataCadastroPicker.SelectedDate ?? DateTime.Now,
            Ativo = AtivoSwitch.IsChecked == true,
            SaldoEst = 0 
        };

        try
        {
            if (_produtoId != 0)
            {
                produto.CodProduto = _produtoId;
                var result = await _produtoService.UpdateAsync(produto);
                if (result > 0)
                {
                    await MessageBox.Show(window, "Produto atualizado com sucesso!", "Sucesso");
                    NavigationHelper.NavigateTo(new ProdutoSearch());
                }
            }
            else
            {
                int newId = await _produtoService.InsertAsync(produto);
                if (newId > 0)
                {
                    await MessageBox.Show(window, "Produto cadastrado com sucesso!", "Sucesso");
                    NavigationHelper.NavigateTo(new ProdutoSearch());
                }
            }
        }
        catch (Exception ex)
        {
            await MessageBox.Show(window, $"Ocorreu um erro ao salvar o produto: {ex.Message}", "Erro");
        }
    }

    private void CancelarButton_Clicked(object? sender, RoutedEventArgs e)
    {
        NavigationHelper.NavigateTo(new ProdutoSearch());
    }
}
