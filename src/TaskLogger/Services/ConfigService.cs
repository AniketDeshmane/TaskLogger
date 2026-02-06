using System;
using System.IO;
using Microsoft.Win32;

namespace TaskLogger.Services
{
    public class ConfigService : IConfigService
    {
        private const string ConfigFileName = "settings.json";

        public string GetDatabasePath()
        {
            try
            {
                var config = LoadConfiguration();
                if (config != null && !string.IsNullOrEmpty(config.DatabasePath) && File.Exists(config.DatabasePath))
                {
                    return config.DatabasePath;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading database path from config: {ex.Message}");
            }

            return GetDefaultDatabasePath();
        }

        public void SetDatabasePath(string path)
        {
            try
            {
                var config = LoadConfiguration() ?? new Configuration();
                config.DatabasePath = path;
                SaveConfiguration(config);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving database path to config: {ex.Message}");
                throw new InvalidOperationException($"Failed to save database path: {ex.Message}");
            }
        }

        public bool IsDatabasePathConfigured()
        {
            try
            {
                var config = LoadConfiguration();
                return config != null && !string.IsNullOrEmpty(config.DatabasePath) && ValidateDatabasePath(config.DatabasePath);
            }
            catch
            {
                return false;
            }
        }

        public string GetDefaultDatabasePath()
        {
            var appDataPath = GetAppDataPath();
            return Path.Combine(appDataPath, "TaskLogger.db");
        }

        public bool GetIsDarkTheme()
        {
            try
            {
                var config = LoadConfiguration();
                return config?.IsDarkTheme ?? false;
            }
            catch
            {
                return false;
            }
        }

        public void SetIsDarkTheme(bool isDarkTheme)
        {
            try
            {
                var config = LoadConfiguration() ?? new Configuration();
                config.IsDarkTheme = isDarkTheme;
                SaveConfiguration(config);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving theme setting: {ex.Message}");
                throw;
            }
        }

        private string GetAppDataPath()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDataPath = Path.Combine(localAppData, "TaskLogger");
            Directory.CreateDirectory(appDataPath);
            return appDataPath;
        }

        private string GetConfigFilePath()
        {
            return Path.Combine(GetAppDataPath(), ConfigFileName);
        }

        private Configuration? LoadConfiguration()
        {
            try
            {
                var configFile = GetConfigFilePath();
                if (File.Exists(configFile))
                {
                    var json = File.ReadAllText(configFile);
                    return System.Text.Json.JsonSerializer.Deserialize<Configuration>(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading configuration: {ex.Message}");
            }
            return null;
        }

        private void SaveConfiguration(Configuration config)
        {
            try
            {
                var configFile = GetConfigFilePath();
                var json = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving configuration: {ex.Message}");
                throw;
            }
        }

        public bool ValidateDatabasePath(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return false;

                var directory = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(directory))
                    return false;

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class Configuration
    {
        public string? DatabasePath { get; set; }
        public bool IsDarkTheme { get; set; }
    }
}
