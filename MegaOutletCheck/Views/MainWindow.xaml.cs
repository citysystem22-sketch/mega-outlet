using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using MegaOutletCheck.Models;
using MegaOutletCheck.Services;
using MegaOutletCheck.ViewModels;

namespace MegaOutletCheck.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load settings
                var settings = AppSettings.Load();
                
                // Initialize services
                var wooService = new WooCommerceService(settings);
                var cacheService = new ProductCacheService(settings);
                
                // Load cache
                await cacheService.LoadAsync();
                
                // Initialize view model
                _viewModel = new MainViewModel(settings, wooService, cacheService);
                DataContext = _viewModel;
                
                // Wire up dark mode change
                _viewModel.DarkModeChanged += OnDarkModeChanged;
                
                // Apply initial theme
                if (settings.IsDarkMode)
                {
                    ApplyTheme(true);
                }
                
                // Auto-focus search box
                SearchBox.Focus();
                
                // Show settings if not configured
                if (!settings.IsConfigured)
                {
                    _viewModel.OpenSettingsCommand.Execute(null);
                }
                
                App.Log("Main window loaded successfully");
            }
            catch (Exception ex)
            {
                App.LogError("Failed to load main window", ex);
                MessageBox.Show(
                    $"Błąd inicjalizacji: {ex.Message}",
                    "Błąd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Adjust grid columns based on window width
            // Could be used for responsive layout
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            // Handle fullscreen / kiosk mode
            if (WindowState == WindowState.Maximized)
            {
                // Enable fullscreen mode for kiosk
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
            }
            else
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanResize;
            }
        }

        private void OnDarkModeChanged(object? sender, bool isDarkMode)
        {
            ApplyTheme(isDarkMode);
        }

        private void ApplyTheme(bool isDarkMode)
        {
            var app = Application.Current;
            var resources = app.Resources;
            
            if (isDarkMode)
            {
                // Dark colors
                resources["BackgroundBrush"] = System.Windows.Media.Brushes.Black;
                resources["SurfaceBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30));
                resources["TextPrimaryBrush"] = System.Windows.Media.Brushes.White;
                resources["TextSecondaryBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(180, 180, 180));
                resources["PrimaryBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(96, 165, 250)); // Light blue
                resources["ErrorBrush"] = System.Windows.Media.Brushes.OrangeRed;
                resources["SuccessBrush"] = System.Windows.Media.Brushes.LightGreen;
                resources["InStockBrush"] = System.Windows.Media.Brushes.LightGreen;
                resources["OutOfStockBrush"] = System.Windows.Media.Brushes.Red;
                resources["LowStockBrush"] = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                // Light colors
                resources["BackgroundBrush"] = System.Windows.Media.Brushes.White;
                resources["SurfaceBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 250, 252));
                resources["TextPrimaryBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 41, 59));
                resources["TextSecondaryBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 116, 139));
                resources["PrimaryBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(37, 99, 235)); // Blue
                resources["ErrorBrush"] = System.Windows.Media.Brushes.Red;
                resources["SuccessBrush"] = System.Windows.Media.Brushes.Green;
                resources["InStockBrush"] = System.Windows.Media.Brushes.Green;
                resources["OutOfStockBrush"] = System.Windows.Media.Brushes.Red;
                resources["LowStockBrush"] = System.Windows.Media.Brushes.Orange;
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Show touch keyboard on focus (Windows touch keyboard)
            ShowTouchKeyboard();
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle Enter key
            if (e.Key == Key.Escape)
            {
                _viewModel?.ClearSearchCommand.Execute(null);
            }
        }

        private void ProductCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is Product product)
            {
                _viewModel?.SelectProductCommand.Execute(product);
            }
        }

        private void SettingsOverlay_Click(object sender, MouseButtonEventArgs e)
        {
            // Close settings when clicking outside
            _viewModel?.CloseSettingsCommand.Execute(null);
        }

        private void SettingsDialog_Click(object sender, MouseButtonEventArgs e)
        {
            // Prevent closing when clicking inside dialog
            e.Handled = true;
        }

        private void ApiSecretBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Update API secret in view model
            if (_viewModel != null)
            {
                _viewModel.SettingsApiSecret = ApiSecretBox.Password;
            }
        }

        private void ShowTouchKeyboard()
        {
            try
            {
                // Try to show Windows touch keyboard
                var taskbar = Type.GetTypeFromCLSID(new Guid("56FDF344-FD6D-11d0-958A-006097C9A090"));
                if (taskbar != null)
                {
                    dynamic? taskbarInstance = Activator.CreateInstance(taskbar);
                    if (taskbarInstance != null)
                    {
                        taskbarInstance.ToggleOfk();
                    }
                }
            }
            catch
            {
                // Ignore errors showing touch keyboard
            }
        }

        // P/Invoke for showing touch keyboard
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const byte VK_TAB = 0x09;
        private const uint KEYEVENTF_KEYDOWN = 0x0000;
    }
}