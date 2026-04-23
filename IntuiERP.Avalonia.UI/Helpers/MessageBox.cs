using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.Helpers
{
    public enum MessageBoxButton
    {
        Ok,
        YesNo
    }

    public static class MessageBox
    {
        public static async Task Show(Window owner, string message, string title, MessageBoxButton button = MessageBoxButton.Ok)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Thickness(20),
                    Spacing = 10,
                    Children =
                    {
                        new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
                        new Button 
                        { 
                            Content = "OK", 
                            HorizontalAlignment = HorizontalAlignment.Right,
                            // Proper way to handle click in code-behind for Avalonia
                        }
                    }
                }
            };

            var okButton = (Button)((StackPanel)dialog.Content).Children[1];
            okButton.Click += (s, e) => dialog.Close();

            await dialog.ShowDialog(owner);
        }
    }
}
