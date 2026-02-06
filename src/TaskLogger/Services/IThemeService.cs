namespace TaskLogger.Services
{
    public interface IThemeService
    {
        bool IsDarkTheme { get; set; }
        void ApplyTheme();
    }
}
