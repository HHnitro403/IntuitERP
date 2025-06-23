using IntuitERP.models;
using IntuitERP.Services;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;

namespace IntuitERP.Viwes;

public partial class CadastrodeCompra : ContentPage
{
    private readonly CompraService _compraService;
    private readonly ItemCompraService _itemCompraService;
    private readonly FornecedorService _fornecedorService;
    private readonly ProdutoService _produtoService;
    private readonly VendedorService _vendedorService; // Changed from UsuarioService
    private readonly EstoqueService _estoqueService;

    // These are public for the Product Picker's ItemsSource binding in XAML
    public ObservableCollection<ProdutoModel> AvailableProducts { get; private set; }

    private ObservableCollection<FornecedorModel> _availableFornecedoresInternal;
    private ObservableCollection<VendedorModel> _availableVendedoresInternal; // Changed from UsuarioModel
    private ObservableCollection<ItemCompraModel> _itensCompraInternal;
    private readonly int _compraId;


    public CadastrodeCompra(
        CompraService compraService,
        ItemCompraService itemCompraService,
        FornecedorService fornecedorService,
        ProdutoService produtoService,
        VendedorService vendedorService, // Changed from UsuarioService
        EstoqueService estoqueService, int compraId = 0)
    {
        InitializeComponent();

        _compraService = compraService;
        _itemCompraService = itemCompraService;
        _fornecedorService = fornecedorService;
        _produtoService = produtoService;
        _vendedorService = vendedorService; // Changed from _usuarioService
        _estoqueService = estoqueService;


        _availableFornecedoresInternal = new ObservableCollection<FornecedorModel>();
        _availableVendedoresInternal = new ObservableCollection<VendedorModel>(); // Changed from UsuarioModel
        AvailableProducts = new ObservableCollection<ProdutoModel>(); // Public for XAML binding
        _itensCompraInternal = new ObservableCollection<ItemCompraModel>();

        // Set ItemsSource for pickers and CollectionView
        FornecedorPicker.ItemsSource = _availableFornecedoresInternal;
        FornecedorPicker.ItemDisplayBinding = new Binding("NomeFantasia"); // Or RazaoSocial, as appropriate

        VendedorPicker.ItemsSource = _availableVendedoresInternal;
        VendedorPicker.ItemDisplayBinding = new Binding("NomeVendedor"); // Changed from "Usuario" to "NomeVendedor"

        ItensCompraCollectionView.ItemsSource = _itensCompraInternal;

        // Initialize default values
        DataCompraPicker.Date = DateTime.Today;
        HoraCompraPicker.Time = DateTime.Now.TimeOfDay;

        _compraId = compraId;
        // Set BindingContext to the page itself for XAML bindings to page properties (like AvailableProducts)
        this.BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadInitialDataAsync();
        if (_compraId > 0)
        {
            var compra = await _compraService.GetByIdAsync(_compraId);
            if (compra != null)
            {
                FornecedorPicker.SelectedItem = compra.Fornecedor;
                VendedorPicker.SelectedItem = compra.Vendedor;
                DataCompraPicker.Date = (DateTime)compra.data_compra;
                HoraCompraPicker.Time = (TimeSpan)compra.hora_compra;
                _itensCompraInternal.Clear();

                var itensCompra = await _itemCompraService.GetByCompraAsync(_compraId);

                foreach (var item in itensCompra)
                {
                    _itensCompraInternal.Add(item);
                }
            }
        }
    }

    private async Task LoadInitialDataAsync()
    {
        try
        {
            var fornecedores = await _fornecedorService.GetAllAsync();
            _availableFornecedoresInternal.Clear();
            foreach (var f in fornecedores.OrderBy(fn => fn.NomeFantasia)) _availableFornecedoresInternal.Add(f);

            var vendedores = await _vendedorService.GetAllAsync(); // Changed from _usuarioService
            _availableVendedoresInternal.Clear();
            foreach (var v in vendedores.OrderBy(u => u.NomeVendedor)) _availableVendedoresInternal.Add(v); // Changed from u.Usuario to u.NomeVendedor

            var produtos = await _produtoService.GetAllAsync();
            AvailableProducts.Clear();
            foreach (var p in produtos.OrderBy(pr => pr.Descricao)) AvailableProducts.Add(p);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading initial data: {ex.Message}");
            await DisplayAlert("Erro", $"Falha ao carregar dados iniciais: {ex.Message}", "OK");
        }
    }

    private void AdicionarItemButton_Clicked(object sender, EventArgs e)
    {
        var newItem = new ItemCompraModel { quantidade = 1, desconto = 0m }; // Sensible defaults
        _itensCompraInternal.Add(newItem);
        RecalculateGrandTotal();
    }

