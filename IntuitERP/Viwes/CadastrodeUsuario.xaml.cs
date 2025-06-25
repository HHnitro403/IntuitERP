using IntuitERP.models;
using IntuitERP.Services;

namespace IntuitERP.Viwes;

public partial class CadastrodeUsuario : ContentPage
{

    private readonly UsuarioService _usuarioService;
    private readonly int  _id = 0;
    public CadastrodeUsuario(UsuarioService usuarioService, int id = 0)
    {
        InitializeComponent();
        _usuarioService = usuarioService;
        if (id != 0)
        {
            _id = id;
        }
    }

    protected override async void OnAppearing()
    {
        var usuario = await _usuarioService.GetByIdAsync(_id);
        base.OnAppearing();
        if (_id != 0)
        {
            UsuarioEntry.Text = usuario.Usuario.ToString();
            SenhaEntry.Text = usuario.Senha.ToString();
            ConfirmarSenhaEntry.Text = "";
            //permissoes
            PermissaoClientesCreateSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoClientesCreate);
            PermissaoClientesReadSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoClientesRead);
            PermissaoClientesUpdateSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoClientesUpdate);
            PermissaoClientesDeleteSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoClientesDelete);

            PermissaoProdutosCreateSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoProdutosCreate);
            PermissaoProdutosReadSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoProdutosRead);
            PermissaoProdutosUpdateSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoProdutosUpdate);
            PermissaoProdutosDeleteSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoProdutosDelete);

            PermissaoVendasCreateSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoVendasCreate);
            PermissaoVendasReadSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoVendasRead);
            PermissaoVendasUpdateSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoVendasUpdate);
            PermissaoVendasDeleteSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoVendasDelete);

            PermissaoVendedoresCreateSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoVendedoresCreate);
            PermissaoVendedoresReadSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoVendedoresRead);
            PermissaoVendedoresUpdateSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoVendedoresUpdate);
            PermissaoVendedoresDeleteSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoVendedoresDelete);

            PermissaoFornecedoresCreateSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoFornecedoresCreate);
            PermissaoFornecedoresReadSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoFornecedoresRead);
            PermissaoFornecedoresUpdateSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoFornecedoresUpdate);
            PermissaoFornecedoresDeleteSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoFornecedoresDelete);

            PermissaoRelatoriosGenerateSwitch.IsToggled = Convert.ToBoolean(usuario.PermissaoRelatoriosGenerate);


        }
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
        var usuario = new UsuarioModel
        {
            Usuario = UsuarioEntry.Text.Trim(),
            // In a real application, hash the password before saving!
            // For this example, we're saving it as plain text, which is NOT secure.
            Senha = SenhaEntry.Text, // IMPORTANT: HASH THIS IN A REAL APP

            // Produtos Permissions
            PermissaoProdutosCreate = Convert.ToInt32(PermissaoProdutosCreateSwitch.IsToggled),
            PermissaoProdutosRead = Convert.ToInt32(PermissaoProdutosReadSwitch.IsToggled),
            PermissaoProdutosUpdate = Convert.ToInt32(PermissaoProdutosUpdateSwitch.IsToggled),
            PermissaoProdutosDelete = Convert.ToInt32(PermissaoProdutosDeleteSwitch.IsToggled),

            // Vendas Permissions
            PermissaoVendasCreate = Convert.ToInt32(PermissaoVendasCreateSwitch.IsToggled),
            PermissaoVendasRead = Convert.ToInt32(PermissaoVendasReadSwitch.IsToggled),
            PermissaoVendasUpdate = Convert.ToInt32(PermissaoVendasUpdateSwitch.IsToggled),
            PermissaoVendasDelete = Convert.ToInt32(PermissaoVendasDeleteSwitch.IsToggled),

            // Vendedores Permissions
            PermissaoVendedoresCreate = Convert.ToInt32(PermissaoVendedoresCreateSwitch.IsToggled),
            PermissaoVendedoresRead = Convert.ToInt32(PermissaoVendedoresReadSwitch.IsToggled),
            PermissaoVendedoresUpdate = Convert.ToInt32(PermissaoVendedoresUpdateSwitch.IsToggled),
            PermissaoVendedoresDelete = Convert.ToInt32(PermissaoVendedoresDeleteSwitch.IsToggled),

            // Fornecedores Permissions
            PermissaoFornecedoresCreate = Convert.ToInt32(PermissaoFornecedoresCreateSwitch.IsToggled),
            PermissaoFornecedoresRead = Convert.ToInt32(PermissaoFornecedoresReadSwitch.IsToggled),
            PermissaoFornecedoresUpdate = Convert.ToInt32(PermissaoFornecedoresUpdateSwitch.IsToggled),
            PermissaoFornecedoresDelete = Convert.ToInt32(PermissaoFornecedoresDeleteSwitch.IsToggled),

            // Clientes Permissions
            PermissaoClientesCreate = Convert.ToInt32(PermissaoClientesCreateSwitch.IsToggled),
            PermissaoClientesRead = Convert.ToInt32(PermissaoClientesReadSwitch.IsToggled),
            PermissaoClientesUpdate = Convert.ToInt32(PermissaoClientesUpdateSwitch.IsToggled),
            PermissaoClientesDelete = Convert.ToInt32(PermissaoClientesDeleteSwitch.IsToggled),

            // Relatorios Permission
            PermissaoRelatoriosGenerate = Convert.ToInt32(PermissaoRelatoriosGenerateSwitch.IsToggled)
        };

        try
        {
            if (_id != 0)
            {
                usuario.CodUsuarios = _id;
                var updateuser = await _usuarioService.UpdateAsync(usuario); // up

                if (updateuser > 0)
                {
                    await DisplayAlert("Sucesso", "Usuário atualizado com sucesso!", "OK");
                    await Navigation.PopAsync();
                }
            }
            else
            {
                // Check if username already exists
                var existingUser = await _usuarioService.GetByUsuarioAsync(usuario.Usuario);
                if (existingUser != null)
                {
                    await DisplayAlert("Usuário Existente", $"O nome de usuário '{usuario.Usuario}' já está em uso.", "OK");
                    UsuarioEntry.Focus();
                    return;
                }

                int newUsuarioId = await _usuarioService.InsertAsync(usuario);

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