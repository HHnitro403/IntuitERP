using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using DBConfigurator.Avalonia.UI.Models;
using DBConfigurator.Avalonia.UI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Dapper;
using Npgsql;

namespace DBConfigurator.Avalonia.UI
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseService _databaseService;
        private readonly AdminCredentialsService _adminCredentialsService;
        private readonly ConnectionTesterService _connectionTesterService;
        private Configuration? _selectedConfig;

        public ICommand LoginCommand { get; }
        public ICommand TestConnectionCommand { get; }
        public ICommand SaveConfigCommand { get; }
        public ICommand ClearFormCommand { get; }
        public ICommand CreateUserCommand { get; }
        public ICommand RefreshUsersCommand { get; }
        public ICommand DeleteConfigCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ToggleThemeCommand { get; }

        public MainWindow()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _adminCredentialsService = new AdminCredentialsService();
            _connectionTesterService = new ConnectionTesterService(_databaseService);

            LoginCommand = new RelayCommand<object>(async (_) => await OnLoginClicked());
            TestConnectionCommand = new RelayCommand<object>(async (_) => await OnTestConnectionClicked());
            SaveConfigCommand = new RelayCommand<object>(async (_) => await OnSaveClicked());
            ClearFormCommand = new RelayCommand<object>((_) => OnClearClicked());
            CreateUserCommand = new RelayCommand<object>(async (_) => await OnCreateUserClicked());
            RefreshUsersCommand = new RelayCommand<object>(async (_) => await OnRefreshUsersClicked());
            ToggleThemeCommand = new RelayCommand<object>((_) => ToggleTheme());

            DeleteConfigCommand = new RelayCommand<Configuration>(async (config) => {
                if (config != null)
                {
                    await _databaseService.DeleteConfigurationAsync(config);
                    await LoadConfigurations();
                }
            });

            DeleteUserCommand = new RelayCommand<UserViewModel>(async (user) => {
                if (user != null)
                {
                    await DeleteUserInternal(user);
                }
            });

            DataContext = this;
        }

        private void ToggleTheme()
        {
            var app = Application.Current;
            if (app != null)
            {
                var currentTheme = app.ActualThemeVariant;
                app.RequestedThemeVariant = currentTheme == ThemeVariant.Dark 
                    ? ThemeVariant.Light 
                    : ThemeVariant.Dark;
            }
        }

        private async Task OnLoginClicked()
        {
            string username = AdminUserEntry.Text ?? "";
            string password = AdminPassEntry.Text ?? "";

            if (_adminCredentialsService.ValidateCredentials(username, password))
            {
                LoginGrid.IsVisible = false;
                MainTabControl.IsVisible = true;
                await LoadConfigurations();
            }
            else
            {
                LoginErrorLabel.Text = "Invalid username or password.";
                LoginErrorLabel.IsVisible = true;
            }
        }

        #region TAB 1: DATABASE CONFIGURATION

        private async Task LoadConfigurations()
        {
            var configs = await _databaseService.GetConfigurationsAsync();
            ConfigsListBox.ItemsSource = configs;
        }

        private Configuration GetConfigFromForm()
        {
            int.TryParse(PortEntry.Text, out int port);
            if (port <= 0) port = 5432;

            return new Configuration
            {
                ID = _selectedConfig?.ID ?? 0,
                Server = ServerEntry.Text ?? "",
                Port = port,
                DataBase = DatabaseEntry.Text ?? "",
                User = UserEntry.Text ?? "",
                Password = PasswordEntry.Text ?? ""
            };
        }

        private async Task OnTestConnectionClicked()
        {
            if (string.IsNullOrWhiteSpace(ServerEntry.Text) || 
                string.IsNullOrWhiteSpace(DatabaseEntry.Text) || 
                string.IsNullOrWhiteSpace(UserEntry.Text))
            {
                if (ConnectionTestResultLabel != null)
                {
                    ConnectionTestResultLabel.Text = "Fill in Server, Database, and User fields.";
                    ConnectionTestResultLabel.Foreground = Brushes.Orange;
                    ConnectionTestResultLabel.IsVisible = true;
                }
                return;
            }

            var config = GetConfigFromForm();
            
            if (ConnectionTestResultLabel != null)
            {
                ConnectionTestResultLabel.Text = "Testing connection...";
                ConnectionTestResultLabel.Foreground = Brushes.Gray;
                ConnectionTestResultLabel.IsVisible = true;
            }

            var result = await _connectionTesterService.TestConnectionAsync(config);

            if (ConnectionTestResultLabel != null)
            {
                ConnectionTestResultLabel.Text = result.Message;
                ConnectionTestResultLabel.Foreground = result.Success ? Brushes.Green : Brushes.Red;
            }
        }

        private async Task OnSaveClicked()
        {
            if (string.IsNullOrWhiteSpace(ServerEntry.Text) || 
                string.IsNullOrWhiteSpace(DatabaseEntry.Text) || 
                string.IsNullOrWhiteSpace(UserEntry.Text))
            {
                return;
            }

            var config = GetConfigFromForm();

            await _databaseService.SaveConfigurationAsync(config);
            await LoadConfigurations();
            OnClearClicked();
        }

        private void OnClearClicked()
        {
            ServerEntry.Text = "";
            PortEntry.Text = "5432";
            DatabaseEntry.Text = "";
            UserEntry.Text = "";
            PasswordEntry.Text = "";
            if (ConnectionTestResultLabel != null) ConnectionTestResultLabel.IsVisible = false;
            ConfigsListBox.SelectedItem = null;
            _selectedConfig = null;
        }

        public void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            _selectedConfig = ConfigsListBox.SelectedItem as Configuration;
            if (_selectedConfig != null)
            {
                ServerEntry.Text = _selectedConfig.Server;
                PortEntry.Text = _selectedConfig.Port.ToString();
                DatabaseEntry.Text = _selectedConfig.DataBase;
                UserEntry.Text = _selectedConfig.User;
                PasswordEntry.Text = _selectedConfig.Password;
                if (ConnectionTestResultLabel != null) ConnectionTestResultLabel.IsVisible = false;
            }
        }

        #endregion

        #region TAB 2: USER MANAGEMENT

        private async Task OnCreateUserClicked()
        {
            string username = ERPUserEntry.Text?.Trim() ?? "";
            string password = ERPPassEntry.Text ?? "";
            int profileIndex = ProfileComboBox.SelectedIndex;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || profileIndex < 0)
            {
                ShowUserStatus("Fill all fields.", Brushes.Orange);
                return;
            }

            try
            {
                var configs = await _databaseService.GetConfigurationsAsync();
                var activeConfig = configs.LastOrDefault();
                if (activeConfig == null)
                {
                    ShowUserStatus("Configure DB first.", Brushes.Red);
                    return;
                }

                string connStr = _databaseService.BuildConnectionString(activeConfig);
                using (var conn = new NpgsqlConnection(connStr))
                {
                    await conn.OpenAsync();
                    
                    // Profile 0: User (Read only), 1: Administrator (Full)
                    int permValue = (profileIndex == 1) ? 1 : 0;
                    int readPerm = 1; // Both have read perm

                    string hash = BCrypt.Net.BCrypt.HashPassword(password, 12);

                    string sql = @"
                        INSERT INTO usuarios (
                            usuario, senha, 
                            permissao_produtos_create, permissao_produtos_read, permissao_produtos_update, permissao_produtos_delete,
                            permissao_vendas_create, permissao_vendas_read, permissao_vendas_update, permissao_vendas_delete,
                            permissao_relatorios_generate,
                            permissao_vendedores_create, permissao_vendedores_read, permissao_vendedores_update, permissao_vendedores_delete,
                            permissao_fornecedores_create, permissao_fornecedores_read, permissao_fornecedores_update, permissao_fornecedores_delete,
                            permissao_clientes_create, permissao_clientes_read, permissao_clientes_update, permissao_clientes_delete
                        ) VALUES (
                            @Usuario, @Senha, 
                            @Val, @Read, @Val, @Val, 
                            @Val, @Read, @Val, @Val, 
                            @Val, 
                            @Val, @Read, @Val, @Val, 
                            @Val, @Read, @Val, @Val, 
                            @Val, @Read, @Val, @Val
                        )";

                    await conn.ExecuteAsync(sql, new { 
                        Usuario = username, 
                        Senha = hash, 
                        Val = permValue, 
                        Read = readPerm 
                    });

                    ShowUserStatus("User created!", Brushes.Green);
                }
                await OnRefreshUsersClicked();
            }
            catch (Exception ex)
            {
                ShowUserStatus($"Error: {ex.Message}", Brushes.Red);
            }
        }

        private async Task OnRefreshUsersClicked()
        {
            try
            {
                var configs = await _databaseService.GetConfigurationsAsync();
                var activeConfig = configs.LastOrDefault();
                if (activeConfig == null) return;

                string connStr = _databaseService.BuildConnectionString(activeConfig);
                using (var conn = new NpgsqlConnection(connStr))
                {
                    await conn.OpenAsync();
                    
                    // Ensure SysAdmin exists
                    await EnsureSysAdminExists(conn);

                    var users = await conn.QueryAsync<UserViewModel>("SELECT cod_usuarios, usuario FROM usuarios");
                    UsersListBox.ItemsSource = users.ToList();
                }
            }
            catch (Exception ex)
            {
                ShowUserStatus($"Refresh error: {ex.Message}", Brushes.Red);
            }
        }

        private async Task EnsureSysAdminExists(NpgsqlConnection conn)
        {
            var exists = await conn.ExecuteScalarAsync<bool>("SELECT EXISTS(SELECT 1 FROM usuarios WHERE usuario = 'sysadmin')");
            if (!exists)
            {
                string hash = BCrypt.Net.BCrypt.HashPassword("SysAdmin123!", 12);
                string sql = @"
                    INSERT INTO usuarios (
                        usuario, senha, 
                        permissao_produtos_create, permissao_produtos_read, permissao_produtos_update, permissao_produtos_delete,
                        permissao_vendas_create, permissao_vendas_read, permissao_vendas_update, permissao_vendas_delete,
                        permissao_relatorios_generate,
                        permissao_vendedores_create, permissao_vendedores_read, permissao_vendedores_update, permissao_vendedores_delete,
                        permissao_fornecedores_create, permissao_fornecedores_read, permissao_fornecedores_update, permissao_fornecedores_delete,
                        permissao_clientes_create, permissao_clientes_read, permissao_clientes_update, permissao_clientes_delete
                    ) VALUES (
                        'sysadmin', @Senha, 
                        1, 1, 1, 1, 
                        1, 1, 1, 1, 
                        1, 
                        1, 1, 1, 1, 
                        1, 1, 1, 1, 
                        1, 1, 1, 1
                    )";
                await conn.ExecuteAsync(sql, new { Senha = hash });
            }
        }

        private async Task DeleteUserInternal(UserViewModel user)
        {
            if (user.usuario == "sysadmin") return;

            try
            {
                var configs = await _databaseService.GetConfigurationsAsync();
                var activeConfig = configs.LastOrDefault();
                if (activeConfig == null) return;

                string connStr = _databaseService.BuildConnectionString(activeConfig);
                using (var conn = new NpgsqlConnection(connStr))
                {
                    await conn.ExecuteAsync("DELETE FROM usuarios WHERE cod_usuarios = @Id", new { Id = user.cod_usuarios });
                }
                await OnRefreshUsersClicked();
            }
            catch (Exception ex)
            {
                ShowUserStatus($"Delete error: {ex.Message}", Brushes.Red);
            }
        }

        private void ShowUserStatus(string msg, IBrush color)
        {
            if (UserActionStatusLabel != null)
            {
                UserActionStatusLabel.Text = msg;
                UserActionStatusLabel.Foreground = color;
                UserActionStatusLabel.IsVisible = true;
            }
        }

        #endregion
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        public RelayCommand(Action<T?> execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute((T?)parameter);
#pragma warning disable 67
        public event EventHandler? CanExecuteChanged;
#pragma warning restore 67
    }

    public class UserViewModel
    {
        public int cod_usuarios { get; set; }
        public string usuario { get; set; } = "";
        public bool IsNotSysAdmin => usuario != "sysadmin";
    }
}
