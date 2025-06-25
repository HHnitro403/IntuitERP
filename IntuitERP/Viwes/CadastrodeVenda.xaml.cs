using IntuitERP.models;
using IntuitERP.Services;
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

    // Data collections
    private ObservableCollection<ClienteModel> _listaClientes;
    private ObservableCollection<VendedorModel> _listaVendedores;
    public ObservableCollection<ProdutoModel> MasterListaProdutos { get; private set; }
    public ObservableCollection<VendaItemDisplay> ItensVenda { get; set; }
    private readonly int? _vendaId;

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

    public class StatusVendaItem { public string DisplayName { get; set; } public byte Value { get; set; } }
    private ObservableCollection<StatusVendaItem> _statusVendaList;

    public CadastrodeVenda(
        VendaService vendaService, ItemVendaService itemVendaService, ClienteService clienteService,
        VendedorService vendedorService, ProdutoService produtoService, EstoqueService estoqueService, int? vendaId = null)
    {
        InitializeComponent();

        _vendaService = vendaService;
        _itemVendaService = itemVendaService;
        _clienteService = clienteService;
        _vendedorService = vendedorService;
        _produtoService = produtoService;
        _estoqueService = estoqueService;

        _listaClientes = new ObservableCollection<ClienteModel>();
        _listaVendedores = new ObservableCollection<VendedorModel>();
        MasterListaProdutos = new ObservableCollection<ProdutoModel>();
        ItensVenda = new ObservableCollection<VendaItemDisplay>();

        ClientePicker.ItemsSource = _listaClientes;
        ClientePicker.ItemDisplayBinding = new Binding("Nome");
        VendedorPicker.ItemsSource = _listaVendedores;
        VendedorPicker.ItemDisplayBinding = new Binding("NomeVendedor");
        ProdutoParaAdicionarPicker.ItemsSource = MasterListaProdutos;
        ProdutoParaAdicionarPicker.ItemDisplayBinding = new Binding("Descricao");
        ItensVendaCollectionView.ItemsSource = ItensVenda;

        _statusVendaList = new ObservableCollection<StatusVendaItem>
        {
            new StatusVendaItem { DisplayName = "Orçamento", Value = 0 },
            new StatusVendaItem { DisplayName = "Pendente", Value = 1 },
            new StatusVendaItem { DisplayName = "Faturada", Value = 2 },
            new StatusVendaItem { DisplayName = "Cancelada", Value = 3 }
        };
        StatusVendaPicker.ItemsSource = _statusVendaList;
        StatusVendaPicker.ItemDisplayBinding = new Binding("DisplayName");
        FormaPagamentoPicker.ItemsSource = new List<string> { "Dinheiro", "Cartão de Crédito", "Cartão de Débito", "PIX", "Boleto Bancário" };

        DataVendaPicker.Date = DateTime.Today;
        HoraVendaPicker.Time = DateTime.Now.TimeOfDay;
        _vendaId = vendaId;

        if (_vendaId.HasValue && _vendaId > 0)
        {
            this.Title = $"Editar Venda Cód: {_vendaId}";
            HeaderLabel.Text = $"Editar Venda #{_vendaId}";
        }

        // Wire up events
        
        ItensVenda.CollectionChanged += (s, e) => {
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
            if (venda == null) { await DisplayAlert("Erro", "Venda não encontrada.", "OK"); await Navigation.PopAsync(); return; }

            if (venda.data_venda.HasValue) DataVendaPicker.Date = venda.data_venda.Value;
            if (venda.hora_venda.HasValue) HoraVendaPicker.Time = venda.hora_venda.Value;

            ClientePicker.SelectedItem = _listaClientes.FirstOrDefault(c => c.CodCliente == venda.CodCliente);
            VendedorPicker.SelectedItem = _listaVendedores.FirstOrDefault(v => v.CodVendedor == venda.CodVendedor);
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

            if (venda.status_venda == 2 || venda.status_venda == 3)
            {
                DetailsFrame.IsEnabled = false;
                AddItemFrame.IsEnabled = false;
                ItensSectionFrame.IsEnabled = false;
                SalvarVendaButton.IsEnabled = false;
            }
           
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro ao Carregar", $"Não foi possível carregar os dados da venda: {ex.Message}", "OK");
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
            await DisplayAlert("Erro de Dados", $"Não foi possível carregar listas: {ex.Message}", "OK");
        }
    }

    private void ConfirmarAdicionarItemButton_Clicked(object sender, EventArgs e)
    {
        var selectedProduto = ProdutoParaAdicionarPicker.SelectedItem as ProdutoModel;
        if (selectedProduto == null) { DisplayAlert("Produto Inválido", "Por favor, selecione um produto.", "OK"); return; }
        if (!int.TryParse(QuantidadeParaAdicionarEntry.Text, out int quantidade) || quantidade <= 0) { DisplayAlert("Quantidade Inválida", "A quantidade deve ser um número inteiro maior que zero.", "OK"); return; }
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

        ProdutoParaAdicionarPicker.SelectedItem = null;
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
        if (ClientePicker.SelectedItem == null || VendedorPicker.SelectedItem == null || FormaPagamentoPicker.SelectedItem == null || StatusVendaPicker.SelectedItem == null)
        { await DisplayAlert("Campos Obrigatórios", "Cliente, Vendedor, Forma de Pagamento e Status são obrigatórios.", "OK"); return; }
        if (!ItensVenda.Any()) { await DisplayAlert("Itens da Venda", "Adicione pelo menos um item à venda.", "OK"); return; }

        var selectedStatus = (StatusVendaItem)StatusVendaPicker.SelectedItem;
        foreach (var displayItem in ItensVenda)
        {
            if (selectedStatus.Value == 2)
            {
                var produtoAtual = await _produtoService.GetByIdAsync(displayItem.Item.CodProduto.Value);
                if (produtoAtual == null || produtoAtual.SaldoEst < displayItem.Item.quantidade)
                {
                    await DisplayAlert("Estoque Insuficiente", $"Estoque para '{displayItem.Item.Descricao}' insuficiente.", "OK"); return;
                }
            }
        }

        var selectedCliente = (ClienteModel)ClientePicker.SelectedItem;
        var selectedVendedor = (VendedorModel)VendedorPicker.SelectedItem;
        decimal.TryParse(DescontoVendaEntry.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal descontoGeralParsed);

        var vendaModel = new VendaModel
        {
            CodVenda = _vendaId ?? 0,
            data_venda = DataVendaPicker.Date,
            hora_venda = HoraVendaPicker.Time,
            CodCliente = selectedCliente.CodCliente,
            CodVendedor = selectedVendedor.CodVendedor,
            Desconto = descontoGeralParsed,
            OBS = ObservacoesEditor.Text?.Trim(),
            forma_pagamento = FormaPagamentoPicker.SelectedItem.ToString(),
            status_venda = selectedStatus.Value,
            valor_total = decimal.Parse(ValorTotalVendaLabel.Text, NumberStyles.Currency, CultureInfo.CurrentCulture)
        };

        try
        {
            int vendaIdParaItens;
            if (vendaModel.CodVenda > 0) // UPDATE
            {
                await _vendaService.UpdateAsync(vendaModel);
                vendaIdParaItens = vendaModel.CodVenda;
                await _itemVendaService.DeleteByVendaAsync(vendaIdParaItens);
            }
            else // INSERT
            {
                vendaIdParaItens = await _vendaService.InsertAsync(vendaModel);
                if (vendaIdParaItens <= 0) { await DisplayAlert("Erro", "Falha ao criar o registro da venda.", "OK"); return; }
            }

            foreach (var displayItem in ItensVenda)
            {
                displayItem.Item.CodVenda = vendaIdParaItens;
                await _itemVendaService.InsertAsync(displayItem.Item);

                if (selectedStatus.Value == 2) // Faturada
                {
                    await _estoqueService.InsertAsync(new EstoqueModel { CodProduto = displayItem.Item.CodProduto.Value, Tipo = 'S', Qtd = displayItem.Item.quantidade, Data = vendaModel.data_venda.Value });
                    await _produtoService.AtualizarEstoqueAsync(displayItem.Item.CodProduto.Value, (int)(-1 * displayItem.Item.quantidade.Value));
                }
            }

            if (selectedStatus.Value == 2)
            {
                await _vendaService.AtualizarClienteUltimaCompraAsync(selectedCliente.CodCliente);
                await _vendedorService.IncrementVendasAsync(selectedVendedor.CodVendedor);
                await _vendedorService.IncrementVendasFinalizadasAsync(selectedVendedor.CodVendedor);
            }

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
        if (await DisplayAlert("Cancelar", "Tem certeza? Informações não salvas serão perdidas.", "Sim", "Não") && StatusVendaPicker.SelectedIndex != 2 && StatusVendaPicker.SelectedIndex != 3)
        {
            await Navigation.PopAsync();
        }
    }
}
