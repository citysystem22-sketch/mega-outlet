using System;
using System.Collections.Generic;
using System.Linq;

namespace MegaOutletCheck.Models
{
    /// <summary>
    /// Represents a product from WooCommerce
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
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
        public List<ProductImage> Images { get; set; } = new();
        public List<ProductCategory> Categories { get; set; } = new();
        public List<ProductTag> Tags { get; set; } = new();
        public string AverageRating { get; set; } = string.Empty;
        public int ReviewCount { get; set; }
        public int StockQuantity { get; set; }
        public string StockStatus { get; set; } = "instock";
        public bool ManageStock { get; set; }
        public string Stock { get; set; } = string.Empty;
        public List<ProductAttribute> Attributes { get; set; } = new();
        public List<int> Variants { get; set; } = new();
        public string Permalink { get; set; } = string.Empty;

        // Stock logic - CORRECT
        public bool IsInStock
        {
            get
            {
                if (!string.IsNullOrEmpty(StockStatus) && StockStatus.ToLowerInvariant() == "instock") return true;
                if (!string.IsNullOrEmpty(StockStatus) && StockStatus.ToLowerInvariant() == "outofstock") return false;
                return StockQuantity > 0;
            }
        }
        
        public bool IsOutOfStock
        {
            get
            {
                if (!string.IsNullOrEmpty(StockStatus) && StockStatus.ToLowerInvariant() == "outofstock") return true;
                return StockQuantity <= 0;
            }
        }
        
        public bool IsLowStock => IsInStock && StockQuantity > 0 && StockQuantity <= 5;

        public string StockDisplayText
        {
            get
            {
                if (!string.IsNullOrEmpty(StockStatus) && StockStatus.ToLowerInvariant() == "outofstock") return "Niedostępny";
                if (StockQuantity <= 0) return "Niedostępny";
                if (StockQuantity <= 5) return $"Ostatnie sztuki ({StockQuantity})";
                if (StockQuantity > 5) return $"Dostępny ({StockQuantity})";
                return "Dostępny";
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
        public string Src { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
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