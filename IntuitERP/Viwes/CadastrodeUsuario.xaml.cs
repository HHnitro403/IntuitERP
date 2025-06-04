using IntuitERP.models;
using IntuitERP.Services;

namespace IntuitERP.Viwes;

public partial class CadastrodeUsuario : ContentPage
{

    private readonly UsuarioService _usuarioService;
    public CadastrodeUsuario(UsuarioService usuarioService)
    {
        InitializeComponent();
        _usuarioService = usuarioService;
    }

    private void ClearForm()
    {
        UsuarioEntry.Text = string.Empty;
        SenhaEntry.Text = string.Empty;
        ConfirmarSenhaEntry.Text = string.Empty;

        // Reset all permission switches to false (or their default state)
        PermissaoProdutosCreateSwitch.IsToggled = false;
        PermissaoProdutosReadSwitch.IsToggled = false;
        PermissaoProdutosUpdateSwitch.IsToggled = false;
        PermissaoProdutosDeleteSwitch.IsToggled = false;

        PermissaoVendasCreateSwitch.IsToggled = false;
        PermissaoVendasReadSwitch.IsToggled = false;
        PermissaoVendasUpdateSwitch.IsToggled = false;
        PermissaoVendasDeleteSwitch.IsToggled = false;

        PermissaoVendedoresCreateSwitch.IsToggled = false;
        PermissaoVendedoresReadSwitch.IsToggled = false;
        PermissaoVendedoresUpdateSwitch.IsToggled = false;
        PermissaoVendedoresDeleteSwitch.IsToggled = false;

        PermissaoFornecedoresCreateSwitch.IsToggled = false;
        PermissaoFornecedoresReadSwitch.IsToggled = false;
        PermissaoFornecedoresUpdateSwitch.IsToggled = false;
        PermissaoFornecedoresDeleteSwitch.IsToggled = false;

        PermissaoClientesCreateSwitch.IsToggled = false;
        PermissaoClientesReadSwitch.IsToggled = false;
        PermissaoClientesUpdateSwitch.IsToggled = false;
        PermissaoClientesDeleteSwitch.IsToggled = false;

        PermissaoRelatoriosGenerateSwitch.IsToggled = false;

        UsuarioEntry.Focus();
    }

    private async void SalvarUsuarioButton_Clicked(object sender, EventArgs e)
    {
        // --- Basic Validation ---
        if (string.IsNullOrWhiteSpace(UsuarioEntry.Text))
        {
            await DisplayAlert("Campo Obrigatório", "Por favor, preencha o nome de Usuário.", "OK");
            UsuarioEntry.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(SenhaEntry.Text))
        {
            await DisplayAlert("Campo Obrigatório", "Por favor, preencha a Senha.", "OK");
            SenhaEntry.Focus();
            return;
        }

        if (SenhaEntry.Text.Length < 6) // Example: Minimum password length
        {
            await DisplayAlert("Senha Inválida", "A senha deve ter pelo menos 6 caracteres.", "OK");
            SenhaEntry.Focus();
            return;
        }

        if (SenhaEntry.Text != ConfirmarSenhaEntry.Text)
        {
            await DisplayAlert("Senhas Não Conferem", "A senha e a confirmação de senha não são iguais.", "OK");
            ConfirmarSenhaEntry.Focus();
            return;
        }

        // --- Create UsuarioModel ---
        var novoUsuario = new UsuarioModel
        {
            UsuarioNome = UsuarioEntry.Text.Trim(),
            // In a real application, hash the password before saving!
            // For this example, we're saving it as plain text, which is NOT secure.
            Senha = SenhaEntry.Text, // IMPORTANT: HASH THIS IN A REAL APP

            // Produtos Permissions
            PermissaoProdutosCreate = PermissaoProdutosCreateSwitch.IsToggled,
            PermissaoProdutosRead = PermissaoProdutosReadSwitch.IsToggled,
            PermissaoProdutosUpdate = PermissaoProdutosUpdateSwitch.IsToggled,
            PermissaoProdutosDelete = PermissaoProdutosDeleteSwitch.IsToggled,

            // Vendas Permissions
            PermissaoVendasCreate = PermissaoVendasCreateSwitch.IsToggled,
            PermissaoVendasRead = PermissaoVendasReadSwitch.IsToggled,
            PermissaoVendasUpdate = PermissaoVendasUpdateSwitch.IsToggled,
            PermissaoVendasDelete = PermissaoVendasDeleteSwitch.IsToggled,

            // Vendedores Permissions
            PermissaoVendedoresCreate = PermissaoVendedoresCreateSwitch.IsToggled,
            PermissaoVendedoresRead = PermissaoVendedoresReadSwitch.IsToggled,
            PermissaoVendedoresUpdate = PermissaoVendedoresUpdateSwitch.IsToggled,
            PermissaoVendedoresDelete = PermissaoVendedoresDeleteSwitch.IsToggled,

            // Fornecedores Permissions
            PermissaoFornecedoresCreate = PermissaoFornecedoresCreateSwitch.IsToggled,
            PermissaoFornecedoresRead = PermissaoFornecedoresReadSwitch.IsToggled,
            PermissaoFornecedoresUpdate = PermissaoFornecedoresUpdateSwitch.IsToggled,
            PermissaoFornecedoresDelete = PermissaoFornecedoresDeleteSwitch.IsToggled,

            // Clientes Permissions
            PermissaoClientesCreate = PermissaoClientesCreateSwitch.IsToggled,
            PermissaoClientesRead = PermissaoClientesReadSwitch.IsToggled,
            PermissaoClientesUpdate = PermissaoClientesUpdateSwitch.IsToggled,
            PermissaoClientesDelete = PermissaoClientesDeleteSwitch.IsToggled,

            // Relatorios Permission
            PermissaoRelatoriosGenerate = PermissaoRelatoriosGenerateSwitch.IsToggled
        };

        try
        {
            // Check if username already exists
            var existingUser = await _usuarioService.GetByUsuarioAsync(novoUsuario.UsuarioNome);
            if (existingUser != null)
            {
                await DisplayAlert("Usuário Existente", $"O nome de usuário '{novoUsuario.UsuarioNome}' já está em uso.", "OK");
                UsuarioEntry.Focus();
                return;
            }

            int newUsuarioId = await _usuarioService.InsertAsync(novoUsuario);

            if (newUsuarioId > 0)
            {
                await DisplayAlert("Sucesso", "Usuário cadastrado com sucesso!", "OK");
                ClearForm();
                // Optionally, navigate away or update a list
                // await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Erro", "Não foi possível cadastrar o usuário. Verifique os dados e tente novamente.", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving user: {ex.Message}");
            // Consider more specific error messages based on the exception type
            await DisplayAlert("Erro Inesperado", $"Ocorreu um erro ao salvar o usuário: {ex.Message}", "OK");
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