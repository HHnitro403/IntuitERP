using IntuitERP.models;
using IntuitERP.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq; // Required for LINQ methods like FirstOrDefault

namespace IntuitERP.Viwes;

public partial class CadastrodeVenda : ContentPage
{

    // Services
    private readonly VendaService _vendaService;
    private readonly ItemVendaService _itemVendaService;
    private readonly ClienteService _clienteService;
    private readonly VendedorService _vendedorService;
    private readonly ProdutoService _produtoService;
    private readonly EstoqueService _estoqueService;

    // Data sources for pickers
    private ObservableCollection<ClienteModel> _listaClientes;
    private ObservableCollection<VendedorModel> _listaVendedores;
    private List<ProdutoModel> _masterListaProdutos;
    public ObservableCollection<ItemVendaModel> ItensVenda { get; set; }
    private readonly int _VendaId;

    public class StatusVendaItem
    {
        public string DisplayName { get; set; }
        public byte Value { get; set; } // Assumes status_venda is byte
    }
    private ObservableCollection<StatusVendaItem> _statusVendaList;

    public CadastrodeVenda(
        VendaService vendaService, ItemVendaService itemVendaService, ClienteService clienteService,
        VendedorService vendedorService, ProdutoService produtoService, EstoqueService estoqueService, int? vendaId = 0)
    {
        InitializeComponent();

        // Assign services
        _vendaService = vendaService;
        _itemVendaService = itemVendaService;
        _clienteService = clienteService;
        _vendedorService = vendedorService;
        _produtoService = produtoService;
        _estoqueService = estoqueService;

        // Initialize collections
        _listaClientes = new ObservableCollection<ClienteModel>();
        _listaVendedores = new ObservableCollection<VendedorModel>();
        _masterListaProdutos = new List<ProdutoModel>();
        ItensVenda = new ObservableCollection<ItemVendaModel>();

        // Set ItemsSources for Pickers and the CollectionView
        ClientePicker.ItemsSource = _listaClientes;
        ClientePicker.ItemDisplayBinding = new Binding("Nome");

        VendedorPicker.ItemsSource = _listaVendedores;
        VendedorPicker.ItemDisplayBinding = new Binding("NomeVendedor");

        ItensVendaCollectionView.ItemsSource = ItensVenda;

        // Populate static Status Picker
        _statusVendaList = new ObservableCollection<StatusVendaItem>
            {
                new StatusVendaItem { DisplayName = "Orçamento", Value = 0 },
                new StatusVendaItem { DisplayName = "Pendente", Value = 1 },
                new StatusVendaItem { DisplayName = "Faturada", Value = 2 },
                new StatusVendaItem { DisplayName = "Cancelada", Value = 3 }
            };
        StatusVendaPicker.ItemsSource = _statusVendaList;
        StatusVendaPicker.ItemDisplayBinding = new Binding("DisplayName");
        StatusVendaPicker.SelectedIndex = 0; // Default

        // Set default date/time
        DataVendaPicker.Date = DateTime.Today;
        HoraVendaPicker.Time = DateTime.Now.TimeOfDay;

        _VendaId = vendaId ?? 0;

        // Change Title if editing
        if (_VendaId > 0)
        {
            this.Title = $"Editar Venda {_VendaId}";
           // NovaVendaTitleLabel.Text = $"Editar Venda #{_VendaId}"; // Assuming you have a x:Name="NovaVendaTitleLabel" on your header label
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // It's crucial to load the general data first (clients, products, etc.)
        await LoadInitialDataAsync();

        // If we are editing an existing sale, load its specific data now
        if (_VendaId > 0)
        {
            await LoadVendaAsync(_VendaId);
        }
    }

    /// <summary>
    /// Loads an existing sale's data into the form.
    /// </summary>
    private async Task LoadVendaAsync(int vendaId)
    {
        try
        {
            // 1. Fetch the main sale record
            var venda = await _vendaService.GetByIdAsync(vendaId);
            if (venda == null)
            {
                await DisplayAlert("Erro", "Venda não encontrada.", "OK");
                await Navigation.PopAsync(); // Or close the page
                return;
            }

            // 2. Fetch the sale items
            var vendaselecionada = await _itemVendaService.GetByIdAsync(vendaId);

            // 3. Populate the UI with the fetched data

            // --- Header fields ---
            if (venda.data_venda.HasValue)
                DataVendaPicker.Date = venda.data_venda.Value;

            if (venda.hora_venda.HasValue)
                HoraVendaPicker.Time = venda.hora_venda.Value;

            // Find and set the selected client, vendor, and status in the pickers
            // This relies on _listaClientes and _listaVendedores already being populated
            ClientePicker.SelectedItem = _listaClientes.FirstOrDefault(c => c.CodCliente == venda.CodCliente);
            VendedorPicker.SelectedItem = _listaVendedores.FirstOrDefault(v => v.CodVendedor == venda.CodVendedor);
            StatusVendaPicker.SelectedItem = _statusVendaList.FirstOrDefault(s => s.Value == venda.status_venda);

            // Set simple fields
            DescontoVendaEntry.Text = venda.Desconto?.ToString("F2", CultureInfo.CurrentCulture);
            ObservacoesEditor.Text = venda.OBS;
            FormaPagamentoPicker.SelectedItem = venda.forma_pagamento;

            var itens = await _itemVendaService.GetByVendaAsync(vendaId);

            // --- Sale Items ---
            ItensVenda.Clear();
            if (itens != null)
            {
                foreach (var item in itens)
                {
                    ItensVenda.Add(item);
                }
            }

            // 4. Recalculate total to update the label
            RecalculateTotalVenda();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading sale data: {ex.ToString()}");
            await DisplayAlert("Erro de Carregamento", $"Não foi possível carregar os dados da venda. Detalhe: {ex.Message}", "OK");
        }
    }


    private async Task LoadInitialDataAsync()
    {
        try
        {
            // Load data sequentially to avoid DataReader conflict on a single connection
            var clientes = await _clienteService.GetAllAsync();
            _listaClientes.Clear();
            if (clientes != null)
            {
                foreach (var c in clientes.OrderBy(cn => cn.Nome))
                {
                    _listaClientes.Add(c);
                }
            }

            var vendedores = await _vendedorService.GetAllAsync();
            _listaVendedores.Clear();
            if (vendedores != null)
            {
                foreach (var v in vendedores.OrderBy(vn => vn.NomeVendedor))
                {
                    _listaVendedores.Add(v);
                }
            }

            var produtos = await _produtoService.GetAllAsync();
            _masterListaProdutos.Clear();
            if (produtos != null)
            {
                _masterListaProdutos.AddRange(produtos.OrderBy(p => p.Descricao));
            }

            RecalculateTotalVenda();
        }
        catch (Exception ex)
        {
            // Log the full exception details for better debugging
            Console.WriteLine($"Error loading initial data for Sale: {ex.ToString()}");
            await DisplayAlert("Erro de Carregamento", $"Não foi possível carregar os dados. Verifique a conexão ou contate o suporte. Detalhe: {ex.Message}", "OK");
        }
    }

    private void AdicionarItemVendaButton_Clicked(object sender, EventArgs e)
    {
        if (!_masterListaProdutos.Any())
        {
            DisplayAlert("Atenção", "Não há produtos cadastrados para adicionar.", "OK");
            return;
        }
        var newItem = new ItemVendaModel { quantidade = 1, desconto = 0 };



        ItensVenda.Add(newItem);

    }



    private void ItemProductPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        if (picker == null) return;

        // Find the ItemVendaModel associated with this picker
        // This assumes the BindingContext of the Picker (or its parent Grid/Frame in the DataTemplate) is the ItemVendaModel
        var itemVenda = picker.BindingContext as ItemVendaModel;
        var selectedProduto = picker.SelectedItem as ProdutoModel;

        if (itemVenda != null && selectedProduto != null)
        {
            itemVenda.CodProduto = selectedProduto.CodProduto;
            itemVenda.Descricao = selectedProduto.Descricao;
            itemVenda.valor_unitario = selectedProduto.PrecoUnitario ?? 0;
            // If valor_unitario is directly bound in XAML's Entry, this manual set might be redundant
            // but ensures the model is updated.
        }
        else if (itemVenda != null && picker.SelectedIndex == -1) // No product selected
        {
            itemVenda.CodProduto = null;
            itemVenda.Descricao = string.Empty;
            itemVenda.valor_unitario = 0;
        }
        RecalculateTotalVenda();
    }

