using DBconfigurator.Models;
using DBconfigurator.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DBconfigurator
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private bool _isLoggedIn = false;
        private string _server;
        private string _dataBase;
        private string _user;
        private string _password;

        public ObservableCollection<Configuration> Configurations { get; } = new();

        public string Server { get => _server; set => SetProperty(ref _server, value); }
        public string DataBase { get => _dataBase; set => SetProperty(ref _dataBase, value); }
        public string User { get => _user; set => SetProperty(ref _user, value); }
        public string Password { get => _password; set => SetProperty(ref _password, value); }

        public MainPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            BindingContext = this;
            // Hide the main content until the user is logged in
            this.Content.IsVisible = false;
        }

        private void OnPageLoaded(object sender, EventArgs e)
        {
            // Use the Loaded event to ensure the page is ready for pop-ups
            if (!_isLoggedIn)
            {
                // Dispatch the login prompt to the UI thread to avoid timing issues on Windows.
                Dispatcher.Dispatch(async () =>
                {
                    // A small delay can help ensure the window is fully ready.
                    await Task.Delay(100);
                    await PerformLogin();
                });
            }
        }

        private async Task PerformLogin()
        {
            while (!_isLoggedIn)
            {
                string enteredUser = await DisplayPromptAsync("Login Required", "Please enter your username:", "OK", "Cancel", initialValue: "", keyboard: Keyboard.Default);

                // Handle user cancelling the prompt
                if (enteredUser == null)
                {
                    await HandleLoginCancellation();
                    return;
                }

                string enteredPassword = await DisplayPromptAsync("Login Required", "Please enter your password:", "OK", "Cancel", placeholder: "Password", initialValue: "");

                // Handle user cancelling the prompt
                if (enteredPassword == null)
                {
                    await HandleLoginCancellation();
                    return;
                }

                // --- IMPORTANT: This is for demonstration only. ---
                // In a real-world application, you should use a secure method 
                // for credential validation (e.g., hashing, authentication service).
                if (enteredUser == "admin" && enteredPassword == "password")
                {
                    _isLoggedIn = true;
                    this.Content.IsVisible = true; // Show the main UI
                    await LoadConfigurations();
                }
                else
                {
                    bool tryAgain = await DisplayAlert("Login Failed", "Invalid username or password.", "Try Again", "Cancel");
                    if (!tryAgain)
                    {
                        await HandleLoginCancellation();
                        return;
                    }
                }
            }
        }

        private async Task HandleLoginCancellation()
        {
            await DisplayAlert("Login Required", "Access denied. The application will close.", "OK");
            Application.Current.Quit();
        }

        private async Task LoadConfigurations()
        {
            var configs = await _databaseService.GetConfigurationsAsync();
            Configurations.Clear();
            foreach (var config in configs)
            {
                Configurations.Add(config);
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Server) || string.IsNullOrWhiteSpace(DataBase) || string.IsNullOrWhiteSpace(User))
            {
                await DisplayAlert("Error", "Server, DataBase, and User fields cannot be empty.", "OK");
                return;
            }

            Configuration selectedConfig = ConfigsCollectionView.SelectedItem as Configuration;

            var newConfig = new Configuration
            {
                ID = selectedConfig?.ID ?? 0, // Use existing ID if updating, otherwise 0 for new
                Server = this.Server,
                DataBase = this.DataBase,
                User = this.User,
                Password = this.Password ?? string.Empty
            };

            await _databaseService.SaveConfigurationAsync(newConfig);
            await LoadConfigurations();
            OnClearClicked(this, EventArgs.Empty); // Clear form after saving
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Configuration config)
            {
                bool confirmed = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the configuration for '{config.Server}'?", "Yes", "No");
                if (confirmed)
                {
                    await _databaseService.DeleteConfigurationAsync(config);
                    await LoadConfigurations();
                }
            }
        }

        private void OnClearClicked(object sender, EventArgs e)
        {
            Server = string.Empty;
            DataBase = string.Empty;
            User = string.Empty;
            Password = string.Empty;
            ConfigsCollectionView.SelectedItem = null;
        }

        private void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedConfig = e.CurrentSelection.FirstOrDefault() as Configuration;
            if (selectedConfig != null)
            {
                Server = selectedConfig.Server;
                DataBase = selectedConfig.DataBase;
                User = selectedConfig.User;
                Password = selectedConfig.Password;
            }
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }

}
