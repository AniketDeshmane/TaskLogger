using System;

namespace TaskLogger.Services
{
    public interface IDatabaseConfigService
    {
        string GetDatabasePath();
        void SetDatabasePath(string path);
        bool IsDatabasePathConfigured();
        string GetDefaultDatabasePath();
        bool ValidateDatabasePath(string path);
    }
}
