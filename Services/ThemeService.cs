using System;
using System.Windows;
using Microsoft.Win32;

namespace TaskLogger.Services
{
    public interface IThemeService
    {
        bool IsDarkMode { get; set; }
        void ApplyTheme(bool isDarkMode);
        void LoadThemePreference();
        void SaveThemePreference();
    }

    public class ThemeService : IThemeService
    {
        private const string ThemeKey = "IsDarkMode";
        private const string RegistryPath = @"SOFTWARE\TaskLogger";
        private bool _isDarkMode;

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    ApplyTheme(value);
                    SaveThemePreference();
                }
            }
        }

        public ThemeService()
        {
            LoadThemePreference();
        }

        public void ApplyTheme(bool isDarkMode)
        {
            try
            {
                var app = Application.Current;
                if (app == null) return;

                var mergedDictionaries = app.Resources.MergedDictionaries;
                mergedDictionaries.Clear();

                var themeUri = isDarkMode
                    ? new Uri("/Themes/DarkTheme.xaml", UriKind.Relative)
                    : new Uri("/Themes/LightTheme.xaml", UriKind.Relative);

                mergedDictionaries.Add(new ResourceDictionary { Source = themeUri });
                _isDarkMode = isDarkMode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme: {ex.Message}");
            }
        }

        public void LoadThemePreference()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, false);
                if (key != null)
                {
                    var value = key.GetValue(ThemeKey);
                    if (value != null && bool.TryParse(value.ToString(), out var isDarkMode))
                    {
                        _isDarkMode = isDarkMode;
                        ApplyTheme(isDarkMode);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading theme preference: {ex.Message}");
            }

            // Default to light mode
            _isDarkMode = false;
            ApplyTheme(false);
        }

        public void SaveThemePreference()
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
                key?.SetValue(ThemeKey, _isDarkMode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving theme preference: {ex.Message}");
            }
        }
    }
}