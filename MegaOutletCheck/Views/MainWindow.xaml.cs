using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
                // Dark mode colors
                resources["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(15, 15, 20));
                resources["SurfaceBrush"] = new SolidColorBrush(Color.FromRgb(30, 30, 40));
                resources["TextPrimaryBrush"] = new SolidColorBrush(Color.FromRgb(240, 240, 245));
                resources["TextSecondaryBrush"] = new SolidColorBrush(Color.FromRgb(140, 145, 155));
                resources["PrimaryBrush"] = new SolidColorBrush(Color.FromRgb(96, 165, 250)); // Light blue
                resources["ErrorBrush"] = new SolidColorBrush(Color.FromRgb(248, 113, 113)); // Light red
                resources["SuccessBrush"] = new SolidColorBrush(Color.FromRgb(74, 222, 128)); // Light green
                resources["InStockBrush"] = new SolidColorBrush(Color.FromRgb(34, 197, 94)); // Green
                resources["OutOfStockBrush"] = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
                resources["LowStockBrush"] = new SolidColorBrush(Color.FromRgb(251, 191, 36)); // Yellow
                resources["BorderBrush"] = new SolidColorBrush(Color.FromRgb(60, 60, 70)); // Dark border
                
                // Update window background directly
                if (this.Background != null)
                {
                    this.Background = new SolidColorBrush(Color.FromRgb(15, 15, 20));
                }
            }
            else
            {
                // Light mode colors
                resources["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(248, 250, 252));
                resources["SurfaceBrush"] = new SolidColorBrush(Colors.White);
                resources["TextPrimaryBrush"] = new SolidColorBrush(Color.FromRgb(15, 23, 42));
                resources["TextSecondaryBrush"] = new SolidColorBrush(Color.FromRgb(100, 116, 139));
                resources["PrimaryBrush"] = new SolidColorBrush(Color.FromRgb(37, 99, 235)); // Blue
                resources["ErrorBrush"] = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
                resources["SuccessBrush"] = new SolidColorBrush(Color.FromRgb(16, 185, 129)); // Green
                resources["InStockBrush"] = new SolidColorBrush(Color.FromRgb(34, 197, 94)); // Green
                resources["OutOfStockBrush"] = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
                resources["LowStockBrush"] = new SolidColorBrush(Color.FromRgb(245, 158, 11)); // Yellow
                resources["BorderBrush"] = new SolidColorBrush(Color.FromRgb(226, 232, 240)); // Light border
                
                // Update window background directly
                if (this.Background != null)
                {
                    this.Background = new SolidColorBrush(Color.FromRgb(248, 250, 252));
                }
            }
            
            App.Log("Theme applied: " + (isDarkMode ? "dark" : "light"));
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