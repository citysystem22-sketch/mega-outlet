using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MegaOutletCheck.Models;
using Newtonsoft.Json;

namespace MegaOutletCheck.Services
{
    /// <summary>
    /// Service for interacting with WooCommerce REST API
    /// </summary>
    public class WooCommerceService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly AppSettings _settings;
        private string? _baseUrl;
        private string? _consumerKey;
        private string? _consumerSecret;
        
        // Cache for products
        private readonly Dictionary<string, (Product[] products, DateTime timestamp)> _searchCache = new();
        private readonly Dictionary<int, Product> _productCache = new();
        
        // For retry logic
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 1000;

        public WooCommerceService(AppSettings settings)
        {
            _settings = settings;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            
            Configure();
        }

        /// <summary>
        /// Configure the service with current settings
        /// </summary>
        public void Configure()
        {
            _baseUrl = _settings.StoreUrl.TrimEnd('/');
            _consumerKey = _settings.ApiKey;
            _consumerSecret = _settings.ApiSecret;
            
            // Clear cache on reconfigure
            _searchCache.Clear();
            _productCache.Clear();
        }

        /// <summary>
        /// Test connection to the WooCommerce store
        /// </summary>
        public async Task<(bool success, string message)> TestConnectionAsync()
        {
            if (!_settings.IsConfigured)
            {
                return (false, "API keys not configured. Please configure API keys in settings.");
            }

            try
            {
                var response = await GetAsync<dynamic>("/wp-json/wc/v3/products", new Dictionary<string, string>
                {
                    ["per_page"] = "1"
                });

                if (response != null)
                {
                    return (true, "Connection successful!");
                }
                
                return (false, "Invalid response from server");
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Connection failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Search products by query
        /// </summary>
        public async Task<Product[]> SearchProductsAsync(string query, int page = 1, int perPage = 20)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Array.Empty<Product>();
            }

            // Check cache first
            var cacheKey = $"{query.ToLowerInvariant()}_{page}_{perPage}";
            
            if (_settings.EnableCache && _searchCache.TryGetValue(cacheKey, out var cached))
            {
                var age = DateTime.Now - cached.timestamp;
                if (age.TotalMinutes < _settings.CacheExpirationMinutes)
                {
                    return cached.products;
                }
            }

            try
            {
                var results = await GetProductsAsync(new Dictionary<string, string>
                {
                    ["search"] = query,
                    ["per_page"] = perPage.ToString(),
                    ["page"] = page.ToString(),
                    ["orderby"] = "relevance"
                });

                // Cache the results
                if (_settings.EnableCache && results.Length > 0)
                {
                    _searchCache[cacheKey] = (results, DateTime.Now);
                }

                return results;
            }
            catch
            {
                // Return cached on error
                if (_searchCache.TryGetValue(cacheKey, out var cachedResults))
                {
                    return cachedResults.products;
                }
                
                throw;
            }
        }

        /// <summary>
        /// Get all products with pagination
        /// </summary>
        public async Task<Product[]> GetAllProductsAsync(int page = 1, int perPage = 20)
        {
            return await GetProductsAsync(new Dictionary<string, string>
            {
                ["per_page"] = perPage.ToString(),
                ["page"] = page.ToString(),
                ["orderby"] = "menu_order",
                ["order"] = "asc"
            });
        }

        /// <summary>
        /// Get a single product by ID
        /// </summary>
        public async Task<Product?> GetProductAsync(int productId)
        {
            // Check memory cache first
            if (_productCache.TryGetValue(productId, out var cached))
            {
                return cached;
            }

            try
            {
                var product = await GetAsync<Product>($"/wp-json/wc/v3/products/{productId}");
                
                if (product != null)
                {
                    _productCache[productId] = product;
                }
                
                return product;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get featured products (on sale)
        /// </summary>
        public async Task<Product[]> GetFeaturedProductsAsync(int count = 10)
        {
            return await GetProductsAsync(new Dictionary<string, string>
            {
                ["featured"] = "true",
                ["per_page"] = count.ToString()
            });
        }

        /// <summary>
        /// Get products on sale
        /// </summary>
        public async Task<Product[]> GetOnSaleProductsAsync(int page = 1, int perPage = 20)
        {
            return await GetProductsAsync(new Dictionary<string, string>
            {
                ["on_sale"] = "true",
                ["per_page"] = perPage.ToString(),
                ["page"] = page.ToString()
            });
        }

        private async Task<Product[]> GetProductsAsync(Dictionary<string, string> queryParams)
        {
            var response = await GetAsync<List<Product>>("/wp-json/wc/v3/products", queryParams);
            return response?.ToArray() ?? Array.Empty<Product>();
        }

        private async Task<T?> GetAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null)
            where T : class
        {
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(_baseUrl);
            urlBuilder.Append(endpoint);
            
            if (queryParams != null && queryParams.Count > 0)
            {
                urlBuilder.Append('?');
                var first = true;
                foreach (var param in queryParams)
                {
                    if (!first) urlBuilder.Append('&');
                    urlBuilder.Append(WebUtility.UrlEncode(param.Key));
                    urlBuilder.Append('=');
                    urlBuilder.Append(WebUtility.UrlEncode(param.Value));
                    first = false;
                }
            }

            // Add authentication to URL
            var separator = urlBuilder.ToString().Contains('?') ? '&' : '?';
            urlBuilder.Append(separator);
            urlBuilder.Append("consumer_key=");
            urlBuilder.Append(WebUtility.UrlEncode(_consumerKey));
            urlBuilder.Append("&consumer_secret=");
            urlBuilder.Append(WebUtility.UrlEncode(_consumerSecret));

            var request = new HttpRequestMessage(HttpMethod.Get, urlBuilder.ToString());
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await SendWithRetryAsync<T>(request);
        }

        private async Task<T?> SendWithRetryAsync<T>(HttpRequestMessage request)
            where T : class
        {
            Exception? lastException = null;
            
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    var response = await _httpClient.SendAsync(request);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        
                        // Handle specific types
                        if (typeof(T) == typeof(List<Product>))
                        {
                            return JsonConvert.DeserializeObject<List<Product>>(content) as T;
                        }
                        else if (typeof(T) == typeof(Product))
                        {
                            return JsonConvert.DeserializeObject<Product>(content) as T;
                        }
                        
                        // For dynamic, deserialize to object
                        if (typeof(T) == typeof(object))
                        {
                            return JsonConvert.DeserializeObject<dynamic>(content) as T;
                        }
                        
                        return JsonConvert.DeserializeObject<T>(content);
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new HttpRequestException("Invalid API keys");
                    }
                    else
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
                catch (HttpRequestException ex) when (attempt < MaxRetries - 1)
                {
                    lastException = ex;
                    await Task.Delay(RetryDelayMs * (attempt + 1));
                }
            }
            
            throw lastException ?? new HttpRequestException("Request failed after retries");
        }

        /// <summary>
        /// Clear the product cache
        /// </summary>
        public void ClearCache()
        {
            _searchCache.Clear();
            _productCache.Clear();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}