    private void ItemQuantity_TextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = sender as Entry;
        if (entry == null) return;
        var itemVenda = entry.BindingContext as ItemVendaModel;

        if (itemVenda != null)
        {
            if (decimal.TryParse(e.NewTextValue, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal newQty) && newQty >= 0)
            {
                itemVenda.quantidade = Convert.ToInt32(newQty);
            }
            else if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                itemVenda.quantidade = 0; // Or handle as invalid input
            }
        }
        RecalculateTotalVenda();
    }

    private void ItemDiscount_TextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = sender as Entry;
        if (entry == null) return;
        var itemVenda = entry.BindingContext as ItemVendaModel;

        if (itemVenda != null)
        {
            if (decimal.TryParse(e.NewTextValue, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal newDiscount) && newDiscount >= 0)
            {
                itemVenda.desconto = newDiscount;
            }
            else if (decimal.TryParse(e.NewTextValue, NumberStyles.Currency, CultureInfo.InvariantCulture, out newDiscount) && newDiscount >= 0) // Fallback for '.'
            {
                itemVenda.desconto = newDiscount;
            }
            else if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                itemVenda.desconto = 0;
            }
        }
        RecalculateTotalVenda();
    }

    private void RemoveItemButton_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button == null) return;
        var itemVenda = button.BindingContext as ItemVendaModel;
        if (itemVenda != null)
        {
            ItensVenda.Remove(itemVenda);
        }
        // RecalculateTotalVenda() is called by the CollectionChanged event.
    }

    private void RecalculateTotalVenda()
    {
        decimal subTotalItens = 0;
        foreach (var item in ItensVenda)
        {
            // Ensure item.valor_unitario is up-to-date if product was re-selected
            if (item.CodProduto.HasValue && _masterListaProdutos.Any(p => p.CodProduto == item.CodProduto.Value))
            {
                var prod = _masterListaProdutos.First(p => p.CodProduto == item.CodProduto.Value);
                item.valor_unitario = prod.PrecoUnitario ?? 0; // Ensure price is from master list
                if (string.IsNullOrWhiteSpace(item.Descricao)) item.Descricao = prod.Descricao;
            }


            item.valor_total = (item.quantidade ?? 0) * (item.valor_unitario ?? 0) - (item.desconto ?? 0);
            if (item.valor_total < 0) item.valor_total = 0;
            subTotalItens += item.valor_total.Value;
        }

        decimal descontoGeral = 0;
        if (!string.IsNullOrWhiteSpace(DescontoVendaEntry.Text))
        {
            if (decimal.TryParse(DescontoVendaEntry.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal parsedDesconto) ||
                decimal.TryParse(DescontoVendaEntry.Text, NumberStyles.Currency, CultureInfo.InvariantCulture, out parsedDesconto))
            {
                if (parsedDesconto >= 0) descontoGeral = parsedDesconto;
            }
        }

        decimal valorTotalFinal = subTotalItens - descontoGeral;
        if (valorTotalFinal < 0) valorTotalFinal = 0;

        ValorTotalVendaLabel.Text = valorTotalFinal.ToString("C", CultureInfo.CurrentCulture);
    }

    private async void SalvarVendaButton_Clicked(object sender, EventArgs e)
    {
        // --- Validation ---
        if (ClientePicker.SelectedItem == null || VendedorPicker.SelectedItem == null ||
            FormaPagamentoPicker.SelectedItem == null || StatusVendaPicker.SelectedItem == null)
        {
            await DisplayAlert("Campos Obrigatórios", "Cliente, Vendedor, Forma de Pagamento e Status são obrigatórios.", "OK"); return;
        }
        if (!ItensVenda.Any())
        {
            await DisplayAlert("Itens da Venda", "Adicione pelo menos um item à venda.", "OK"); return;
        }

        var selectedStatus = (StatusVendaItem)StatusVendaPicker.SelectedItem;
        foreach (var item in ItensVenda)
        {
            if (item.CodProduto == null || item.CodProduto <= 0) { await DisplayAlert("Item Inválido", "Todos os itens devem ter um produto selecionado.", "OK"); return; }
            if (item.quantidade == null || item.quantidade <= 0) { await DisplayAlert("Quantidade Inválida", $"A quantidade para o produto '{item.Descricao}' deve ser maior que zero.", "OK"); return; }

            if (selectedStatus.DisplayName == "Faturada") // Or compare by selectedStatus.Value
            {
                var produtoAtual = await _produtoService.GetByIdAsync(item.CodProduto.Value);
                if (produtoAtual == null || produtoAtual.SaldoEst < item.quantidade.Value)
                {
                    await DisplayAlert("Estoque Insuficiente", $"Estoque insuficiente para '{item.Descricao}'. Saldo: {produtoAtual?.SaldoEst ?? 0}", "OK");
                    return;
                }
            }
        }

        decimal descontoGeralParsed = 0;
        if (!string.IsNullOrWhiteSpace(DescontoVendaEntry.Text))
        {
            if (!decimal.TryParse(DescontoVendaEntry.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out descontoGeralParsed) &&
                !decimal.TryParse(DescontoVendaEntry.Text, NumberStyles.Currency, CultureInfo.InvariantCulture, out descontoGeralParsed))
            {
                await DisplayAlert("Desconto Inválido", "O valor do Desconto Geral é inválido.", "OK"); return;
            }
        }
        if (descontoGeralParsed < 0) descontoGeralParsed = 0;


        var selectedCliente = (ClienteModel)ClientePicker.SelectedItem;
        var selectedVendedor = (VendedorModel)VendedorPicker.SelectedItem;

        var novaVenda = new VendaModel
        {
            data_venda = DataVendaPicker.Date,
            hora_venda = HoraVendaPicker.Time,
            CodCliente = selectedCliente.CodCliente,
            CodVendedor = selectedVendedor.CodVendedor,
            Desconto = descontoGeralParsed,
            OBS = ObservacoesEditor.Text?.Trim(),
            forma_pagamento = FormaPagamentoPicker.SelectedItem.ToString(),
            status_venda = selectedStatus.Value,
            valor_total = decimal.Parse(ValorTotalVendaLabel.Text, NumberStyles.Currency, CultureInfo.CurrentCulture) // Get from calculated label
        };
        if (novaVenda.valor_total < 0) novaVenda.valor_total = 0;


        // --- Database Operations (ideally in a transaction) ---
        try
        {
            int newVendaId = await _vendaService.InsertAsync(novaVenda);
            if (newVendaId <= 0) { await DisplayAlert("Erro", "Falha ao salvar a venda principal.", "OK"); return; }

            foreach (var item in ItensVenda)
            {
                item.CodVenda = newVendaId; // Assign the generated Venda ID to each item
                                            // Ensure description and unit price are from the product at the time of sale, not just the picker's display
                var produtoInfo = _masterListaProdutos.FirstOrDefault(p => p.CodProduto == item.CodProduto);
                if (produtoInfo != null)
                {
                    item.Descricao = produtoInfo.Descricao; // Persist description from product master
                                                            // If unit price could be overridden in the item row, use item.valor_unitario.
                                                            // If it's always from product master, use produtoInfo.PrecoUnitario
                                                            // The current item.valor_unitario is set when product is picked.
                }

                await _itemVendaService.InsertAsync(item);

                if (selectedStatus.DisplayName == "Faturada") // Or compare by selectedStatus.Value
                {
                    await _estoqueService.InsertAsync(new EstoqueModel
                    {
                        CodProduto = item.CodProduto.Value,
                        Tipo = 'S', // Saída
                        Qtd = item.quantidade.Value, // Ensure item.quantidade has a value
                        Data = novaVenda.data_venda.GetValueOrDefault(DateTime.Today)
                    });
                    // The AtualizarEstoqueAsync method in your ProdutoService expects an int.
                    // This will truncate if item.quantidade is decimal.
                    // Consider changing ProdutoService.AtualizarEstoqueAsync to accept decimal.
                    await _produtoService.AtualizarEstoqueAsync(item.CodProduto.Value, (int)(-1 * item.quantidade.Value));
                }
            }

            if (selectedStatus.DisplayName == "Faturada") // Or compare by selectedStatus.Value
            {
                await _vendaService.AtualizarClienteUltimaCompraAsync(selectedCliente.CodCliente);
                await _vendedorService.IncrementVendasAsync(selectedVendedor.CodVendedor);
                await _vendedorService.IncrementVendasFinalizadasAsync(selectedVendedor.CodVendedor);
            }
            else if (selectedStatus.DisplayName == "Cancelada") // Or compare by selectedStatus.Value
            {
                // This assumes a sale might be created directly as "Cancelada"
                // or an existing sale might be updated to "Cancelada" (which this form isn't doing).
                // If a sale becomes "Cancelada" after being "Faturada", stock adjustments would be needed (reversal).
                await _vendedorService.IncrementVendasCanceladasAsync(selectedVendedor.CodVendedor);
            }

            await DisplayAlert("Sucesso", "Venda registrada com sucesso!", "OK");
            ClearFormAndItems();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving sale: {ex.ToString()}");
            await DisplayAlert("Erro Crítico", $"Ocorreu um erro ao salvar a venda: {ex.Message}", "OK");
        }
    }

    private void ClearFormAndItems()
    {
        // Reset header fields
        DataVendaPicker.Date = DateTime.Today;
        HoraVendaPicker.Time = DateTime.Now.TimeOfDay;
        ClientePicker.SelectedItem = null;
        VendedorPicker.SelectedItem = null;
        DescontoVendaEntry.Text = "0.00"; // Or string.Empty if you prefer placeholder
        ObservacoesEditor.Text = string.Empty;
        FormaPagamentoPicker.SelectedItem = null; // Or default to first item
        StatusVendaPicker.SelectedIndex = 0; // Default to "Orçamento"

        // Clear items list
        ItensVenda.Clear();
        // RecalculateTotalVenda(); // Called by ItensVenda.CollectionChanged

        ClientePicker.Focus();
    }

    private async void CancelarVendaButton_Clicked(object sender, EventArgs e)
    {
        bool confirmCancel = await DisplayAlert("Cancelar Venda", "Tem certeza que deseja cancelar esta venda? Todas as informações não salvas serão perdidas.", "Sim", "Não");
        if (confirmCancel)
        {
            ClearFormAndItems();
            // Optionally navigate back if this page was pushed
            if (Navigation.NavigationStack.Any() && Navigation.NavigationStack.Last() == this)
            {
                // await Navigation.PopAsync();
            }
        }
    }
}
