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
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            if (IsDarkTheme)
                theme.SetDarkTheme();
            else
                theme.SetLightTheme();
            paletteHelper.SetTheme(theme);
        }
    }
}
