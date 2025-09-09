using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;

namespace TaskLogger.Services
{
    public class SystemTrayService : ISystemTrayService
    {
        private TaskbarIcon? _taskbarIcon;
        private bool _isVisible = true;

        public bool IsVisible => _isVisible;
        public event EventHandler? ShowRequested;
        public event EventHandler? ExitRequested;

        public void Initialize()
        {
            try
            {
                _taskbarIcon = new TaskbarIcon();
                _taskbarIcon.Icon = GetApplicationIcon();
                _taskbarIcon.ToolTipText = "Task Logger - Click to show/hide";
                _taskbarIcon.TrayMouseDoubleClick += OnTrayLeftClick;

                // Create context menu
                _taskbarIcon.ContextMenu = CreateContextMenu();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing system tray: {ex.Message}");
            }
        }

        public void ShowBalloonTip(string title, string message, BalloonIcon icon = BalloonIcon.Info)
        {
            try
            {
                var balloonIcon = icon switch
                {
                    BalloonIcon.Warning => Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning,
                    BalloonIcon.Error => Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error,
                    _ => Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info
                };

                _taskbarIcon?.ShowBalloonTip(title, message, balloonIcon);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing balloon tip: {ex.Message}");
            }
        }

        public void HideToTray()
        {
            _isVisible = false;
            _taskbarIcon?.ShowBalloonTip("Task Logger", "Minimized to system tray", 
                                       Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
        }

        public void ShowFromTray()
        {
            _isVisible = true;
        }

        public void ExitApplication()
        {
            ExitRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnTrayLeftClick(object sender, RoutedEventArgs e)
        {
            ShowRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnTrayRightClick(object sender, RoutedEventArgs e)
        {
            // Context menu will be shown automatically
        }

        private ContextMenu CreateContextMenu()
        {
            var contextMenu = new ContextMenu();

            var showItem = new MenuItem
            {
                Header = "Show Task Logger",
                Icon = new System.Windows.Controls.TextBlock { Text = "📝", FontSize = 12 }
            };
            showItem.Click += (s, e) => ShowRequested?.Invoke(this, EventArgs.Empty);

            var separator1 = new Separator();

            var exitItem = new MenuItem
            {
                Header = "Exit Completely",
                Icon = new System.Windows.Controls.TextBlock { Text = "❌", FontSize = 12 }
            };
            exitItem.Click += (s, e) => 
            {
                var result = MessageBox.Show(
                    "Are you sure you want to exit Task Logger completely?\n\nThis will stop the background service.",
                    "Exit Task Logger",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                    
                if (result == MessageBoxResult.Yes)
                {
                    ExitRequested?.Invoke(this, EventArgs.Empty);
                }
            };

            contextMenu.Items.Add(showItem);
            contextMenu.Items.Add(separator1);
            contextMenu.Items.Add(exitItem);

            return contextMenu;
        }

        private Icon GetApplicationIcon()
        {
            try
            {
                // Try to get the application icon
                var assembly = Assembly.GetExecutingAssembly();
                var iconStream = assembly.GetManifestResourceStream("TaskLogger.Resources.icon.ico");
                
                if (iconStream != null)
                {
                    return new Icon(iconStream);
                }
            }
            catch
            {
                // Fallback to default icon
            }

            // Create a simple default icon
            return SystemIcons.Application;
        }

        public void Dispose()
        {
            _taskbarIcon?.Dispose();
        }
    }
}
