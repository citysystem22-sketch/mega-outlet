using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MegaOutletCheck.Models;
using MegaOutletCheck.Services;

namespace MegaOutletCheck.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly WooCommerceService _wooService;
        private readonly ProductCacheService _cacheService;
        private readonly AppSettings _settings;
        private CancellationTokenSource? _searchCts;
        
        [ObservableProperty]
        private string _searchQuery = string.Empty;
        
        [ObservableProperty]
        private bool _isSearching;
        
        [ObservableProperty]
        private bool _isLoadingMore;
        
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        [ObservableProperty]
        private bool _hasError;
        
        [ObservableProperty]
        private Product? _selectedProduct;
        
        [ObservableProperty]
        private bool _isDetailsPanelOpen;

        [ObservableProperty]
        private bool _showOnlyAvailable = true; // Default: show only available products
        
        [ObservableProperty]
        private bool _isSettingsOpen;
        
        [ObservableProperty]
        private string _settingsStoreUrl = string.Empty;
        
        [ObservableProperty]
        private string _settingsApiKey = string.Empty;
        
        [ObservableProperty]
        private string _settingsApiSecret = string.Empty;
        
        [ObservableProperty]
        private string _connectionStatus = string.Empty;
        
        [ObservableProperty]
        private bool _isTestingConnection;
        
        [ObservableProperty]
        private bool _isOfflineMode;
        
        [ObservableProperty]
        private int _currentPage = 1;
        
        [ObservableProperty]
        private bool _hasMoreResults = true;
        
        [ObservableProperty]
        private bool _isDarkMode;
        
        public ObservableCollection<Product> Products { get; } = new();
        
        public MainViewModel(AppSettings settings, WooCommerceService wooService, ProductCacheService cacheService)
        {
            _settings = settings;
            _wooService = wooService;
            _cacheService = cacheService;
            
            // Initialize settings from loaded config
            SettingsStoreUrl = settings.StoreUrl;
            SettingsApiKey = settings.ApiKey;
            SettingsApiSecret = settings.ApiSecret;
            IsDarkMode = settings.IsDarkMode;
        }

        partial void OnSearchQueryChanged(string value)
        {
            // Cancel previous search
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            
            // Debounce search
            _ = SearchWithDebounceAsync(value, _searchCts.Token);
        }

        private async Task SearchWithDebounceAsync(string query, CancellationToken cancellationToken)
        {
            // Wait for debounce delay
            await Task.Delay(_settings.SearchDebounceMs, cancellationToken);
            
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            
            // Minimum characters check
            if (string.IsNullOrWhiteSpace(query) || query.Length < _settings.MinSearchCharacters)
            {
                Products.Clear();
                IsSearching = false;
                return;
            }
            
            await SearchProductsAsync(query, 1);
        }

        private async Task SearchProductsAsync(string query, int page)
        {
            try
            {
                if (page == 1)
                {
                    IsSearching = true;
                    HasError = false;
                    ErrorMessage = string.Empty;
                    Products.Clear();
                }
                else
                {
                    IsLoadingMore = true;
                }
                
                CurrentPage = page;
                
                // Try online search first
                if (!_settings.IsConfigured)
                {
                    ShowError("API nie skonfigurowane. Otwórz ustawienia.");
                    return;
                }
                
                var results = await _wooService.SearchProductsAsync(query, page, _settings.ResultsPageSize);
                
                // Apply "show only available" filter
                if (ShowOnlyAvailable && results != null)
                {
                    results = results.Where(p => p.IsInStock).ToArray();
                }
                
                IsOfflineMode = false;
                
                if (results != null && results.Length > 0)
                {
                    foreach (var product in results)
                    {
                        // Avoid duplicates
                        if (!Products.Any(p => p.Id == product.Id))
                        {
                            Products.Add(product);
                        }
                    }
                    
                    HasMoreResults = results.Length >= _settings.ResultsPageSize;
                    
                    // Update cache in background
                    await Task.Run(async () =>
                    {
                        _cacheService.AddProducts(results);
                        await _cacheService.SaveAsync();
                    });
                }
                else
                {
                    HasMoreResults = false;
                    
                    // Try offline cache if no results
                    if (page == 1 && Products.Count == 0)
                    {
                        var cachedResults = _cacheService.SearchProducts(query);
                        
                        // Apply filter if enabled
                        if (ShowOnlyAvailable)
                        {
                            cachedResults = cachedResults.Where(p => p.IsInStock).ToList();
                        }
                        
                        foreach (var product in cachedResults)
                        {
                            if (!Products.Any(p => p.Id == product.Id))
                            {
                                Products.Add(product);
                            }
                        }
                        
                        if (Products.Count > 0)
                        {
                            IsOfflineMode = true;
                            ShowError("Tryb offline - wyświetlane dane z pamięci podręcznej");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Search was cancelled, ignore
            }
            catch (Exception ex)
            {
                ShowError($"Błąd wyszukiwania: {ex.Message}");
                
                // Try offline cache
                if (Products.Count == 0)
                {
                    var cachedResults = _cacheService.SearchProducts(query);
                    
                    // Apply filter if enabled
                    if (ShowOnlyAvailable)
                    {
                        cachedResults = cachedResults.Where(p => p.IsInStock).ToList();
                    }
                    
                    foreach (var product in cachedResults)
                    {
                        Products.Add(product);
                    }
                    
                    if (Products.Count > 0)
                    {
                        IsOfflineMode = true;
                    }
                }
            }
            finally
            {
                IsSearching = false;
                IsLoadingMore = false;
            }
        }

        [RelayCommand]
        private async Task LoadMoreAsync()
        {
            if (IsLoadingMore || !HasMoreResults || IsOfflineMode)
            {
                return;
            }
            
            await SearchProductsAsync(SearchQuery, CurrentPage + 1);
        }

        [RelayCommand]
        private void SelectProduct(Product product)
        {
            product?.ResetImageIndex(); // Reset gallery to first image
            SelectedProduct = product;
            IsDetailsPanelOpen = true;
        }

        [RelayCommand]
        private void CloseDetailsPanel()
        {
            IsDetailsPanelOpen = false;
        }

        [RelayCommand]
        private void NextProductImage()
        {
            SelectedProduct?.NextImage();
            OnPropertyChanged(nameof(SelectedProduct));
        }

        [RelayCommand]
        private void PreviousProductImage()
        {
            SelectedProduct?.PreviousImage();
            OnPropertyChanged(nameof(SelectedProduct));
        }

        [RelayCommand]
        private void OpenSettings()
        {
            // Load current settings
            SettingsStoreUrl = _settings.StoreUrl;
            SettingsApiKey = _settings.ApiKey;
            SettingsApiSecret = _settings.ApiSecret;
            IsSettingsOpen = true;
        }

        [RelayCommand]
        private void CloseSettings()
        {
            IsSettingsOpen = false;
        }

        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(SettingsStoreUrl))
            {
                ShowError("URL sklepu jest wymagany");
                return;
            }
            
            // Save settings (credentials are saved if RememberCredentials is enabled)
            _settings.StoreUrl = SettingsStoreUrl.TrimEnd('/');
            
            if (_settings.RememberCredentials)
            {
                _settings.ApiKey = SettingsApiKey;
                _settings.ApiSecret = SettingsApiSecret;
            }
            
            _settings.IsDarkMode = IsDarkMode;
            _settings.Save();
            
            // Reconfigure service
            _wooService.Configure();
            
            IsSettingsOpen = false;
            
            // Clear error
            HasError = false;
            
            // Retry search if there was a query
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                await SearchProductsAsync(SearchQuery, 1);
            }
        }

        partial void OnIsDarkModeChanged(bool value)
        {
            // Save dark mode preference
            _settings.IsDarkMode = value;
            _settings.Save();
            
            // Notify app to update theme (will be handled in MainWindow)
            DarkModeChanged?.Invoke(this, value);
        }
        
        partial void OnShowOnlyAvailableChanged(bool value)
        {
            // Re-search when filter changes
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                _ = SearchProductsAsync(SearchQuery, 1);
            }
        }
        
        /// <summary>
        /// Event fired when dark mode changes
        /// </summary>
        public event EventHandler<bool>? DarkModeChanged;

        [RelayCommand]
        private async Task TestConnectionAsync()
        {
            IsTestingConnection = true;
            ConnectionStatus = "Testowanie połączenia...";
            
            try
            {
                // Temporarily save settings
                _settings.StoreUrl = SettingsStoreUrl.TrimEnd('/');
                _settings.ApiKey = SettingsApiKey;
                _settings.ApiSecret = SettingsApiSecret;
                _wooService.Configure();
                
                var result = await _wooService.TestConnectionAsync();
                
                if (result.success)
                {
                    ConnectionStatus = result.message;
                }
                else
                {
                    ConnectionStatus = result.message;
                }
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Błąd: {ex.Message}";
            }
            finally
            {
                IsTestingConnection = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                return;
            }
            
            await SearchProductsAsync(SearchQuery, 1);
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchQuery = string.Empty;
            Products.Clear();
            SelectedProduct = null;
            HasError = false;
            ErrorMessage = string.Empty;
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true;
        }
    }
}