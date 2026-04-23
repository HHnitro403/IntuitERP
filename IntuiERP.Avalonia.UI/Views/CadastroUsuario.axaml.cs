using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using IntuiERP.Avalonia.UI.models;
using IntuiERP.Avalonia.UI.Services;
using IntuiERP.Avalonia.UI.Helpers;
using IntuiERP.Avalonia.UI.Views.Search;
using System;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.Views;

public partial class CadastroUsuario : UserControl
{
    private readonly UsuarioService _usuarioService;
    private readonly int _usuarioId;

    public CadastroUsuario(int id = 0)
    {
        InitializeComponent();
        
        var factory = new NpgsqlConnectionFactory();
        _usuarioService = new UsuarioService(factory);
        _usuarioId = id;

        SalvarUsuarioButton.Click += SalvarUsuarioButton_Clicked;
        CancelarButton.Click += CancelarButton_Clicked;

        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (_usuarioId != 0)
        {
            HeaderLabel.Text = "Editar Usuário";
            try
            {
                var usuario = await _usuarioService.GetByIdAsync(_usuarioId);
                if (usuario != null)
                {
                    UsuarioEntry.Text = usuario.Usuario;
                    SenhaEntry.Text = usuario.Senha;
                    ConfirmarSenhaEntry.Text = string.Empty;

                    // Permissions
                    PermissaoClientesCreateSwitch.IsChecked = usuario.PermissaoClientesCreate > 0;
                    PermissaoClientesReadSwitch.IsChecked = usuario.PermissaoClientesRead > 0;
                    PermissaoClientesUpdateSwitch.IsChecked = usuario.PermissaoClientesUpdate > 0;
                    PermissaoClientesDeleteSwitch.IsChecked = usuario.PermissaoClientesDelete > 0;

                    PermissaoProdutosCreateSwitch.IsChecked = usuario.PermissaoProdutosCreate > 0;
                    PermissaoProdutosReadSwitch.IsChecked = usuario.PermissaoProdutosRead > 0;
                    PermissaoProdutosUpdateSwitch.IsChecked = usuario.PermissaoProdutosUpdate > 0;
                    PermissaoProdutosDeleteSwitch.IsChecked = usuario.PermissaoProdutosDelete > 0;

                    PermissaoVendasCreateSwitch.IsChecked = usuario.PermissaoVendasCreate > 0;
                    PermissaoVendasReadSwitch.IsChecked = usuario.PermissaoVendasRead > 0;
                    PermissaoVendasUpdateSwitch.IsChecked = usuario.PermissaoVendasUpdate > 0;
                    PermissaoVendasDeleteSwitch.IsChecked = usuario.PermissaoVendasDelete > 0;

                    PermissaoVendedoresCreateSwitch.IsChecked = usuario.PermissaoVendedoresCreate > 0;
                    PermissaoVendedoresReadSwitch.IsChecked = usuario.PermissaoVendedoresRead > 0;
                    PermissaoVendedoresUpdateSwitch.IsChecked = usuario.PermissaoVendedoresUpdate > 0;
                    PermissaoVendedoresDeleteSwitch.IsChecked = usuario.PermissaoVendedoresDelete > 0;

                    PermissaoFornecedoresCreateSwitch.IsChecked = usuario.PermissaoFornecedoresCreate > 0;
                    PermissaoFornecedoresReadSwitch.IsChecked = usuario.PermissaoFornecedoresRead > 0;
                    PermissaoFornecedoresUpdateSwitch.IsChecked = usuario.PermissaoFornecedoresUpdate > 0;
                    PermissaoFornecedoresDeleteSwitch.IsChecked = usuario.PermissaoFornecedoresDelete > 0;

                    PermissaoRelatoriosGenerateSwitch.IsChecked = usuario.PermissaoRelatoriosGenerate > 0;
                }
            }
            catch (Exception ex)
            {
                if (VisualRoot is Window window)
                    await MessageBox.Show(window, $"Erro ao carregar usuário: {ex.Message}", "Erro");
            }
        }
    }

    private void ClearForm()
    {
        UsuarioEntry.Text = string.Empty;
        SenhaEntry.Text = string.Empty;
        ConfirmarSenhaEntry.Text = string.Empty;

        PermissaoProdutosCreateSwitch.IsChecked = false;
        PermissaoProdutosReadSwitch.IsChecked = false;
        PermissaoProdutosUpdateSwitch.IsChecked = false;
        PermissaoProdutosDeleteSwitch.IsChecked = false;

        PermissaoVendasCreateSwitch.IsChecked = false;
        PermissaoVendasReadSwitch.IsChecked = false;
        PermissaoVendasUpdateSwitch.IsChecked = false;
        PermissaoVendasDeleteSwitch.IsChecked = false;

        PermissaoVendedoresCreateSwitch.IsChecked = false;
        PermissaoVendedoresReadSwitch.IsChecked = false;
        PermissaoVendedoresUpdateSwitch.IsChecked = false;
        PermissaoVendedoresDeleteSwitch.IsChecked = false;

        PermissaoFornecedoresCreateSwitch.IsChecked = false;
        PermissaoFornecedoresReadSwitch.IsChecked = false;
        PermissaoFornecedoresUpdateSwitch.IsChecked = false;
        PermissaoFornecedoresDeleteSwitch.IsChecked = false;

        PermissaoClientesCreateSwitch.IsChecked = false;
        PermissaoClientesReadSwitch.IsChecked = false;
        PermissaoClientesUpdateSwitch.IsChecked = false;
        PermissaoClientesDeleteSwitch.IsChecked = false;

        PermissaoRelatoriosGenerateSwitch.IsChecked = false;

        UsuarioEntry.Focus();
    }

