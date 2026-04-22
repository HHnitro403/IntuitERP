using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Avalonia.Controls;
using IntuitERP.Desktop.Views;
using System;

namespace IntuitERP.Desktop
{
    public partial class MainWindow : Window
    {
        private ContentControl MainContentControl => this.FindControl<ContentControl>("MainContentControl")!;

        public MainWindow()
        {
            InitializeComponent();
            ShowLogin();
        }

        private void InitializeComponent()
        {
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }

        private void ShowLogin()
        {
            var loginView = new LoginView();
            loginView.OnLoginSuccess += (s, e) => ShowMainMenu();
            MainContentControl.Content = loginView;
        }

        private void ShowMainMenu()
        {
            MainContentControl.Content = new MainMenuView();
        }
    }
}

