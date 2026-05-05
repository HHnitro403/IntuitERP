using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Dapper;
using IntuiERP.Avalonia.UI.Config;
using IntuiERP.Avalonia.UI.Helpers;
using IntuiERP.Avalonia.UI.models;
using IntuiERP.Avalonia.UI.Services;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI
{
    public class DatabaseOption
    {
        public int Id { get; set; }
        public string Server { get; set; } = string.Empty;
        public int Port { get; set; } = 5432;
        public string DataBase { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string DisplayName => DataBase;
    }

    public partial class MainWindow : Window
    {
        private bool _isUserValid = false;
        private bool _isPasswordValid = false;
        private UsuarioService? _usuarioService;
        private readonly UserContext _userContext;
        private bool _isLoggingIn = false;
        private List<DatabaseOption> _databaseOptions = new();
        private DatabaseOption? _selectedDatabase;

        public MainWindow()
        {
            InitializeComponent();
            _userContext = UserContext.Instance;

            LoginButton.Click += LoginButton_Clicked;
            UserEntry.TextChanged += OnUserTextChanged;
            PasswordEntry.TextChanged += OnPasswordTextChanged;
            LoginButton.Click += LoginButton_Clicked;
            DatabaseComboBox.SelectionChanged += DatabaseComboBox_SelectionChanged;

            var toggleThemeButton = this.FindControl<Button>("ToggleThemeButton");
            if (toggleThemeButton != null)
            {
                toggleThemeButton.Click += (s, e) => ToggleTheme();
            }

            this.Opened += MainWindow_Opened;
        }



        private void ToggleTheme()
        {
            var app = Application.Current;
            if (app != null)
            {
                var currentTheme = app.ActualThemeVariant;
                app.RequestedThemeVariant = currentTheme == global::Avalonia.Styling.ThemeVariant.Dark
                    ? global::Avalonia.Styling.ThemeVariant.Light
                    : global::Avalonia.Styling.ThemeVariant.Dark;
            }
        }

        private async void MainWindow_Opened(object? sender, EventArgs e)
        {
            this.Opened -= MainWindow_Opened;

            UserEntry.Text = string.Empty;
            PasswordEntry.Text = string.Empty;
            LoginButton.IsEnabled = false;

            await LoadDatabaseOptionsAsync();
            await InitializeServicesAsync();
        }

        private async Task InitializeServicesAsync()
        {
            try
            {
                // Use the factory for better lifecycle management
                var factory = new NpgsqlConnectionFactory();

                // Optional: Verify connection string exists
                var configurator = new Configurator();

                _usuarioService = new UsuarioService(factory);
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
            if (_isLoggingIn) return;

            if (_usuarioService == null)
            {
                await MessageBox.Show(this, "The application is not connected to the database. Please restart.", "Error");
                return;
            }

            try
            {
                SetLoginState(true);

                var result = await LoginAsync();
                if (result)
                {
                    // Swap the content of the current window to the MenuPage
                    this.Content = new Views.MenuPage();
                }
            }
            catch (Exception ex)
            {
                await MessageBox.Show(this, ex.Message, "Login Failed");
                Console.WriteLine($"Login error: {ex.Message}");
                SetLoginState(false);
            }
        }

        private void SetLoginState(bool isLoggingIn)
        {
            _isLoggingIn = isLoggingIn;
            LoginButton.IsEnabled = !isLoggingIn && _isUserValid && _isPasswordValid;
            UserEntry.IsEnabled = !isLoggingIn;
            PasswordEntry.IsEnabled = !isLoggingIn;

            if (isLoggingIn)
                LoginButton.Content = "ENTRANDO...";
            else
                LoginButton.Content = "LOGIN";
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
            else if (password.Length < 5)
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
            UserFrame.BorderBrush = GetResourceBrush("BorderBrushColor") ?? Brushes.Gray;
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
            PasswordFrame.BorderBrush = GetResourceBrush("BorderBrushColor") ?? Brushes.Gray;
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
            if (!_isLoggingIn)
                LoginButton.IsEnabled = _isUserValid && _isPasswordValid && _selectedDatabase != null;
        }
        private async Task LoadDatabaseOptionsAsync()
        {
            try
            {
                // Shell only: implement SQLite read from ConfigsDB.db here.
                _databaseOptions = new List<DatabaseOption>();
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string folderPath = Path.Combine(appDataPath, "IntuitERP", "Config");
                string databasepath = Path.Combine(folderPath, "ConfigsDB.db");
                var sqliteconn = new SqliteConnection($"Data Source={databasepath}");
                var rcon = sqliteconn.OpenAsync();
                const string sql = @"
                    SELECT ID, Server, Port, DataBase, User
                    FROM Connection
                    ORDER BY ID DESC;";

                var rows = await sqliteconn.QueryAsync<DatabaseOption>(sql);
                _databaseOptions = rows.AsList();
                DatabaseComboBox.ItemsSource = _databaseOptions;

            }
            catch (Exception Ex)
            {

            }



            DatabaseComboBox.ItemsSource = _databaseOptions;

            if (_databaseOptions.Count > 0)
            {
                DatabaseComboBox.SelectedIndex = 0;
                _selectedDatabase = _databaseOptions[0];
            }
            else
            {
                _selectedDatabase = null;
            }

            await Task.CompletedTask;
        }



        private void DatabaseComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            _selectedDatabase = DatabaseComboBox.SelectedItem as DatabaseOption;
            UpdateLoginButtonState();
        }
    }
}
