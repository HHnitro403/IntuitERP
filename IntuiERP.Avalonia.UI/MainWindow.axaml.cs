using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using IntuiERP.Avalonia.UI.Services;
using IntuiERP.Avalonia.UI.models;
using IntuiERP.Avalonia.UI.Helpers;
using IntuiERP.Avalonia.UI.Config;

namespace IntuiERP.Avalonia.UI
{
    public partial class MainWindow : Window
    {
        private bool _isUserValid = false;
        private bool _isPasswordValid = false;
        private UsuarioService? _usuarioService;
        private readonly UserContext _userContext;

        public MainWindow()
        {
            InitializeComponent();
            _userContext = UserContext.Instance;
            
            UserEntry.TextChanged += OnUserTextChanged;
            PasswordEntry.TextChanged += OnPasswordTextChanged;
            LoginButton.Click += LoginButton_Clicked;
            
            this.Opened += MainWindow_Opened;
        }

        private async void MainWindow_Opened(object? sender, EventArgs e)
        {
            this.Opened -= MainWindow_Opened; 
            
            UserEntry.Text = string.Empty;
            PasswordEntry.Text = string.Empty;
            LoginButton.IsEnabled = false;

            await InitializeServicesAsync();
        }

        private async Task InitializeServicesAsync()
        {
            try
            {
                var configurator = new Configurator();
                IDbConnection connection = configurator.GetNpgsqlConnection();
                
                // Test the connection immediately
                try 
                {
                    connection.Open();
                    connection.Close();
                }
                catch (Exception connEx)
                {
                    await MessageBox.Show(this, $"Database connection failed: {connEx.Message}", "Connection Error");
                }

                _usuarioService = new UsuarioService(connection);
                LoginGrid.IsEnabled = true;
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await MessageBox.Show(this, ex.Message, "Configuration Error");
                    LoginGrid.IsEnabled = false;
                });
            }
        }

        private async void LoginButton_Clicked(object? sender, RoutedEventArgs e)
        {
            if (_usuarioService == null)
            {
                await MessageBox.Show(this, "The application is not connected to the database. Please restart.", "Error");
                return;
            }

            try
            {
                var result = await LoginAsync();
                if (result)
                {
                    var menuWindow = new MaenuWindow(); 
                    menuWindow.Show();
                    this.Close(); 
                }
            }
            catch (Exception ex)
            {
                await MessageBox.Show(this, ex.Message, "Login Failed");
                Console.WriteLine($"Login error: {ex.Message}");
            }
        }

        public async Task<bool> LoginAsync()
        {
            var user = await _usuarioService!.AuthenticateAsync(UserEntry.Text ?? string.Empty, PasswordEntry.Text ?? string.Empty);
            if (user != null)
            {
                _userContext.SetCurrentUser(user);
                return true;
            }
            throw new Exception("Invalid username or password.");
        }

        private void OnUserTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (UserEntry.IsFocused)
            {
                ValidateUser();
                UpdateLoginButtonState();
            }
        }

        private void OnPasswordTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (PasswordEntry.IsFocused)
            {
                ValidatePassword();
                UpdateLoginButtonState();
            }
        }

        private void ValidateUser()
        {
            string username = UserEntry.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(username))
            {
                ShowUserError("Username is required");
                _isUserValid = false;
            }
            else if (username.Length < 3)
            {
                ShowUserError("Username must be at least 3 characters");
                _isUserValid = false;
            }
            else if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            {
                ShowUserError("Username can only contain letters, numbers, and underscores");
                _isUserValid = false;
            }
            else
            {
                HideUserError();
                _isUserValid = true;
            }
        }

        private void ValidatePassword()
        {
            string password = PasswordEntry.Text ?? string.Empty;
            if (string.IsNullOrEmpty(password))
            {
                ShowPasswordError("Password is required");
                _isPasswordValid = false;
            }
            else if (password.Length < 5) // Reduced to 5 for initial tests if needed
            {
                ShowPasswordError("Password must be at least 5 characters");
                _isPasswordValid = false;
            }
            else
            {
                HidePasswordError();
                _isPasswordValid = true;
            }
        }

        private void ShowUserError(string message)
        {
            UserErrorLabel.Text = message;
            UserErrorLabel.IsVisible = true;
            UserFrame.BorderBrush = Brushes.Red;
        }

        private void HideUserError()
        {
            UserErrorLabel.IsVisible = false;
            UserFrame.BorderBrush = GetResourceBrush("BorderBrush") ?? Brushes.Gray;
        }

        private void ShowPasswordError(string message)
        {
            PasswordErrorLabel.Text = message;
            PasswordErrorLabel.IsVisible = true;
            PasswordFrame.BorderBrush = Brushes.Red;
        }

        private void HidePasswordError()
        {
            PasswordErrorLabel.IsVisible = false;
            PasswordFrame.BorderBrush = GetResourceBrush("BorderBrush") ?? Brushes.Gray;
        }

        private IBrush? GetResourceBrush(string resourceKey)
        {
            if (Application.Current != null && Application.Current.TryFindResource(resourceKey, out var resource) && resource is IBrush brush)
            {
                return brush;
            }
            return null;
        }

        private void UpdateLoginButtonState()
        {
            LoginButton.IsEnabled = _isUserValid && _isPasswordValid;
        }
    }
}
