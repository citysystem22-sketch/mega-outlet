using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace MegaOutletCheck.Views
{
    public partial class KeyboardWindow : Window
    {
        private const string ConfigFile = "keyboard_config.json";
        
        public event Action<string>? KeyPressed;
        public event Action? SearchRequested;
        public event Action? ClosedRequested;
        
        public KeyboardWindow()
        {
            InitializeComponent();
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
            if (sender is System.Windows.Controls.Button btn && btn.Content is string key)
            {
                KeyPressed?.Invoke(key);
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
                var config = new
                {
                    Left = Left,
                    Top = Top,
                    Width = Width,
                    Height = Height
                };
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