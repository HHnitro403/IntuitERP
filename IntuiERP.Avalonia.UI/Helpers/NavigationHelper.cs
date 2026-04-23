using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;

namespace IntuiERP.Avalonia.UI.Helpers
{
    public static class NavigationHelper
    {
        /// <summary>
        /// Navigates to a new view by replacing the main application content.
        /// Works across desktop and single-view platforms.
        /// </summary>
        public static void NavigateTo(Control newView)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.MainWindow != null)
                {
                    desktop.MainWindow.Content = newView;
                }
            }
            else if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleView)
            {
                singleView.MainView = newView;
            }
        }

        /// <summary>
        /// Gets the current main window if available.
        /// </summary>
        public static Window? GetMainWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }
            return null;
        }

        /// <summary>
        /// Attempts to get a Window reference from a visual element.
        /// Falls back to the main window if VisualRoot is not a Window (e.g., in Designer or TopLevelHost).
        /// </summary>
        public static Window? GetWindow(Visual visual)
        {
            var root = TopLevel.GetTopLevel(visual);
            if (root is Window window)
            {
                return window;
            }

            return GetMainWindow();
        }
    }
}
