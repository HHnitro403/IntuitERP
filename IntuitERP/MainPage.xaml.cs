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
        // Track validation state
        private bool _isUserValid = false;

        private bool _isPasswordValid = false;
        private IDbConnection _connection;
        private UsuarioService _usuarioService;

        public MainPage()
        {
            InitializeComponent();

            // Force light theme regardless of system settings
            Application.Current.UserAppTheme = AppTheme.Light;

            // Force dark theme
            //Application.Current.UserAppTheme = AppTheme.Dark;

            var configurator = new Configurator();
            _connection = configurator.GetMySqlConnection();

            // Initialize services that need the connection
            _usuarioService = new UsuarioService(_connection);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UserEntry.Text = string.Empty;
            PasswordEntry.Text = string.Empty;

            // Only initialize if it hasn't been done already
            if (_usuarioService == null)
            {
                // Get the existing connection from Configurator
                var configurator = new Configurator();
                _connection = configurator.GetMySqlConnection();

                // Initialize services that need the connection
                _usuarioService = new UsuarioService(_connection);
            }
        }

        private void OnUserTextChanged(object sender, TextChangedEventArgs e)
        {
            if (UserEntry.IsFocused == true)
            {
                ValidateUser();
                UpdateLoginButtonState();
            }
        }

        private void OnPasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            if (PasswordEntry.IsFocused == true)
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
                return;
            }

            if (username.Length < 3)
            {
                ShowUserError("Username must be at least 3 characters");
                _isUserValid = false;
                return;
            }

            // Only allow alphanumeric characters and underscores
            var usernamePattern = @"^[a-zA-Z0-9_]+$";
            if (!Regex.IsMatch(username, usernamePattern))
            {
                ShowUserError("Username can only contain letters, numbers, and underscores");
                _isUserValid = false;
                return;
            }

            // Username is valid
            HideUserError();
            _isUserValid = true;
        }

        private void ValidatePassword()
        {
            string password = PasswordEntry.Text ?? string.Empty;

            if (string.IsNullOrEmpty(password))
            {
                ShowPasswordError("Password is required");
                _isPasswordValid = false;
                return;
            }

            // Only check for minimum length of 4 characters
            if (password.Length < 6)
            {
                ShowPasswordError("Password must be at least  characters");
                _isPasswordValid = false;
                return;
            }

            // Password is valid
            HidePasswordError();
            _isPasswordValid = true;
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

        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
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
                    // Authentication successful
                    return true;
                }
                else
                {
                    // User not found or invalid credentials
                   throw new Exception("Usuario ou Senha Invalidos.");
                    
                }
            }
            catch (Exception ex)
            {
                throw;              
            }
        }
    }
}