using System;
using System.IO;
using Microsoft.Win32;

namespace TaskLogger.Services
{
    public class DatabaseConfigService : IDatabaseConfigService
    {
        private const string DatabasePathKey = "DatabasePath";
        private const string RegistryPath = @"SOFTWARE\TaskLogger";

        public string GetDatabasePath()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, false);
                var configuredPath = key?.GetValue(DatabasePathKey)?.ToString();
                
                if (!string.IsNullOrEmpty(configuredPath) && File.Exists(configuredPath))
                {
                    return configuredPath;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading database path from registry: {ex.Message}");
            }

            // Return default path if not configured or invalid
            return GetDefaultDatabasePath();
        }

        public void SetDatabasePath(string path)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
                key?.SetValue(DatabasePathKey, path);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving database path to registry: {ex.Message}");
                throw new InvalidOperationException($"Failed to save database path: {ex.Message}");
            }
        }

        public bool IsDatabasePathConfigured()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, false);
                var configuredPath = key?.GetValue(DatabasePathKey)?.ToString();
                return !string.IsNullOrEmpty(configuredPath) && ValidateDatabasePath(configuredPath);
            }
            catch
            {
                return false;
            }
        }

        public string GetDefaultDatabasePath()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var appDataPath = Path.Combine(documentsPath, "TaskLogger");
            Directory.CreateDirectory(appDataPath);
            return Path.Combine(appDataPath, "TaskLogger.db");
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

                // Check if directory exists or can be created
                if (!Directory.Exists(directory))
                {
                    try
                    {
                        Directory.CreateDirectory(directory);
                    }
                    catch
                    {
                        return false;
                    }
                }

                // Check if we can write to the directory
                var testFile = Path.Combine(directory, "test_write.tmp");
                try
                {
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
