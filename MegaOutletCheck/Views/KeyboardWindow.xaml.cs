using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace MegaOutletCheck.Views
{
    public partial class KeyboardWindow : Window
    {
        private const string ConfigFile = "keyboard_config.json";

        // Polish char mappings
        private static readonly Dictionary<char, char[]> PolishMap = new()
        {
            { 'A', new[] { 'Ą' } },
            { 'C', new[] { 'Ć' } },
            { 'E', new[] { 'Ę' } },
            { 'L', new[] { 'Ł' } },
            { 'N', new[] { 'Ń' } },
            { 'O', new[] { 'Ó' } },
            { 'S', new[] { 'Ś' } },
            { 'Z', new[] { 'Ź', 'Ż' } }
        };

        private static readonly Dictionary<char, char[]> PolishLowerMap = new()
        {
            { 'A', new[] { 'ą', 'Ą' } },
            { 'C', new[] { 'ć', 'Ć' } },
            { 'E', new[] { 'ę', 'Ę' } },
            { 'L', new[] { 'ł', 'Ł' } },
            { 'N', new[] { 'ń', 'Ń' } },
            { 'O', new[] { 'ó', 'Ó' } },
            { 'S', new[] { 'ś', 'Ś' } },
            { 'Z', new[] { 'ź', 'Ź', 'ż', 'Ż' } }
        };

        public event Action<string>? KeyPressed;
        public event Action? SearchRequested;
        public event Action? ClosedRequested;

        private DispatcherTimer? _longPressTimer;
        private Button? _longPressButton;
        private bool _isPolishMode;
        private DateTime _pressStart;

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

        private void Key_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button btn && btn.Content is string key && key.Length == 1)
            {
                char baseChar = char.ToUpper(key[0]);
                if (PolishMap.ContainsKey(baseChar))
                {
                    _pressStart = DateTime.Now;
                    _longPressButton = btn;
                    _longPressTimer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(500)
                    };
                    _longPressTimer.Tick += (s, args) =>
                    {
                        _longPressTimer?.Stop();
                        ShowPolishPopup(btn, baseChar);
                    };
                    _longPressTimer.Start();
                }
            }
        }

        private void Key_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _longPressTimer?.Stop();
            _longPressTimer = null;
            _longPressButton = null;
            PolishPopup.IsOpen = false;
        }

        private void ShowPolishPopup(Button source, char baseChar)
        {
            PolishCharsPanel.Children.Clear();

            var map = _isPolishMode ? PolishLowerMap : PolishMap;
            var chars = map.ContainsKey(baseChar) ? map[baseChar] : Array.Empty<char>();

            foreach (var c in chars)
            {
                var btn = new Button
                {
                    Content = c.ToString(),
                    Width = 36,
                    Height = 36,
                    Margin = new Thickness(2),
                    FontSize = 16,
                    Cursor = Cursors.Hand
                };
                var charToSend = c;
                btn.Click += (s, args) =>
                {
                    KeyPressed?.Invoke(charToSend.ToString());
                    PolishPopup.IsOpen = false;
                };
                PolishCharsPanel.Children.Add(btn);
            }

            PolishPopup.PlacementTarget = source;
            PolishPopup.IsOpen = true;
        }

        private void KeyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string key)
            {
                // If quick tap (< 500ms), send regular key
                if (_longPressTimer == null || (DateTime.Now - _pressStart).TotalMilliseconds < 500)
                {
                    if (_isPolishMode && key.Length == 1 && char.IsLetter(key[0]))
                    {
                        key = char.ToLower(key[0]).ToString();
                    }
                    KeyPressed?.Invoke(key == "Space" ? " " : key);
                }
            }
            // Exit Polish mode after one key
            if (_isPolishMode)
            {
                _isPolishMode = false;
                UpdatePLButton();
            }
        }

        private void PolishToggle_Click(object sender, RoutedEventArgs e)
        {
            _isPolishMode = !_isPolishMode;
            UpdatePLButton();
        }

        private void UpdatePLButton()
        {
            if (BtnPL != null)
            {
                BtnPL.Content = _isPolishMode ? "PL ⭐" : "PL";
                BtnPL.FontWeight = _isPolishMode ? FontWeights.Bold : FontWeights.Normal;
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