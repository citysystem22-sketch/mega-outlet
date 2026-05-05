using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using MegaOutletCheck.Models;
using MegaOutletCheck.Services;
using MegaOutletCheck.ViewModels;

namespace MegaOutletCheck
{
    public partial class App : Application
    {
        private static readonly string LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MegaOutletCheck",
            "logs");

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Set up global exception handling
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            
            // Ensure log directory exists
            EnsureLogDirectory();
            
            Log("Application started");
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            LogError("Unhandled exception", exception);
            
            MessageBox.Show(
                $"Wystąpił nieoczekiwany błąd: {exception?.Message}\n\nProsimy zrestartować aplikację.",
                "Błąd aplikacji",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogError("Dispatcher exception", e.Exception);
            
            MessageBox.Show(
                $"Wystąpił błąd: {e.Exception.Message}",
                "Błąd",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            
            e.Handled = true;
        }

        private void EnsureLogDirectory()
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }
            }
            catch
            {
                // Ignore directory creation errors
            }
        }

        public static void Log(string message)
        {
            try
            {
                var logFile = Path.Combine(LogDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}\n";
                File.AppendAllText(logFile, logMessage);
            }
            catch
            {
                // Ignore logging errors
            }
        }

        public static void LogError(string message, Exception? exception)
        {
            try
            {
                var logFile = Path.Combine(LogDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}: {exception?.Message}\n{exception?.StackTrace}\n\n";
                File.AppendAllText(logFile, logMessage);
            }
            catch
            {
                // Ignore logging errors
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log("Application exit");
            base.OnExit(e);
        }
    }
}