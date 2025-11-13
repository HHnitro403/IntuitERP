using IntuitERP.models;
using IntuitERP.Services;
using IntuitERP.Viwes.Modals;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace IntuitERP.Viwes;

// HELPER CLASS FOR UI-BINDING
// This class wraps the ItemVendaModel and notifies the UI when data changes.
public class VendaItemDisplay : INotifyPropertyChanged
{
    public ItemVendaModel Item { get; }

    public int? Quantidade
    {
        get => Item.quantidade;
        set
        {
            if (Item.quantidade != value && value >= 0)
            {
                Item.quantidade = value;
                OnPropertyChanged();
                RecalculateTotal();
            }
        }
    }

    public decimal? Desconto
    {
        get => Item.desconto;
        set
        {
            if (Item.desconto != value && value >= 0)
            {
                Item.desconto = value;
                OnPropertyChanged();
                RecalculateTotal();
            }
        }
    }

    // Read-only properties for display binding
    public string ValorUnitarioDisplay => (Item.valor_unitario ?? 0).ToString("N2");

    public string ValorTotalItemDisplay => (Item.valor_total ?? 0).ToString("N2");

    public VendaItemDisplay(ItemVendaModel item)
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

public partial class CadastrodeVenda : ContentPage, INotifyPropertyChanged
{
    // Services
    private readonly VendaService _vendaService;
    private readonly ItemVendaService _itemVendaService;
    private readonly ClienteService _clienteService;
    private readonly VendedorService _vendedorService;
    private readonly ProdutoService _produtoService;
    private readonly EstoqueService _estoqueService;
    private readonly TransactionService _transactionService;

    // Data collections
    private ObservableCollection<ClienteModel> _listaClientes;

    private ObservableCollection<VendedorModel> _listaVendedores;
    public ObservableCollection<ProdutoModel> MasterListaProdutos { get; private set; }
    public ObservableCollection<VendaItemDisplay> ItensVenda { get; set; }
    private readonly int? _vendaId;
    private int _clienteId;
    private int _vendedorId;
    private ProdutoModel _produtoSelecionadoParaAdicionar;

    // UI State Property
    private bool _hasItems;

    public bool HasItems
    {
        get => _hasItems;
        set
        {
            if (_hasItems != value)
            {
                _hasItems = value;
                OnPropertyChanged(); // Notify the UI to update IsVisible
            }
        }
    }

    public class StatusVendaItem
    { public string DisplayName { get; set; } public byte Value { get; set; } }

    private ObservableCollection<StatusVendaItem> _statusVendaList;

    public CadastrodeVenda(
        VendaService vendaService, ItemVendaService itemVendaService, ClienteService clienteService,
        VendedorService vendedorService, ProdutoService produtoService, EstoqueService estoqueService,
        TransactionService transactionService, int? vendaId = null)
    {
        InitializeComponent();

        _vendaService = vendaService;
        _itemVendaService = itemVendaService;
        _clienteService = clienteService;
        _vendedorService = vendedorService;
        _produtoService = produtoService;
        _estoqueService = estoqueService;
        _transactionService = transactionService;

        _listaClientes = new ObservableCollection<ClienteModel>();
        _listaVendedores = new ObservableCollection<VendedorModel>();
        MasterListaProdutos = new ObservableCollection<ProdutoModel>();
        ItensVenda = new ObservableCollection<VendaItemDisplay>();
        ItensVendaCollectionView.ItemsSource = ItensVenda;

        _statusVendaList = new ObservableCollection<StatusVendaItem>
        {
            new StatusVendaItem { DisplayName = "Or�amento", Value = 0 },
            new StatusVendaItem { DisplayName = "Pendente", Value = 1 },
            new StatusVendaItem { DisplayName = "Faturada", Value = 2 },
            new StatusVendaItem { DisplayName = "Cancelada", Value = 3 }
        };
        StatusVendaPicker.ItemsSource = _statusVendaList;
        StatusVendaPicker.ItemDisplayBinding = new Binding("DisplayName");
        FormaPagamentoPicker.ItemsSource = new List<string> { "Dinheiro", "Cart�o de Cr�dito", "Cart�o de D�bito", "PIX", "Boleto Banc�rio" };

        DataVendaPicker.Date = DateTime.Today;
        HoraVendaPicker.Time = DateTime.Now.TimeOfDay;
        _vendaId = vendaId;

        if (_vendaId.HasValue && _vendaId > 0)
        {
            this.Title = $"Editar Venda C�d: {_vendaId}";
            HeaderLabel.Text = $"Editar Venda #{_vendaId}";
        }

        // Wire up events

        ItensVenda.CollectionChanged += (s, e) =>
        {
            HasItems = ItensVenda.Any();
            RecalculateTotalVenda();
        };
        DescontoVendaEntry.TextChanged += (s, e) => RecalculateTotalVenda();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadInitialDataAsync();
        if (_vendaId.HasValue && _vendaId > 0)
        {
            await LoadVendaAsync(_vendaId.Value);
        }
    }

