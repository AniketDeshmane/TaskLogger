namespace TaskLogger.Services
{
    public interface IStartupService
    {
        bool IsStartupEnabled();
        void EnableStartup();
        void DisableStartup();
        void ToggleStartup();
    }
}
