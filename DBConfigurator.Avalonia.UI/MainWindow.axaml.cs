using Avalonia.Controls;
using Avalonia.Interactivity;
using DBConfigurator.Avalonia.UI.Models;
using DBConfigurator.Avalonia.UI.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DBConfigurator.Avalonia.UI
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseService _databaseService;
        private readonly AdminCredentialsService _adminCredentialsService;
        private Configuration? _selectedConfig;

        public MainWindow()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _adminCredentialsService = new AdminCredentialsService();
        }

        private async void OnLoginClicked(object? sender, RoutedEventArgs e)
        {
            string username = AdminUserEntry.Text ?? "";
            string password = AdminPassEntry.Text ?? "";

            if (_adminCredentialsService.ValidateCredentials(username, password))
            {
                LoginGrid.IsVisible = false;
                MainGrid.IsVisible = true;
                await LoadConfigurations();
            }
            else
            {
                LoginErrorLabel.Text = "Invalid username or password.";
                LoginErrorLabel.IsVisible = true;
            }
        }

        private async Task LoadConfigurations()
        {
            var configs = await _databaseService.GetConfigurationsAsync();
            ConfigsListBox.ItemsSource = configs;
        }

        private async void OnSaveClicked(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ServerEntry.Text) || 
                string.IsNullOrWhiteSpace(DatabaseEntry.Text) || 
                string.IsNullOrWhiteSpace(UserEntry.Text))
            {
                return;
            }

            var config = new Configuration
            {
                ID = _selectedConfig?.ID ?? 0,
                Server = ServerEntry.Text,
                DataBase = DatabaseEntry.Text,
                User = UserEntry.Text,
                Password = PasswordEntry.Text ?? ""
            };

            await _databaseService.SaveConfigurationAsync(config);
            await LoadConfigurations();
            OnClearClicked(null, null);
        }

        private async void OnDeleteClicked(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Configuration config)
            {
                await _databaseService.DeleteConfigurationAsync(config);
                await LoadConfigurations();
            }
        }

        private void OnClearClicked(object? sender, RoutedEventArgs? e)
        {
            ServerEntry.Text = "";
            DatabaseEntry.Text = "";
            UserEntry.Text = "";
            PasswordEntry.Text = "";
            ConfigsListBox.SelectedItem = null;
            _selectedConfig = null;
        }

        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            _selectedConfig = ConfigsListBox.SelectedItem as Configuration;
            if (_selectedConfig != null)
            {
                ServerEntry.Text = _selectedConfig.Server;
                DatabaseEntry.Text = _selectedConfig.DataBase;
                UserEntry.Text = _selectedConfig.User;
                PasswordEntry.Text = _selectedConfig.Password;
            }
        }
    }
}