    private async void SalvarUsuarioButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window) return;

        if (string.IsNullOrWhiteSpace(UsuarioEntry.Text))
        {
            await MessageBox.Show(window, "Por favor, preencha o nome de Usuário.", "Campo Obrigatório");
            UsuarioEntry.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(SenhaEntry.Text))
        {
            await MessageBox.Show(window, "Por favor, preencha a Senha.", "Campo Obrigatório");
            SenhaEntry.Focus();
            return;
        }

        if (SenhaEntry.Text != ConfirmarSenhaEntry.Text)
        {
            await MessageBox.Show(window, "A senha e a confirmação de senha não são iguais.", "Senhas Não Conferem");
            ConfirmarSenhaEntry.Focus();
            return;
        }

        var usuario = new UsuarioModel
        {
            Usuario = UsuarioEntry.Text.Trim(),
            Senha = SenhaEntry.Text,

            PermissaoProdutosCreate = PermissaoProdutosCreateSwitch.IsChecked == true ? 1 : 0,
            PermissaoProdutosRead = PermissaoProdutosReadSwitch.IsChecked == true ? 1 : 0,
            PermissaoProdutosUpdate = PermissaoProdutosUpdateSwitch.IsChecked == true ? 1 : 0,
            PermissaoProdutosDelete = PermissaoProdutosDeleteSwitch.IsChecked == true ? 1 : 0,

            PermissaoVendasCreate = PermissaoVendasCreateSwitch.IsChecked == true ? 1 : 0,
            PermissaoVendasRead = PermissaoVendasReadSwitch.IsChecked == true ? 1 : 0,
            PermissaoVendasUpdate = PermissaoVendasUpdateSwitch.IsChecked == true ? 1 : 0,
            PermissaoVendasDelete = PermissaoVendasDeleteSwitch.IsChecked == true ? 1 : 0,

            PermissaoVendedoresCreate = PermissaoVendedoresCreateSwitch.IsChecked == true ? 1 : 0,
            PermissaoVendedoresRead = PermissaoVendedoresReadSwitch.IsChecked == true ? 1 : 0,
            PermissaoVendedoresUpdate = PermissaoVendedoresUpdateSwitch.IsChecked == true ? 1 : 0,
            PermissaoVendedoresDelete = PermissaoVendedoresDeleteSwitch.IsChecked == true ? 1 : 0,

            PermissaoFornecedoresCreate = PermissaoFornecedoresCreateSwitch.IsChecked == true ? 1 : 0,
            PermissaoFornecedoresRead = PermissaoFornecedoresReadSwitch.IsChecked == true ? 1 : 0,
            PermissaoFornecedoresUpdate = PermissaoFornecedoresUpdateSwitch.IsChecked == true ? 1 : 0,
            PermissaoFornecedoresDelete = PermissaoFornecedoresDeleteSwitch.IsChecked == true ? 1 : 0,

            PermissaoClientesCreate = PermissaoClientesCreateSwitch.IsChecked == true ? 1 : 0,
            PermissaoClientesRead = PermissaoClientesReadSwitch.IsChecked == true ? 1 : 0,
            PermissaoClientesUpdate = PermissaoClientesUpdateSwitch.IsChecked == true ? 1 : 0,
            PermissaoClientesDelete = PermissaoClientesDeleteSwitch.IsChecked == true ? 1 : 0,

            PermissaoRelatoriosGenerate = PermissaoRelatoriosGenerateSwitch.IsChecked == true ? 1 : 0
        };

        try
        {
            if (_usuarioId != 0)
            {
                usuario.CodUsuarios = _usuarioId;
                var result = await _usuarioService.UpdateAsync(usuario);
                if (result > 0)
                {
                    await MessageBox.Show(window, "Usuário atualizado com sucesso!", "Sucesso");
                    window.Content = new UsuarioSearch();
                }
            }
            else
            {
                var existingUser = await _usuarioService.GetByUsuarioAsync(usuario.Usuario);
                if (existingUser != null)
                {
                    await MessageBox.Show(window, $"O nome de usuário '{usuario.Usuario}' já está em uso.", "Usuário Existente");
                    UsuarioEntry.Focus();
                    return;
                }

                int newUserId = await _usuarioService.InsertAsync(usuario);
                if (newUserId > 0)
                {
                    await MessageBox.Show(window, "Usuário cadastrado com sucesso!", "Sucesso");
                    window.Content = new UsuarioSearch();
                }
            }
        }
        catch (Exception ex)
        {
            await MessageBox.Show(window, $"Ocorreu um erro ao salvar o usuário: {ex.Message}", "Erro");
        }
    }

    private void CancelarButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is Window window)
        {
            window.Content = new UsuarioSearch();
        }
    }
}