    private async Task LoadVendaAsync(int vendaId)
    {
        try
        {
            var venda = await _vendaService.GetByIdAsync(vendaId);
            if (venda == null) { await DisplayAlert("Erro", "Venda n�o encontrada.", "OK"); await Navigation.PopAsync(); return; }

            if (venda.data_venda.HasValue) DataVendaPicker.Date = venda.data_venda.Value;
            if (venda.hora_venda.HasValue) HoraVendaPicker.Time = venda.hora_venda.Value;

            var cliente = _listaClientes.FirstOrDefault(c => c.CodCliente == venda.CodCliente);
            ClienteDisplayEntry.Text = cliente?.Nome;
            var vendedor = _listaVendedores.FirstOrDefault(v => v.CodVendedor == venda.CodVendedor);
            VendedorDisplayEntry.Text = vendedor?.NomeVendedor;
            StatusVendaPicker.SelectedItem = _statusVendaList.FirstOrDefault(s => s.Value == venda.status_venda);
            FormaPagamentoPicker.SelectedItem = venda.forma_pagamento;
            DescontoVendaEntry.Text = venda.Desconto?.ToString("F2", CultureInfo.CurrentCulture);
            ObservacoesEditor.Text = venda.OBS;

            var itens = await _itemVendaService.GetByVendaAsync(vendaId);
            ItensVenda.Clear();
            if (itens != null)
            {
                foreach (var item in itens)
                {
                    var displayItem = new VendaItemDisplay(item);
                    ItensVenda.Add(displayItem);
                }
            }

            // Allow editing of all sales - stock reversion is handled automatically
            // Users can re-open or cancel invoiced sales, and stock will be restored properly
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro ao Carregar", $"N�o foi poss�vel carregar os dados da venda: {ex.Message}", "OK");
        }
    }

    private async Task LoadInitialDataAsync()
    {
        try
        {
            var clientes = await _clienteService.GetAllAsync();
            _listaClientes.Clear();
            foreach (var c in clientes.OrderBy(cn => cn.Nome)) _listaClientes.Add(c);

            var vendedores = await _vendedorService.GetAllAsync();
            _listaVendedores.Clear();
            foreach (var v in vendedores.OrderBy(vn => vn.NomeVendedor)) _listaVendedores.Add(v);

            var produtos = await _produtoService.GetAllAsync();
            MasterListaProdutos.Clear();
            foreach (var p in produtos.OrderBy(pr => pr.Descricao)) MasterListaProdutos.Add(p);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro de Dados", $"N�o foi poss�vel carregar listas: {ex.Message}", "OK");
        }
    }

