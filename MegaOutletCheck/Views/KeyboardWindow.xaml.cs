using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MegaOutletCheck.Views
{
    public partial class KeyboardWindow : Window
    {
        private const string ConfigFile = "keyboard_config.json";

        public event Action<string>? KeyPressed;
        public event Action? SearchRequested;
        public event Action? ClosedRequested;

        private bool _isPolishMode;

        public KeyboardWindow()
        {
            InitializeComponent();

            if (Left <= 0 || Top <= 0)
            {
                Left = SystemParameters.PrimaryScreenWidth / 2 - Width / 2;
                Top = SystemParameters.PrimaryScreenHeight - Height - 100;
            }

            LoadPosition();
            Closing += KeyboardWindow_Closing;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void KeyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string key)
            {
                // Directly insert the character - no transformation
                KeyPressed?.Invoke(key == "Space" ? " " : key);
            }
        }

        private void PolishMode_Click(object sender, RoutedEventArgs e)
        {
            _isPolishMode = !_isPolishMode;

            if (_isPolishMode)
            {
                StandardKeyboard.Visibility = Visibility.Collapsed;
                PolishKeyboard.Visibility = Visibility.Visible;
                PLButton.Content = "QWERTY";
            }
            else
            {
                StandardKeyboard.Visibility = Visibility.Visible;
                PolishKeyboard.Visibility = Visibility.Collapsed;
                PLButton.Content = "PL ⭐";
            }
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            KeyPressed?.Invoke("BACKSPACE");
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchRequested?.Invoke();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            ClosedRequested?.Invoke();
        }

        private void KeyboardWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            SavePosition();
        }

        private void SavePosition()
        {
            try
            {
                var config = new { Left, Top, Width, Height };
                var json = JsonSerializer.Serialize(config);
                File.WriteAllText(ConfigFile, json);
            }
            catch { }
        }

        private void LoadPosition()
        {
            try
            {
                if (File.Exists(ConfigFile))
                {
                    var json = File.ReadAllText(ConfigFile);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("Left", out var left))
                        Left = left.GetDouble();
                    if (doc.RootElement.TryGetProperty("Top", out var top))
                        Top = top.GetDouble();
                    if (doc.RootElement.TryGetProperty("Width", out var width))
                        Width = width.GetDouble();
                    if (doc.RootElement.TryGetProperty("Height", out var height))
                        Height = height.GetDouble();
                }
            }
            catch { }
        }
    }
}