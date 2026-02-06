using System;

namespace TaskLogger.Services
{
    public interface IConfigService
    {
        string GetDatabasePath();
        void SetDatabasePath(string path);
        bool IsDatabasePathConfigured();
        string GetDefaultDatabasePath();
        bool ValidateDatabasePath(string path);
        bool GetIsDarkTheme();
        void SetIsDarkTheme(bool isDarkTheme);
    }
}