    private void ItemProductPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        if (picker?.BindingContext is ItemCompraModel itemModel && picker.SelectedItem is ProdutoModel selectedProduct)
        {
            itemModel.CodProduto = selectedProduct.CodProduto;
            itemModel.Descricao = selectedProduct.Descricao;
            itemModel.valor_unitario = selectedProduct.PrecoUnitario;

            UpdateSpecificItemTotalLabel(picker, itemModel);
            RecalculateGrandTotal();
        }
    }

    private void ItemEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = sender as Entry;
        if (entry?.BindingContext is ItemCompraModel itemModel)
        {
            // Model property is updated by two-way binding.
            UpdateSpecificItemTotalLabel(entry, itemModel);
            RecalculateGrandTotal();
        }
    }

    private void UpdateSpecificItemTotalLabel(Element controlInItemTemplate, ItemCompraModel itemModel)
    {
        if (itemModel == null || controlInItemTemplate == null) return;

        decimal quantidade = itemModel.quantidade ?? 0m;
        decimal valorUnitario = itemModel.valor_unitario ?? 0m;
        decimal descontoItem = itemModel.desconto ?? 0m;
        itemModel.valor_total = (quantidade * valorUnitario) - descontoItem;

        var itemViewRoot = GetItemViewRoot(controlInItemTemplate);

        if (itemViewRoot != null)
        {
            var totalLabel = FindVisualChild<Label>(itemViewRoot, el => el.AutomationId == "ValorTotalItemLabel");
            if (totalLabel != null)
            {
                var fs = new FormattedString();
                fs.Spans.Add(new Span { Text = "Total: R$ " });
                fs.Spans.Add(new Span { Text = (itemModel.valor_total ?? 0m).ToString("N2", CultureInfo.GetCultureInfo("pt-BR")) });
                totalLabel.FormattedText = fs;
            }
        }
    }

    private Element GetItemViewRoot(Element element)
    {
        if (element == null) return null;
        Element current = element;
        while (current != null)
        {
            var parent = current.Parent;
            if (parent == null || parent is CollectionView || (parent.BindingContext != current.BindingContext && current.BindingContext is ItemCompraModel))
            {
                return current;
            }
            current = parent;
        }
        return element;
    }


    public static T FindVisualChild<T>(Element element, Func<T, bool> predicate) where T : Element
    {
        if (element == null) return null;
        if (element is T typedElement && predicate(typedElement)) return typedElement;

        foreach (var child in GetVisualChildren(element))
        {
            var result = FindVisualChild(child, predicate);
            if (result != null) return result;
        }
        return null;
    }

    public static System.Collections.Generic.IEnumerable<Element> GetVisualChildren(Element element)
    {
        if (element is Layout layout)
        {
            foreach (var child in layout.Children)
            {
                if (child is Element childElement)
                {
                    yield return childElement;
                }
            }
        }
        else if (element is ContentView contentView && contentView.Content is Element contentElement)
        {
            yield return contentElement;
        }
        else if (element is Frame frame && frame.Content is Element frameContent)
        {
            yield return frameContent;
        }
        else if (element is ScrollView scrollView && scrollView.Content is Element scrollContent)
        {
            yield return scrollContent;
        }
        else if (element is Border border && border.Content is Element borderContent)
        {
            yield return borderContent;
        }
    }


    private void RecalculateGrandTotal()
    {
        decimal subTotalItens = _itensCompraInternal.Sum(item =>
        {
            decimal qty = item.quantidade ?? 0m;
            decimal unitPrice = item.valor_unitario ?? 0m;
            decimal itemDiscount = item.desconto ?? 0m;
            return (qty * unitPrice) - itemDiscount;
        });

        decimal descontoGeral = 0m;
        if (decimal.TryParse(DescontoCompraEntry.Text, NumberStyles.Currency, CultureInfo.GetCultureInfo("pt-BR"), out var desc))
        {
            descontoGeral = desc;
        }
        else if (decimal.TryParse(DescontoCompraEntry.Text, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-BR"), out desc))
        {
            descontoGeral = desc;
        }

        decimal totalFinal = subTotalItens - descontoGeral;
        ValorTotalCompraLabel.Text = totalFinal.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));
    }

    private void RemoverItem_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ItemCompraModel itemToRemove)
        {
            _itensCompraInternal.Remove(itemToRemove);
            RecalculateGrandTotal();
        }
    }

    private byte? MapStatusCompraToByte(string statusString)
    {
        if (string.IsNullOrWhiteSpace(statusString)) return null;

        switch (statusString.ToLowerInvariant())
        {
            case "pendente": return 0;
            case "em processamento": return 1;
            case "concluída": return 2; // Assuming 'Concluída' maps to 2
            case "cancelada": return 3; // Assuming 'Cancelada' maps to 3
            default:
                Console.WriteLine($"Unknown compra status: {statusString}");
                return null;
        }
    }


    private async void SalvarCompraButton_Clicked(object sender, EventArgs e)
    {
        if (FornecedorPicker.SelectedItem == null)
        {
            await DisplayAlert("Validação", "Selecione um fornecedor.", "OK"); return;
        }
        if (VendedorPicker.SelectedItem == null)
        {
            await DisplayAlert("Validação", "Selecione um vendedor.", "OK"); return;
        }
        if (FormaPagamentoPicker.SelectedItem == null)
        {
            await DisplayAlert("Validação", "Selecione uma forma de pagamento.", "OK"); return;
        }

        byte? statusCompraByte = MapStatusCompraToByte(StatusCompraPicker.SelectedItem?.ToString());
        if (StatusCompraPicker.SelectedItem == null || statusCompraByte == null)
        {
            await DisplayAlert("Validação", "Selecione um status válido para a compra.", "OK"); return;
        }

        if (!_itensCompraInternal.Any())
        {
            await DisplayAlert("Validação", "Adicione pelo menos um item à compra.", "OK"); return;
        }

        foreach (var itemModel in _itensCompraInternal)
        {
            if (!itemModel.CodProduto.HasValue || itemModel.CodProduto == 0)
            {
                await DisplayAlert("Validação", "Todos os itens devem ter um produto selecionado.", "OK"); return;
            }
            if (!itemModel.quantidade.HasValue || itemModel.quantidade <= 0)
            {
                await DisplayAlert("Validação", $"O item '{itemModel.Descricao ?? "Desconhecido"}' deve ter uma quantidade válida.", "OK"); return;
            }
            // Ensure the itemModel.valor_total is up-to-date before saving.
            // This calculation should mirror what UpdateSpecificItemTotalLabel does for the model part.
            decimal quantidade = itemModel.quantidade ?? 0m;
            decimal valorUnitario = itemModel.valor_unitario ?? 0m;
            decimal descontoItem = itemModel.desconto ?? 0m;
            itemModel.valor_total = (quantidade * valorUnitario) - descontoItem;
        }
        RecalculateGrandTotal(); // Recalculate grand total based on potentially updated item totals.

        var compra = new CompraModel
        {
            data_compra = DataCompraPicker.Date,
            hora_compra = HoraCompraPicker.Time,
            CodFornec = (FornecedorPicker.SelectedItem as FornecedorModel)?.CodFornecedor,
            CodVendedor = (VendedorPicker.SelectedItem as VendedorModel)?.CodVendedor,
            OBS = ObservacoesEditor.Text,
            forma_pagamento = FormaPagamentoPicker.SelectedItem?.ToString(),
            status_compra = statusCompraByte
        };

        if (decimal.TryParse(DescontoCompraEntry.Text, NumberStyles.Currency, CultureInfo.GetCultureInfo("pt-BR"), out var d))
            compra.Desconto = d;
        else if (decimal.TryParse(DescontoCompraEntry.Text, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-BR"), out d))
            compra.Desconto = d;
        else
            compra.Desconto = 0m;

        if (decimal.TryParse(ValorTotalCompraLabel.Text, NumberStyles.Currency, CultureInfo.GetCultureInfo("pt-BR"), out var vt))
            compra.valor_total = vt;
        else if (decimal.TryParse(ValorTotalCompraLabel.Text, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-BR"), out vt))
            compra.valor_total = vt;
        else
        {
            await DisplayAlert("Erro de Cálculo", "Não foi possível determinar o valor total da compra.", "OK");
            return;
        }

        try
        {
            int compraId = await _compraService.InsertAsync(compra);
            if (compraId > 0)
            {
                foreach (var itemModel in _itensCompraInternal)
                {
                    itemModel.CodCompra = compraId;
                    await _itemCompraService.InsertAsync(itemModel);

                    if (itemModel.CodProduto.HasValue && itemModel.quantidade.HasValue)
                    {
                        await _produtoService.AtualizarEstoqueAsync(itemModel.CodProduto.Value, (int)itemModel.quantidade.Value);
                    }
                }

                if (compra.CodFornec.HasValue)
                {
                    await _fornecedorService.UpdateUltimaCompraAsync(compra.CodFornec.Value);
                }

                await DisplayAlert("Sucesso", "Compra salva com sucesso!", "OK");
                ClearForm();
            }
            else
            {
                await DisplayAlert("Erro", "Falha ao salvar a compra principal.", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving Compra: {ex.Message}");
            await DisplayAlert("Erro Crítico", $"Ocorreu um erro ao salvar a compra: {ex.Message}", "OK");
        }
    }

    private void CancelarCompraButton_Clicked(object sender, EventArgs e)
    {
        ClearForm();
    }

    private void ClearForm()
    {
        DataCompraPicker.Date = DateTime.Today;
        HoraCompraPicker.Time = DateTime.Now.TimeOfDay;
        FornecedorPicker.SelectedItem = null;
        VendedorPicker.SelectedItem = null;
        DescontoCompraEntry.Text = string.Empty;
        ObservacoesEditor.Text = string.Empty;
        FormaPagamentoPicker.SelectedItem = null;
        StatusCompraPicker.SelectedItem = null;
        _itensCompraInternal.Clear();
        ValorTotalCompraLabel.Text = (0m).ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));
    }
}