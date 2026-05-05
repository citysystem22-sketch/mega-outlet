using System;
using System.IO;
using Newtonsoft.Json;

namespace MegaOutletCheck.Models
{
    /// <summary>
    /// Application settings
    /// </summary>
    public class AppSettings
    {
        // Default settings
        public string StoreUrl { get; set; } = "https://mega-outlet.pl";
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public int ApiUserId { get; set; } = 1; // WooCommerce API requires user_id
        
        // Search settings
        public int SearchDebounceMs { get; set; } = 300;
        public int MinSearchCharacters { get; set; } = 2;
        public int ResultsPageSize { get; set; } = 20;
        
        // Cache settings
        public bool EnableCache { get; set; } = true;
        public int CacheExpirationMinutes { get; set; } = 30;
        
        // Display settings
        public string Language { get; set; } = "pl-PL";
        public bool IsDarkMode { get; set; } = false; // Default to light mode
        public bool RememberCredentials { get; set; } = true; // Save credentials locally
        
        /// <summary>
        /// Check if API is configured
        /// </summary>
        public bool IsConfigured => !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ApiSecret);

        /// <summary>
        /// Get the config file path
        /// </summary>
        public static string ConfigFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MegaOutletCheck",
            "settings.json");

        /// <summary>
        /// Get external config path (next to exe)
        /// </summary>
        public static string ExternalConfigPath
        {
            get
            {
                var exePath = AppDomain.CurrentDomain.BaseDirectory;
                // Try multiple locations
                var paths = new[]
                {
                    Path.Combine(exePath, "settings.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "settings.json"),
                    Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase ?? "", "settings.json")
                };
                
                foreach (var path in paths)
                {
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
                
                // Return first path as default
                return paths[0];
            }
        }

        /// <summary>
        /// Get the cache directory path
        /// </summary>
        public static string CacheDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MegaOutletCheck",
            "cache");

        /// <summary>
        /// Save settings to file
        /// </summary>
        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(ConfigFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Don't save secrets in plain text in production
                // For now, we store them; in production use DPAPI or keyring
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                LogError("Failed to save settings", ex);
            }
        }

        /// <summary>
        /// Load settings from file
        /// </summary>
        public static AppSettings Load()
        {
            try
            {
                // First check for external config (next to exe)
                if (File.Exists(ExternalConfigPath))
                {
                    var json = File.ReadAllText(ExternalConfigPath);
                    var settings = JsonConvert.DeserializeObject<AppSettings>(json);
                    if (settings != null && settings.IsConfigured)
                    {
                        return settings;
                    }
                }
                
                // Then check for local config
                if (File.Exists(ConfigFilePath))
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                LogError("Failed to load settings", ex);
            }

            return new AppSettings();
        }

        private static void LogError(string message, Exception ex)
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MegaOutletCheck",
                "logs");

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
                // Ignore logging errors
            }
        }
    }
}