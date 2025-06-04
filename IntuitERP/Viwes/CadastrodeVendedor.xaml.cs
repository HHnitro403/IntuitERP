using IntuitERP.models;
using IntuitERP.Services;

namespace IntuitERP.Viwes;

public partial class CadastrodeVendedor : ContentPage
{
    private readonly VendedorService _vendedorService;

    // Constructor for Dependency Injection (recommended)
    public CadastrodeVendedor(VendedorService vendedorService)
    {
        InitializeComponent();
        _vendedorService = vendedorService;

        // Initialize read-only fields (though they are already set to "0" in XAML)
        // This is more for programmatic consistency if needed elsewhere.
        TotalVendasEntry.Text = "0";
        VendasFinalizadasEntry.Text = "0";
        VendasCanceladasEntry.Text = "0";

    }

    private void ClearForm()
    {
        NomeVendedorEntry.Text = string.Empty;
        // Read-only fields remain as they are (or reset to "0" if they could change,
        // but for a "new" vendor form, "0" is the correct initial state).
        TotalVendasEntry.Text = "0";
        VendasFinalizadasEntry.Text = "0";
        VendasCanceladasEntry.Text = "0";

        NomeVendedorEntry.Focus();
    }

    private async void SalvarVendedorButton_Clicked(object sender, EventArgs e)
    {
        // --- Basic Validation ---
        if (string.IsNullOrWhiteSpace(NomeVendedorEntry.Text))
        {
            await DisplayAlert("Campo Obrigatório", "Por favor, preencha o Nome do Vendedor.", "OK");
            NomeVendedorEntry.Focus();
            return;
        }

        // --- Create VendedorModel ---
        // For a new vendor, sales-related fields (totalvendas, etc.)
        // are typically initialized to 0 by the service if not provided.
        var novoVendedor = new VendedorModel
        {
            NomeVendedor = NomeVendedorEntry.Text.Trim()
            // totalvendas, vendasfinalizadas, vendascanceladas will be defaulted to 0
            // by the VendedorService.InsertAsync if not set or if set to null.
        };

        try
        {
            // Optional: Check if a vendor with the same name already exists.
            // This would require an additional method in your VendedorService, e.g., GetByNomeAsync.
            // Example:
            //var existingVendedor = await _vendedorService.get(novoVendedor.NomeVendedor);
            //if (existingVendedor != null)
            //{
            //    await DisplayAlert("Duplicidade", $"Já existe um vendedor com o nome: {novoVendedor.NomeVendedor}", "OK");
            //    NomeVendedorEntry.Focus();
            //  return;
            //}

            int newVendedorId = await _vendedorService.InsertAsync(novoVendedor);

            if (newVendedorId > 0)
            {
                await DisplayAlert("Sucesso", "Vendedor cadastrado com sucesso!", "OK");
                ClearForm();
                // Optionally, navigate away or update a list
                // await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Erro", "Não foi possível cadastrar o vendedor. Verifique os dados e tente novamente.", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving vendor: {ex.Message}");
            await DisplayAlert("Erro Inesperado", $"Ocorreu um erro ao salvar o vendedor: {ex.Message}", "OK");
        }
    }

    private async void CancelarButton_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Cancelar Cadastro", "Tem certeza que deseja cancelar o cadastro? Todas as informações não salvas serão perdidas.", "Sim", "Não");
        if (confirm)
        {
            ClearForm();
            if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync();
            }
        }
    }
}