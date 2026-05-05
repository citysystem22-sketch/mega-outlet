using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MegaOutletCheck.Models;
using Newtonsoft.Json;

namespace MegaOutletCheck.Services
{
    /// <summary>
    /// Cache service for offline fallback and performance
    /// </summary>
    public class ProductCacheService
    {
        private readonly string _cacheDirectory;
        private readonly int _expirationMinutes;
        
        private Dictionary<int, Product> _productsCache = new();
        private Dictionary<string, List<int>> _searchIndex = new();
        private DateTime _lastUpdate;
        
        public ProductCacheService(AppSettings settings)
        {
            _cacheDirectory = AppSettings.CacheDirectory;
            _expirationMinutes = settings.CacheExpirationMinutes;
            
            EnsureCacheDirectory();
        }

        private void EnsureCacheDirectory()
        {
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
        }

        /// <summary>
        /// Load cache from disk
        /// </summary>
        public async Task LoadAsync()
        {
            try
            {
                var productsFile = Path.Combine(_cacheDirectory, "products.json");
                
                if (File.Exists(productsFile))
                {
                    var json = await File.ReadAllTextAsync(productsFile);
                    var products = JsonConvert.DeserializeObject<List<Product>>(json);
                    
                    if (products != null)
                    {
                        _productsCache = products.ToDictionary(p => p.Id);
                        _searchIndex = BuildSearchIndex(products);
                        _lastUpdate = File.GetLastWriteTime(productsFile);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Failed to load cache", ex);
            }
        }

        /// <summary>
        /// Save cache to disk
        /// </summary>
        public async Task SaveAsync()
        {
            try
            {
                var productsFile = Path.Combine(_cacheDirectory, "products.json");
                var products = _productsCache.Values.ToList();
                var json = JsonConvert.SerializeObject(products, Formatting.Indented);
                await File.WriteAllTextAsync(productsFile, json);
                
                _lastUpdate = DateTime.Now;
            }
            catch (Exception ex)
            {
                LogError("Failed to save cache", ex);
            }
        }

        /// <summary>
        /// Add products to cache
        /// </summary>
        public void AddProducts(IEnumerable<Product> products)
        {
            foreach (var product in products)
            {
                _productsCache[product.Id] = product;
            }
            
            _searchIndex = BuildSearchIndex(_productsCache.Values.ToList());
        }

        /// <summary>
        /// Search products in cache
        /// </summary>
        public List<Product> SearchProducts(string query)
        {
            var results = new List<Product>();
            var queryLower = query.ToLowerInvariant();
            var queryTerms = queryLower.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var kvp in _searchIndex)
            {
                var searchTerms = kvp.Key.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                // Check if any search term matches
                bool matches = queryTerms.Any(qt => 
                    searchTerms.Any(st => st.Contains(qt) || qt.Contains(st)));
                
                if (matches)
                {
                    foreach (var productId in kvp.Value)
                    {
                        if (_productsCache.TryGetValue(productId, out var product))
                        {
                            results.Add(product);
                        }
                    }
                }
            }
            
            return results.Distinct().Take(20).ToList();
        }

        /// <summary>
        /// Get product by ID from cache
        /// </summary>
        public Product? GetProduct(int productId)
        {
            return _productsCache.TryGetValue(productId, out var product) ? product : null;
        }

        /// <summary>
        /// Get all cached products
        /// </summary>
        public List<Product> GetAllProducts()
        {
            return _productsCache.Values.ToList();
        }

        /// <summary>
        /// Check if cache is stale
        /// </summary>
        public bool IsCacheStale()
        {
            var age = DateTime.Now - _lastUpdate;
            return age.TotalMinutes > _expirationMinutes;
        }

        /// <summary>
        /// Get cache age in minutes
        /// </summary>
        public int GetCacheAgeMinutes()
        {
            return (int)(DateTime.Now - _lastUpdate).TotalMinutes;
        }

        /// <summary>
        /// Clear cache
        /// </summary>
        public void Clear()
        {
            _productsCache.Clear();
            _searchIndex.Clear();
            
            try
            {
                var productsFile = Path.Combine(_cacheDirectory, "products.json");
                if (File.Exists(productsFile))
                {
                    File.Delete(productsFile);
                }
            }
            catch (Exception ex)
            {
                LogError("Failed to clear cache", ex);
            }
        }

        private Dictionary<string, List<int>> BuildSearchIndex(List<Product> products)
        {
            var index = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var product in products)
            {
                // Index by name words
                var words = GetSearchWords(product.Name);
                foreach (var word in words)
                {
                    if (!index.ContainsKey(word))
                    {
                        index[word] = new List<int>();
                    }
                    index[word].Add(product.Id);
                }
                
                // Index by SKU if available
                var sku = product.Slug?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(sku))
                {
                    if (!index.ContainsKey(sku))
                    {
                        index[sku] = new List<int>();
                    }
                    index[sku].Add(product.Id);
                }
            }
            
            return index;
        }

        private List<string> GetSearchWords(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new List<string>();
            }
            
            // Split by common separators and take unique words
            return text.ToLowerInvariant()
                .Split(new[] { ' ', '-', '_', '/', '\\', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']' }, 
                    StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length >= 2)
                .Distinct()
                .ToList();
        }

        private void LogError(string message, Exception ex)
        {
            var logDir = Path.Combine(_cacheDirectory, "logs");
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            var logFile = Path.Combine(logDir, $"error_{DateTime.Now:yyyyMMdd}.log");
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}: {ex.Message}\n{ex.StackTrace}\n\n";
            
            try
            {
                File.AppendAllText(logFile, logMessage);
            }
            catch
            {
                // Ignore
            }
        }
    }
}