    private void ConfirmarAdicionarItemButton_Clicked(object sender, EventArgs e)
    {
        var selectedProduto = _produtoSelecionadoParaAdicionar;
        if (selectedProduto == null) { DisplayAlert("Produto Inv�lido", "Por favor, selecione um produto.", "OK"); return; }
        if (!int.TryParse(QuantidadeParaAdicionarEntry.Text, out int quantidade) || quantidade <= 0) { DisplayAlert("Quantidade Inv�lida", "A quantidade deve ser um n�mero inteiro maior que zero.", "OK"); return; }
        decimal.TryParse(DescontoParaAdicionarEntry.Text, out decimal desconto);

        var newItemModel = new ItemVendaModel
        {
            CodProduto = selectedProduto.CodProduto,
            Descricao = selectedProduto.Descricao,
            valor_unitario = selectedProduto.PrecoUnitario,
            quantidade = quantidade,
            desconto = desconto
        };

        ItensVenda.Add(new VendaItemDisplay(newItemModel));

        ProdutoDisplayEntry.Text = string.Empty; // Limpa o campo de produto
        QuantidadeParaAdicionarEntry.Text = "1";
        DescontoParaAdicionarEntry.Text = string.Empty;
    }

    // THIS IS THE NEWLY IMPLEMENTED CLICKED EVENT HANDLER
    private void RemoverItem_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button?.CommandParameter is VendaItemDisplay itemToRemove)
        {
            ItensVenda.Remove(itemToRemove);
        }
    }

    private void RecalculateTotalVenda()
    {
        decimal subTotalItens = ItensVenda.Sum(item => item.Item.valor_total ?? 0);
        decimal.TryParse(DescontoVendaEntry.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal descontoGeral);
        decimal valorTotalFinal = subTotalItens - descontoGeral;
        ValorTotalVendaLabel.Text = valorTotalFinal.ToString("C", CultureInfo.CurrentCulture);
    }

    private async void SalvarVendaButton_Clicked(object sender, EventArgs e)
    {
        // Validation logic...
        if (ClienteDisplayEntry.Text == string.Empty || VendedorDisplayEntry.Text == string.Empty || FormaPagamentoPicker.SelectedItem == null || StatusVendaPicker.SelectedItem == null)
        { await DisplayAlert("Campos Obrigatórios", "Cliente, Vendedor, Forma de Pagamento e Status são obrigatórios.", "OK"); return; }
        if (!ItensVenda.Any()) { await DisplayAlert("Itens da Venda", "Adicione pelo menos um item à venda.", "OK"); return; }

        var selectedStatus = (StatusVendaItem)StatusVendaPicker.SelectedItem;

        decimal.TryParse(DescontoVendaEntry.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal descontoGeralParsed);

        var vendaModel = new VendaModel
        {
            CodVenda = _vendaId ?? 0,
            data_venda = DataVendaPicker.Date,
            hora_venda = HoraVendaPicker.Time,
            CodCliente = _clienteId,
            CodVendedor = _vendedorId,
            Desconto = descontoGeralParsed,
            OBS = ObservacoesEditor.Text?.Trim(),
            forma_pagamento = FormaPagamentoPicker.SelectedItem.ToString(),
            status_venda = selectedStatus.Value,
            valor_total = decimal.Parse(ValorTotalVendaLabel.Text, NumberStyles.Currency, CultureInfo.CurrentCulture)
        };

        try
        {
            // Use transaction to ensure all-or-nothing behavior
            await _transactionService.ExecuteInTransactionAsync(async (conn, trans) =>
            {
                byte oldStatus = 0; // Default for new sales
                List<ItemVendaModel> oldItems = new List<ItemVendaModel>();

                int vendaIdParaItens;

                if (vendaModel.CodVenda > 0) // UPDATE
                {
                    // Get old sale state to detect status changes
                    var oldVenda = await _vendaService.GetByIdAsync(vendaModel.CodVenda);
                    if (oldVenda != null)
                    {
                        oldStatus = oldVenda.status_venda;
                        oldItems = (await _itemVendaService.GetByVendaAsync(vendaModel.CodVenda)).ToList();
                    }

                    // CRITICAL FIX #1: Restore stock if changing FROM Faturada (2) TO anything else
                    if (oldStatus == 2 && selectedStatus.Value != 2)
                    {
                        foreach (var oldItem in oldItems)
                        {
                            // Restore stock (reverse the exit)
                            await _estoqueService.InsertAsync(new EstoqueModel
                            {
                                CodProduto = oldItem.CodProduto.Value,
                                Tipo = 'E',  // Entry to reverse the exit
                                Qtd = oldItem.quantidade,
                                Data = DateTime.Now
                            });
                            await _produtoService.AtualizarEstoqueAsync(oldItem.CodProduto.Value, (int)oldItem.quantidade.Value);
                        }
                    }

                    await _vendaService.UpdateAsync(vendaModel);
                    vendaIdParaItens = vendaModel.CodVenda;
                    await _itemVendaService.DeleteByVendaAsync(vendaIdParaItens);
                }
                else // INSERT
                {
                    vendaIdParaItens = await _vendaService.InsertAsync(vendaModel);
                    if (vendaIdParaItens <= 0)
                        throw new Exception("Falha ao criar o registro da venda.");
                }

                // CRITICAL FIX #2: Only deduct stock if NEW status is Faturada AND old was NOT
                bool shouldDeductStock = (selectedStatus.Value == 2) && (oldStatus != 2);

                // Validate stock BEFORE deducting (if needed)
                if (shouldDeductStock)
                {
                    foreach (var displayItem in ItensVenda)
                    {
                        var produtoAtual = await _produtoService.GetByIdAsync(displayItem.Item.CodProduto.Value);
                        if (produtoAtual == null || produtoAtual.SaldoEst < displayItem.Item.quantidade)
                        {
                            throw new Exception($"Estoque insuficiente para '{displayItem.Item.Descricao}'. Disponível: {produtoAtual?.SaldoEst ?? 0}");
                        }
                    }
                }

                // Insert new items
                foreach (var displayItem in ItensVenda)
                {
                    displayItem.Item.CodVenda = vendaIdParaItens;
                    await _itemVendaService.InsertAsync(displayItem.Item);

                    if (shouldDeductStock)
                    {
                        await _estoqueService.InsertAsync(new EstoqueModel
                        {
                            CodProduto = displayItem.Item.CodProduto.Value,
                            Tipo = 'S',  // Exit
                            Qtd = displayItem.Item.quantidade,
                            Data = vendaModel.data_venda.Value
                        });
                        await _produtoService.AtualizarEstoqueAsync(displayItem.Item.CodProduto.Value, (int)(-1 * displayItem.Item.quantidade.Value));
                    }
                }

                // Update statistics only if newly invoiced
                if (shouldDeductStock)
                {
                    await _vendaService.AtualizarClienteUltimaCompraAsync(_clienteId);
                    await _vendedorService.IncrementVendasAsync(_vendedorId);
                    await _vendedorService.IncrementVendasFinalizadasAsync(_vendedorId);
                }

                return vendaIdParaItens;
            });

            await DisplayAlert("Sucesso", "Venda salva com sucesso!", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro ao Salvar", $"Ocorreu um erro: {ex.Message}", "OK");
        }
    }

    private async void CancelarButton_Clicked(object sender, EventArgs e)
    {
        if (await DisplayAlert("Cancelar", "Tem certeza? Informa��es n�o salvas ser�o perdidas.", "Sim", "N�o") && StatusVendaPicker.SelectedIndex != 2 && StatusVendaPicker.SelectedIndex != 3)
        {
            await Navigation.PopAsync();
        }
    }

    private async void SelectClienteButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var selectedCliente = await ModalPicker.Show<ClienteModel>(Navigation, "Selecione o Cliente", _listaClientes);
            if (selectedCliente != null)
            {
                ClienteDisplayEntry.Text = selectedCliente.Nome;
                _clienteId = selectedCliente.CodCliente;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"N�o foi poss�vel carregar clientes: {ex.Message}", "OK");
        }
    }

    private async void SelectVendedorButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var selectedVendedor = await ModalPicker.Show<VendedorModel>(Navigation, "Selecione o Vendedor", _listaVendedores);
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
            var selectedProduto = await ModalPicker.Show<ProdutoModel>(Navigation, "Selecione o Produto", MasterListaProdutos);
            if (selectedProduto != null)
            {
                ProdutoDisplayEntry.Text = selectedProduto.ToString();
                _produtoSelecionadoParaAdicionar = selectedProduto;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"N�o foi poss�vel carregar produtos: {ex.Message}", "OK");
        }
    }
}