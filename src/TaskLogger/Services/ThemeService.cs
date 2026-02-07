using System.Windows;
using MaterialDesignThemes.Wpf;

namespace TaskLogger.Services
{
    public class ThemeService : IThemeService
    {
        private readonly IConfigService _configService;

        public ThemeService(IConfigService configService)
        {
            _configService = configService;
        }

        public bool IsDarkTheme
        {
            get => _configService.GetIsDarkTheme();
            set
            {
                _configService.SetIsDarkTheme(value);
                ApplyTheme();
            }
        }

        public void ApplyTheme()
        {
            var resources = Application.Current.Resources;
            var theme = resources.GetTheme();
            if (IsDarkTheme)
                theme.SetDarkTheme();
            else
                theme.SetLightTheme();
            resources.SetTheme(theme);
        }
    }
}
