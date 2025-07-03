using IntuitERP.Config;
using IntuitERP.Services;
using IntuitERP.Viwes;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IntuitERP
{
    public partial class MainPage : ContentPage
    {
        private bool _isUserValid = false;
        private bool _isPasswordValid = false;
        private UsuarioService _usuarioService;

        public MainPage()
        {
            InitializeComponent();
        }

        // The logic is now in the Loaded event handler
        private async void MainPage_Loaded(object sender, EventArgs e)
        {
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
                IDbConnection connection = configurator.GetMySqlConnection();
                _usuarioService = new UsuarioService(connection);

                LoginGrid.IsEnabled = true;
            }
            catch (Exception ex)
            {
                // This will now fire reliably after the page has loaded
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Configuration Error", ex.Message, "OK");
                    LoginGrid.IsEnabled = false;
                    await DisplayAlert("Configuration Error", "Exiting Aplication", "OK");
                    App.Current.Quit();
                });
            }
        }

        // ... (The rest of your methods remain the same)

        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
            if (_usuarioService == null)
            {
                await DisplayAlert("Error", "The application is not connected to the database. Please restart.", "OK");
                return;
            }

            try
            {
                var result = await LoginAsync();
                if (result)
                {
                    await Navigation.PushAsync(new MaenuPage());
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Login Failed", ex.Message, "OK");
                Console.WriteLine($"Login error: {ex.Message}");
            }
        }

        public async Task<bool> LoginAsync()
        {
            try
            {
                var usuario = await _usuarioService.AuthenticateAsync(UserEntry.Text, PasswordEntry.Text);
                if (usuario != null)
                {
                    return true;
                }
                else
                {
                    throw new Exception("Invalid username or password.");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void OnUserTextChanged(object sender, TextChangedEventArgs e)
        {
            if (UserEntry.IsFocused)
            {
                ValidateUser();
                UpdateLoginButtonState();
            }
        }

        private void OnPasswordTextChanged(object sender, TextChangedEventArgs e)
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
            else if (password.Length < 6)
            {
                ShowPasswordError("Password must be at least 6 characters");
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
            UserFrame.Stroke = Colors.Red;
        }

        private void HideUserError()
        {
            UserErrorLabel.IsVisible = false;
            UserFrame.Stroke = Colors.Gray;
        }

        private void ShowPasswordError(string message)
        {
            PasswordErrorLabel.Text = message;
            PasswordErrorLabel.IsVisible = true;
            PasswordFrame.Stroke = Colors.Red;
        }

        private void HidePasswordError()
        {
            PasswordErrorLabel.IsVisible = false;
            PasswordFrame.Stroke = Colors.Gray;
        }

        private void UpdateLoginButtonState()
        {
            LoginButton.IsEnabled = _isUserValid && _isPasswordValid;
        }
    }
}