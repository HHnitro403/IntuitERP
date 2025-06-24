using IntuitERP.models;
using IntuitERP.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace IntuitERP.Viwes;

// Helper class for UI-binding of each purchase item
public class CompraItemDisplay : INotifyPropertyChanged
{
    public ItemCompraModel Item { get; }

    public int? Quantidade
    {
        get => Item.quantidade;
        set { if (Item.quantidade != value && value >= 0) { Item.quantidade = value; OnPropertyChanged(); RecalculateTotal(); } }
    }

    public decimal? Desconto
    {
        get => Item.desconto;
        set { if (Item.desconto != value && value >= 0) { Item.desconto = value; OnPropertyChanged(); RecalculateTotal(); } }
    }

    public string ValorUnitarioDisplay => (Item.valor_unitario ?? 0).ToString("N2");
    public string ValorTotalItemDisplay => (Item.valor_total ?? 0).ToString("N2");

    public CompraItemDisplay(ItemCompraModel item)
    {
        Item = item;
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        Item.valor_total = (Item.quantidade ?? 0) * (Item.valor_unitario ?? 0) - (Item.desconto ?? 0);
        if (Item.valor_total < 0) Item.valor_total = 0;
        OnPropertyChanged(nameof(ValorTotalItemDisplay));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public partial class CadastrodeCompra : ContentPage, INotifyPropertyChanged
{
    // Services
    private readonly CompraService _compraService;
    private readonly ItemCompraService _itemCompraService;
    private readonly FornecedorService _fornecedorService;
    private readonly VendedorService _vendedorService;
    private readonly ProdutoService _produtoService;
    private readonly EstoqueService _estoqueService;

    // Data collections
    private ObservableCollection<FornecedorModel> _listaFornecedores;
    private ObservableCollection<VendedorModel> _listaVendedores;
    public ObservableCollection<ProdutoModel> MasterListaProdutos { get; private set; }
    public ObservableCollection<CompraItemDisplay> ItensCompra { get; set; }
    private readonly int? _compraId;

    // UI State Property
    private bool _hasItems;
    public bool HasItems
    {
        get => _hasItems;
        set { if (_hasItems != value) { _hasItems = value; OnPropertyChanged(); } }
    }

    // CORRECTED: StatusCompraItem now uses byte for its Value to match the database
    public class StatusCompraItem { public string DisplayName { get; set; } public byte Value { get; set; } }
    private ObservableCollection<StatusCompraItem> _statusCompraList;

    public CadastrodeCompra(
        CompraService compraService, ItemCompraService itemCompraService, FornecedorService fornecedorService,
        VendedorService vendedorService, ProdutoService produtoService, EstoqueService estoqueService, int? compraId = null)
    {
        InitializeComponent();

        _compraService = compraService;
        _itemCompraService = itemCompraService;
        _fornecedorService = fornecedorService;
        _vendedorService = vendedorService;
        _produtoService = produtoService;
        _estoqueService = estoqueService;

        _listaFornecedores = new ObservableCollection<FornecedorModel>();
        _listaVendedores = new ObservableCollection<VendedorModel>();
        MasterListaProdutos = new ObservableCollection<ProdutoModel>();
        ItensCompra = new ObservableCollection<CompraItemDisplay>();

        FornecedorPicker.ItemsSource = _listaFornecedores;
        FornecedorPicker.ItemDisplayBinding = new Binding("NomeFantasia");
        VendedorPicker.ItemsSource = _listaVendedores;
        VendedorPicker.ItemDisplayBinding = new Binding("NomeVendedor");
        ProdutoParaAdicionarPicker.ItemsSource = MasterListaProdutos;
        ProdutoParaAdicionarPicker.ItemDisplayBinding = new Binding("Descricao");
        ItensCompraCollectionView.ItemsSource = ItensCompra;

        _statusCompraList = new ObservableCollection<StatusCompraItem>
        {
            new StatusCompraItem { DisplayName = "Pendente", Value = 0 },
            new StatusCompraItem { DisplayName = "Em Processamento", Value = 1 },
            new StatusCompraItem { DisplayName = "Concluída", Value = 2 },
            new StatusCompraItem { DisplayName = "Cancelada", Value = 3 }
        };
        StatusCompraPicker.ItemsSource = _statusCompraList;
        StatusCompraPicker.ItemDisplayBinding = new Binding("DisplayName");
        FormaPagamentoPicker.ItemsSource = new List<string> { "Dinheiro", "Cartão de Crédito", "Cartão de Débito", "PIX", "Boleto Bancário" };

        DataCompraPicker.Date = DateTime.Today;
        HoraCompraPicker.Time = DateTime.Now.TimeOfDay;
        _compraId = compraId;

        if (_compraId.HasValue && _compraId > 0)
        {
            this.Title = $"Editar Compra Cód: {_compraId}";
            HeaderLabel.Text = $"Editar Compra #{_compraId}";
        }

        ItensCompra.CollectionChanged += (s, e) => { HasItems = ItensCompra.Any(); RecalculateTotalCompra(); };
        DescontoCompraEntry.TextChanged += (s, e) => RecalculateTotalCompra();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadInitialDataAsync();
        if (_compraId.HasValue && _compraId > 0)
        {
            await LoadCompraAsync(_compraId.Value);
        }
    }

    private async Task LoadCompraAsync(int compraId)
    {
        var compra = await _compraService.GetByIdAsync(compraId);
        if (compra == null) { await DisplayAlert("Erro", "Compra não encontrada.", "OK"); await Navigation.PopAsync(); return; }

        if (compra.data_compra.HasValue) DataCompraPicker.Date = compra.data_compra.Value;
        if (compra.hora_compra.HasValue) HoraCompraPicker.Time = compra.hora_compra.Value;

        FornecedorPicker.SelectedItem = _listaFornecedores.FirstOrDefault(f => f.CodFornecedor == compra.CodFornec);
        VendedorPicker.SelectedItem = _listaVendedores.FirstOrDefault(v => v.CodVendedor == compra.CodVendedor);
        // CORRECTED: Comparison is now byte to byte
        StatusCompraPicker.SelectedItem = _statusCompraList.FirstOrDefault(s => s.Value == compra.status_compra);
        FormaPagamentoPicker.SelectedItem = compra.forma_pagamento;
        DescontoCompraEntry.Text = compra.Desconto?.ToString("F2", CultureInfo.CurrentCulture);
        ObservacoesEditor.Text = compra.OBS;

        var itens = await _itemCompraService.GetByCompraAsync(compraId);
        ItensCompra.Clear();
        foreach (var item in itens ?? Enumerable.Empty<ItemCompraModel>())
        {
            ItensCompra.Add(new CompraItemDisplay(item));
        }

        if (compra.status_compra == 2)
        {        
            SalvarCompraButton.IsEnabled = false;    
            DetalhesdaCompraFrame.IsEnabled = false;
            ItemFrame.IsEnabled = false;
            ItensSectionFrame.IsEnabled = false;
            SalvarCompraButton.IsEnabled = false;
        }
    }

    private async Task LoadInitialDataAsync()
    {
        var fornecedores = await _fornecedorService.GetAllAsync();
        _listaFornecedores.Clear();
        foreach (var f in fornecedores.OrderBy(fn => fn.NomeFantasia ?? fn.RazaoSocial)) _listaFornecedores.Add(f);

        var vendedores = await _vendedorService.GetAllAsync();
        _listaVendedores.Clear();
        foreach (var v in vendedores.OrderBy(vn => vn.NomeVendedor)) _listaVendedores.Add(v);

        var produtos = await _produtoService.GetAllAsync();
        MasterListaProdutos.Clear();
        foreach (var p in produtos.OrderBy(pr => pr.Descricao)) MasterListaProdutos.Add(p);
    }

    private void ConfirmarAdicionarItemButton_Clicked(object sender, EventArgs e)
    {
        var selectedProduto = ProdutoParaAdicionarPicker.SelectedItem as ProdutoModel;
        if (selectedProduto == null) { DisplayAlert("Produto Inválido", "Selecione um produto.", "OK"); return; }
        if (!int.TryParse(QuantidadeParaAdicionarEntry.Text, out int quantidade) || quantidade <= 0) { DisplayAlert("Quantidade Inválida", "A quantidade deve ser > 0.", "OK"); return; }
        decimal.TryParse(DescontoParaAdicionarEntry.Text, out decimal desconto);

        ItensCompra.Add(new CompraItemDisplay(new ItemCompraModel
        {
            CodProduto = selectedProduto.CodProduto,
            Descricao = selectedProduto.Descricao,
            valor_unitario = selectedProduto.PrecoUnitario,
            quantidade = quantidade,
            desconto = desconto
        }));

        ProdutoParaAdicionarPicker.SelectedItem = null;
        QuantidadeParaAdicionarEntry.Text = "1";
        DescontoParaAdicionarEntry.Text = string.Empty;
    }

    private void RemoverItem_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is CompraItemDisplay itemToRemove)
        {
            ItensCompra.Remove(itemToRemove);
        }
    }

