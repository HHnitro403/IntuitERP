using System.Linq;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using IntuitERP.Services;
using IntuitERP.Config;
using System;
using System.Data;
using System.Threading.Tasks;

namespace IntuitERP.Desktop.Views
{
    public partial class LoginView : UserControl
    {
        private TextBox UserEntry => this.FindControl<TextBox>("UserEntry")!;
        private TextBox PasswordEntry => this.FindControl<TextBox>("PasswordEntry")!;
        private TextBlock UserErrorLabel => this.FindControl<TextBlock>("UserErrorLabel")!;
        private Button LoginButton => this.FindControl<Button>("LoginButton")!;

        private UsuarioService? _usuarioService;
        private readonly UserContext _userContext;

        public event EventHandler? OnLoginSuccess;

        public LoginView()
        {
            InitializeComponent();
            _userContext = UserContext.Instance;
            
            LoginButton.Click += LoginButton_Click;

            // Initialize services
            InitializeServices();
        }

        private void InitializeComponent()
        {
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }

        private void InitializeServices()
        {
            try
            {
                var configurator = new Configurator();
                IDbConnection connection = configurator.GetMySqlConnection();
                _usuarioService = new UsuarioService(connection);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Configuration Error: {ex.Message}");
                // In a real app, we'd show a message box here
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (_usuarioService == null) return;

            try
            {
                var usuario = await _usuarioService.AuthenticateAsync(UserEntry.Text ?? "", PasswordEntry.Text ?? "");
                if (usuario != null)
                {
                    _userContext.SetCurrentUser(usuario);
                    OnLoginSuccess?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    UserErrorLabel.Text = "Usuário ou senha inválidos";
                    UserErrorLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
            }
        }
    }
}

