using IntuitERP.models;
using IntuitERP.Services;
using IntuitERP.Viwes.Modals;
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
    private readonly TransactionService _transactionService;

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
    public class StatusCompraItem
    { public string DisplayName { get; set; } public byte Value { get; set; } }

    private ObservableCollection<StatusCompraItem> _statusCompraList;
    private int _fornecedorId;
    private int _vendedorId;
    private ProdutoModel _produtoSelecionadoParaAdicionar;

    public CadastrodeCompra(
        CompraService compraService, ItemCompraService itemCompraService, FornecedorService fornecedorService,
        VendedorService vendedorService, ProdutoService produtoService, EstoqueService estoqueService,
        TransactionService transactionService, int? compraId = null)
    {
        InitializeComponent();

        _compraService = compraService;
        _itemCompraService = itemCompraService;
        _fornecedorService = fornecedorService;
        _vendedorService = vendedorService;
        _produtoService = produtoService;
        _estoqueService = estoqueService;
        _transactionService = transactionService;

        _listaFornecedores = new ObservableCollection<FornecedorModel>();
        _listaVendedores = new ObservableCollection<VendedorModel>();
        MasterListaProdutos = new ObservableCollection<ProdutoModel>();
        ItensCompra = new ObservableCollection<CompraItemDisplay>();

        ItensCompraCollectionView.ItemsSource = ItensCompra;

        _statusCompraList = new ObservableCollection<StatusCompraItem>
        {
            new StatusCompraItem { DisplayName = "Em Processamento", Value = 0 },
            new StatusCompraItem { DisplayName = "Pendente", Value = 1 },
            new StatusCompraItem { DisplayName = "Conclu�da", Value = 2 },
            new StatusCompraItem { DisplayName = "Cancelada", Value = 3 }
        };
        StatusCompraPicker.ItemsSource = _statusCompraList;
        StatusCompraPicker.ItemDisplayBinding = new Binding("DisplayName");
        FormaPagamentoPicker.ItemsSource = new List<string> { "Dinheiro", "Cart�o de Cr�dito", "Cart�o de D�bito", "PIX", "Boleto Banc�rio" };

        DataCompraPicker.Date = DateTime.Today;
        HoraCompraPicker.Time = DateTime.Now.TimeOfDay;
        _compraId = compraId;

        if (_compraId.HasValue && _compraId > 0)
        {
            this.Title = $"Editar Compra C�d: {_compraId}";
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
        if (compra == null) { await DisplayAlert("Erro", "Compra n�o encontrada.", "OK"); await Navigation.PopAsync(); return; }

        if (compra.data_compra.HasValue) DataCompraPicker.Date = compra.data_compra.Value;
        if (compra.hora_compra.HasValue) HoraCompraPicker.Time = compra.hora_compra.Value;

        var fornecedor = _listaFornecedores.FirstOrDefault(f => f.CodFornecedor == compra.CodFornec);
        FornecedorDisplayEntry.Text = fornecedor.RazaoSocial ?? fornecedor.NomeFantasia ?? "Nenhum Fornecedor Foi Selecionado";
        var vendodor = _listaVendedores.FirstOrDefault(v => v.CodVendedor == compra.CodVendedor);
        VendedorDisplayEntry.Text = vendodor.NomeVendedor ?? "Nenhum Vendedor Foi Selecionado"; // Corrected: VendedorDisp
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

        // Allow editing of all purchases - stock reversion is handled automatically
        // Users can re-open or cancel completed purchases, and stock will be restored properly
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
        var selectedProduto = _produtoSelecionadoParaAdicionar;
        if (selectedProduto == null) { DisplayAlert("Produto Inv�lido", "Selecione um produto.", "OK"); return; }
        if (!int.TryParse(QuantidadeParaAdicionarEntry.Text, out int quantidade) || quantidade <= 0) { DisplayAlert("Quantidade Inv�lida", "A quantidade deve ser > 0.", "OK"); return; }
        decimal.TryParse(DescontoParaAdicionarEntry.Text, out decimal desconto);

        ItensCompra.Add(new CompraItemDisplay(new ItemCompraModel
        {
            CodProduto = selectedProduto.CodProduto,
            Descricao = selectedProduto.Descricao,
            valor_unitario = selectedProduto.PrecoUnitario,
            quantidade = quantidade,
            desconto = desconto
        }));

        ProdutoDisplayEntry.Text = string.Empty;
        _produtoSelecionadoParaAdicionar = null;
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
        await SalvarFaturarVenda(1);
    }

    private async Task SalvarFaturarVenda(int statusVenda)
    {
        if (FornecedorDisplayEntry.Text == string.Empty || VendedorDisplayEntry.Text == string.Empty || FormaPagamentoPicker.SelectedItem == null || StatusCompraPicker.SelectedItem == null)
        { await DisplayAlert("Campos Obrigat�rios", "Fornecedor, Vendedor, Forma de Pagamento e Status s�o obrigat�rios.", "OK"); return; }
        if (!ItensCompra.Any()) { await DisplayAlert("Itens da Compra", "Adicione pelo menos um item � compra.", "OK"); return; }

        var selectedStatus = (StatusCompraItem)StatusCompraPicker.SelectedItem;

        decimal.TryParse(DescontoCompraEntry.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal descontoGeralParsed);

        var compraModel = new CompraModel
        {
            CodCompra = _compraId ?? 0,
            data_compra = DataCompraPicker.Date,
            hora_compra = HoraCompraPicker.Time,
            CodFornec = _fornecedorId,
            CodVendedor = _vendedorId,
            Desconto = descontoGeralParsed,
            OBS = ObservacoesEditor.Text?.Trim(),
            forma_pagamento = FormaPagamentoPicker.SelectedItem.ToString(),
            status_compra = selectedStatus.Value,
            valor_total = decimal.Parse(ValorTotalCompraLabel.Text, NumberStyles.Currency, CultureInfo.CurrentCulture)
        };

        try
        {
            // Use transaction to ensure all-or-nothing behavior
            await _transactionService.ExecuteInTransactionAsync(async (conn, trans) =>
            {
                byte oldStatus = 0; // Default for new purchases
                List<ItemCompraModel> oldItems = new List<ItemCompraModel>();

                int compraIdParaItens;

                if (compraModel.CodCompra > 0) // UPDATE
                {
                    // Get old purchase state to detect status changes
                    var oldCompra = await _compraService.GetByIdAsync(compraModel.CodCompra);
                    if (oldCompra != null)
                    {
                        oldStatus = oldCompra.status_compra;
                        oldItems = (await _itemCompraService.GetByCompraAsync(compraModel.CodCompra)).ToList();
                    }

                    // CRITICAL FIX #1: Restore stock if changing FROM Concluída (2) TO anything else
                    if (oldStatus == 2 && selectedStatus.Value != 2)
                    {
                        foreach (var oldItem in oldItems)
                        {
                            // Restore stock (reverse the entry)
                            await _estoqueService.InsertAsync(new EstoqueModel
                            {
                                CodProduto = oldItem.CodProduto.Value,
                                Tipo = 'S',  // Exit to reverse the entry
                                Qtd = oldItem.quantidade,
                                Data = DateTime.Now
                            });
                            await _produtoService.AtualizarEstoqueAsync(oldItem.CodProduto.Value, (int)(-1 * oldItem.quantidade.Value));
                        }
                    }

                    await _compraService.UpdateAsync(compraModel);
                    compraIdParaItens = compraModel.CodCompra;
                    await _itemCompraService.DeleteByCompraAsync(compraIdParaItens);
                }
                else // INSERT
                {
                    compraIdParaItens = await _compraService.InsertAsync(compraModel);
                    if (compraIdParaItens <= 0)
                        throw new Exception("Falha ao criar o registro da compra.");
                }

                // CRITICAL FIX #2: Only add stock if NEW status is Concluída AND old was NOT
                bool shouldAddStock = (selectedStatus.Value == 2) && (oldStatus != 2);

                // Insert new items
                foreach (var displayItem in ItensCompra)
                {
                    displayItem.Item.CodCompra = compraIdParaItens;
                    await _itemCompraService.InsertAsync(displayItem.Item);

                    if (shouldAddStock)
                    {
                        await _estoqueService.InsertAsync(new EstoqueModel
                        {
                            CodProduto = displayItem.Item.CodProduto.Value,
                            Tipo = 'E',  // Entry
                            Qtd = displayItem.Item.quantidade,
                            Data = compraModel.data_compra.Value
                        });
                        await _produtoService.AtualizarEstoqueAsync(displayItem.Item.CodProduto.Value, (int)(displayItem.Item.quantidade ?? 0));
                    }
                }

                // Update statistics only if newly completed
                if (shouldAddStock)
                {
                    await _fornecedorService.UpdateUltimaCompraAsync(_fornecedorId);
                }

                return compraIdParaItens;
            });

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
        if (await DisplayAlert("Cancelar", "Tem certeza? Informa��es n�o salvas ser�o perdidas.", "Sim", "N�o"))
        {
            await Navigation.PopAsync();
        }
    }

    private async void SelectFornecedorButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var fornecedorList = await _fornecedorService.GetAllAsync();
            var selectedFornecedor = await ModalPicker.Show<FornecedorModel>(Navigation, "Selecione o Fornecedor", fornecedorList);
            if (selectedFornecedor != null)
            {
                FornecedorDisplayEntry.Text = selectedFornecedor.NomeFantasia ?? selectedFornecedor.RazaoSocial;
                _fornecedorId = selectedFornecedor.CodFornecedor;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"N�o foi poss�vel carregar fornecedores: {ex.Message}", "OK");
        }
    }

    private async void SelectVendedorButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var vendedorList = await _vendedorService.GetAllAsync();
            var selectedVendedor = await ModalPicker.Show<VendedorModel>(Navigation, "Selecione o Vendedor", vendedorList);
            if (selectedVendedor != null)
            {
                VendedorDisplayEntry.Text = selectedVendedor.NomeVendedor;
                _vendedorId = selectedVendedor.CodVendedor;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"N�o foi poss�vel carregar vendedores: {ex.Message}", "OK");
        }
    }

    private async void SelectProdutoButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var produtoslist = await _produtoService.GetAllAsync();
            var selectedProduto = await ModalPicker.Show<ProdutoModel>(Navigation, "Selecione o Produto", produtoslist);
            if (selectedProduto != null)
            {
                ProdutoDisplayEntry.Text = selectedProduto.Descricao;
                _produtoSelecionadoParaAdicionar = selectedProduto; // Store the selected product
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"N�o foi poss�vel carregar produtos: {ex.Message}", "OK");
        }
    }
}