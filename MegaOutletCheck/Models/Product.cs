using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MegaOutletCheck.Models
{
    /// <summary>
    /// Represents a product from WooCommerce
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("stock_quantity")]
        public int StockQuantity { get; set; }
        
        [JsonProperty("stock_status")]
        public string StockStatus { get; set; } = "instock";
        
        [JsonProperty("manage_stock")]
        public bool ManageStock { get; set; }
        
        // Other properties (no JSON mapping needed - property names match)
        public string Slug { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool Featured { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string RegularPrice { get; set; } = string.Empty;
        public string SalePrice { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public string PriceHtml { get; set; } = string.Empty;
        public bool OnSale { get; set; }
        public int TotalSales { get; set; }
        public bool Virtual { get; set; }
        public bool Downloadable { get; set; }
        
        [JsonProperty("images")]
        public List<ProductImage> Images { get; set; } = new();
        
        [JsonProperty("categories")]
        public List<ProductCategory> Categories { get; set; } = new();
        public string AverageRating { get; set; } = string.Empty;
        public int ReviewCount { get; set; }
        public List<ProductAttribute> Attributes { get; set; } = new();
        public List<int> Variants { get; set; } = new();
        public string Permalink { get; set; } = string.Empty;

        // Stock logic
        public bool IsInStock
        {
            get
            {
                // Check stock_status directly (mapped with JsonProperty)
                if (!string.IsNullOrEmpty(StockStatus) && StockStatus.ToLowerInvariant() == "instock") return true;
                if (!string.IsNullOrEmpty(StockStatus) && StockStatus.ToLowerInvariant() == "outofstock") return false;
                // Otherwise check stock_quantity
                return StockQuantity > 0;
            }
        }
        
        public bool IsOutOfStock => !IsInStock;
        
        public bool IsLowStock => IsInStock && StockQuantity > 0 && StockQuantity <= 5;

        public string StockDisplayText
        {
            get
            {
                // Out of stock
                if (StockQuantity <= 0) return "Niedostępny";
                
                // Show quantity only
                return StockQuantity == 1 ? "Dostępny: 1 szt." : $"Dostępny: {StockQuantity} szt.";
            }
        }

        // Simple number for badge
        public string StockQuantityDisplay
        {
            get
            {
                if (StockQuantity <= 0) return string.Empty;
                return StockQuantity.ToString();
            }
        }

        // Computed properties for UI - with fallbacks for missing data
        public string DisplayPrice
        {
            get
            {
                // Check sale price first
                if (!string.IsNullOrEmpty(SalePrice) && SalePrice != "0" && SalePrice != RegularPrice)
                {
                    return $"{SalePrice} zł";
                }
                // Check regular price
                if (!string.IsNullOrEmpty(RegularPrice) && RegularPrice != "0")
                {
                    return $"{RegularPrice} zł";
                }
                // Fallback to price field
                if (!string.IsNullOrEmpty(Price) && Price != "0")
                {
                    return $"{Price} zł";
                }
                return "Cena niedostępna";
            }
        }
        
        public string DisplayOriginalPrice => !string.IsNullOrEmpty(SalePrice) && SalePrice != "0" && SalePrice != RegularPrice 
            ? $"{RegularPrice} zł" 
            : string.Empty;

        public string PrimaryImageUrl => Images?.Count > 0 ? Images[0].Src : string.Empty;
        
        // Gallery: current image index and navigation
        private int _currentImageIndex = 0;
        public int CurrentImageIndex 
        { 
            get => _currentImageIndex;
            set => _currentImageIndex = Math.Max(0, Math.Min(value, Math.Max(0, Images?.Count ?? 1) - 1));
        }
        
        public string CurrentImageUrl
        {
            get
            {
                if (Images == null || Images.Count == 0) return string.Empty;
                var index = Math.Min(_currentImageIndex, Images.Count - 1);
                return index >= 0 ? Images[index].Src : Images[0].Src;
            }
        }
        
        public void NextImage() 
        {
            if (Images != null && Images.Count > 1)
            {
                _currentImageIndex = (_currentImageIndex + 1) % Images.Count;
            }
        }
        
        public void PreviousImage()
        {
            if (Images != null && Images.Count > 1)
            {
                _currentImageIndex = (_currentImageIndex - 1 + Images.Count) % Images.Count;
            }
        }
        
        public void ResetImageIndex() => _currentImageIndex = 0;
        
        // Get all image URLs for gallery
        public List<string> ImageUrls => Images?.Where(i => !string.IsNullOrEmpty(i.Src)).Select(i => i.Src).ToList() ?? new List<string>();
        
        // Get first N images for gallery
        public List<string> GetImages(int count) => ImageUrls.Take(count).ToList();
        
        // Get all image URLs as single string (for debugging)
        public string DebugImages => string.Join(", ", ImageUrls);
        
        // Display description - prefer short_description first, then full description, then placeholder
        public string DisplayDescription
        {
            get
            {
                // Try short description (strip HTML tags for display)
                if (!string.IsNullOrEmpty(ShortDescription))
                {
                    return StripHtml(ShortDescription);
                }
                // Try full description
                if (!string.IsNullOrEmpty(Description))
                {
                    return StripHtml(Description);
                }
                return "Brak opisu produktu";
            }
        }
        
        // Full HTML description for advanced rendering
        public string HtmlDescription => !string.IsNullOrEmpty(Description) ? Description : 
                                  (!string.IsNullOrEmpty(ShortDescription) ? ShortDescription : "");
        
        private static string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            // Simple HTML tag removal
            return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "").Trim();
        }
    }

    public class ProductImage
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        
        [JsonProperty("src")]
        public string Src { get; set; } = string.Empty;
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("alt")]
        public string Alt { get; set; } = string.Empty;
    }

    public class ProductCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }

    public class ProductTag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }

    public class ProductAttribute
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int Position { get; set; }
        public bool Visible { get; set; }
        public bool Variation { get; set; }
        public List<string> Options { get; set; } = new();
    }

    /// <summary>
    /// WC Product variation
    /// </summary>
    public class ProductVariation
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Permalink { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public string StockStatus { get; set; } = string.Empty;
        public List<ProductImage> Image { get; set; } = new();
        public List<ProductAttribute> Attributes { get; set; } = new();
    }

    /// <summary>
    /// API response for products list
    /// </summary>
    public class ProductsResponse
    {
        public List<Product> Products { get; set; } = new();
        public int Total { get; set; }
        public int TotalPages { get; set; }
    }
}