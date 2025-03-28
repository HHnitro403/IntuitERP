using IntuitERP.Config;
using System.Text.RegularExpressions;

namespace IntuitERP
{
    public partial class MainPage : ContentPage
    {
        // Track validation state
        private bool _isUserValid = false;
        private bool _isPasswordValid = false;


        public MainPage()
        {
            InitializeComponent();

            Configurator config = new Configurator();
        }


        private void OnUserTextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateUser();
            UpdateLoginButtonState();
        }

        private void OnPasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            ValidatePassword();
            UpdateLoginButtonState();
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
            if (password.Length < 4)
            {
                ShowPasswordError("Password must be at least 4 characters");
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
            UserFrame.BorderColor = Colors.Red;
        }

        private void HideUserError()
        {
            UserErrorLabel.IsVisible = false;
            UserFrame.BorderColor = Colors.Gray;
        }

        private void ShowPasswordError(string message)
        {
            PasswordErrorLabel.Text = message;
            PasswordErrorLabel.IsVisible = true;
            PasswordFrame.BorderColor = Colors.Red;
        }

        private void HidePasswordError()
        {
            PasswordErrorLabel.IsVisible = false;
            PasswordFrame.BorderColor = Colors.Gray;
        }

        private void UpdateLoginButtonState()
        {
            LoginButton.IsEnabled = _isUserValid && _isPasswordValid;
        }

        private void OnLoginButtonClicked(object sender, EventArgs e)
        {
            // Here you would typically add the actual login logic
            // Since we're not implementing that, we'll just display a confirmation message

            DisplayAlert("Login Attempted",
                $"Username: {UserEntry.Text}\nPassword: {PasswordEntry.Text}\nRemember Me: {RememberMeCheckbox.IsChecked}",
                "OK");
        }


    }

}