    private void RecalculateTotalCompra()
    {
        decimal subTotalItens = ItensCompra.Sum(item => item.Item.valor_total ?? 0);
        decimal.TryParse(DescontoCompraEntry.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal descontoGeral);
        decimal valorTotalFinal = subTotalItens - descontoGeral;
        ValorTotalCompraLabel.Text = valorTotalFinal.ToString("C", CultureInfo.CurrentCulture);
    }

    private async void SalvarCompraButton_Clicked(object sender, EventArgs e)
    {
        if (FornecedorPicker.SelectedItem == null || VendedorPicker.SelectedItem == null || FormaPagamentoPicker.SelectedItem == null || StatusCompraPicker.SelectedItem == null)
        { await DisplayAlert("Campos Obrigatórios", "Fornecedor, Vendedor, Forma de Pagamento e Status são obrigatórios.", "OK"); return; }
        if (!ItensCompra.Any()) { await DisplayAlert("Itens da Compra", "Adicione pelo menos um item à compra.", "OK"); return; }

        var selectedStatus = (StatusCompraItem)StatusCompraPicker.SelectedItem;
        var selectedFornecedor = (FornecedorModel)FornecedorPicker.SelectedItem;
        var selectedVendedor = (VendedorModel)VendedorPicker.SelectedItem;
        decimal.TryParse(DescontoCompraEntry.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal descontoGeralParsed);

        var compraModel = new CompraModel
        {
            CodCompra = _compraId ?? 0,
            data_compra = DataCompraPicker.Date,
            hora_compra = HoraCompraPicker.Time,
            CodFornec = selectedFornecedor.CodFornecedor,
            CodVendedor = selectedVendedor.CodVendedor,
            Desconto = descontoGeralParsed,
            OBS = ObservacoesEditor.Text?.Trim(),
            forma_pagamento = FormaPagamentoPicker.SelectedItem.ToString(),
            status_compra = selectedStatus.Value, // Correctly assigns the byte value
            valor_total = decimal.Parse(ValorTotalCompraLabel.Text, NumberStyles.Currency, CultureInfo.CurrentCulture)
        };

        try
        {
            int compraIdParaItens;
            if (compraModel.CodCompra > 0) // UPDATE
            {
                await _compraService.UpdateAsync(compraModel);
                compraIdParaItens = compraModel.CodCompra;
                await _itemCompraService.DeleteByCompraAsync(compraIdParaItens);
            }
            else // INSERT
            {
                compraIdParaItens = await _compraService.InsertAsync(compraModel);
                if (compraIdParaItens <= 0) { await DisplayAlert("Erro", "Falha ao criar o registro da compra.", "OK"); return; }
            }

            foreach (var displayItem in ItensCompra)
            {
                displayItem.Item.CodCompra = compraIdParaItens;
                await _itemCompraService.InsertAsync(displayItem.Item);

                // CORRECTED: Comparison is now byte to byte
                if (compraModel.status_compra == 2) // 2 = Concluída
                {
                    await _estoqueService.InsertAsync(new EstoqueModel { CodProduto = displayItem.Item.CodProduto.Value, Tipo = 'E', Qtd = displayItem.Item.quantidade, Data = compraModel.data_compra.Value });
                    await _produtoService.AtualizarEstoqueAsync(displayItem.Item.CodProduto.Value, (int)(displayItem.Item.quantidade ?? 0));
                }
            }

            // CORRECTED: Comparison is now byte to byte
            if (compraModel.status_compra == 2) // 2 = Concluída
            {
                await _fornecedorService.UpdateUltimaCompraAsync(selectedFornecedor.CodFornecedor);
            }

            await DisplayAlert("Sucesso", "Compra salva com sucesso!", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro ao Salvar", $"Ocorreu um erro: {ex.Message}", "OK");
        }
    }

    private async void CancelarButton_Clicked(object sender, EventArgs e)
    {
        if (await DisplayAlert("Cancelar", "Tem certeza? Informações não salvas serão perdidas.", "Sim", "Não"))
        {
            await Navigation.PopAsync();
        }
    }
}
