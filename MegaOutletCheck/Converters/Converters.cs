using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MegaOutletCheck.Models;

namespace MegaOutletCheck.Converters
{
    /// <summary>
    /// Convert boolean to Visibility
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;
            bool invert = parameter?.ToString() == "Invert";
            
            if (invert)
            {
                boolValue = !boolValue;
            }
            
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v == Visibility.Visible;
        }
    }

    /// <summary>
    /// Convert stock status to color brush
    /// </summary>
    public class StockStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Product product)
            {
                if (product.IsOutOfStock)
                {
                    return new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red #EF4444
                }
                if (product.IsLowStock)
                {
                    return new SolidColorBrush(Color.FromRgb(245, 158, 11)); // Orange #F59E0B
                }
                return new SolidColorBrush(Color.FromRgb(16, 185, 129)); // Green #10B981
            }
            
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convert stock status to text
    /// </summary>
    public class StockStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Product product)
            {
                return product.StockDisplayText;
            }
            
            return "Nieznany";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convert string to visibility (empty = collapsed)
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasValue = !string.IsNullOrEmpty(value as string);
            bool invert = parameter?.ToString() == "Invert";
            
            if (invert)
            {
                hasValue = !hasValue;
            }
            
            return hasValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convert price to display format
    /// </summary>
    public class PriceToDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Product product)
            {
                var price = product.DisplayPrice;
                var originalPrice = product.DisplayOriginalPrice;
                
                if (!string.IsNullOrEmpty(originalPrice))
                {
                    return $"{originalPrice} → {price}";
                }
                
                return price;
            }
            
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convert product image URL to image source
    /// </summary>
    public class ImageUrlToSourceConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string url && !string.IsNullOrEmpty(url))
            {
                try
                {
                    return new System.Windows.Media.Imaging.BitmapImage(new Uri(url));
                }
                catch
                {
                    return null;
                }
            }
            
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Invert boolean value
    /// </summary>
    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && !b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && !b;
        }
    }
}