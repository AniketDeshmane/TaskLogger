using System;

namespace TaskLogger.Services
{
    public interface ISystemTrayService
    {
        void Initialize();
        void ShowBalloonTip(string title, string message, BalloonIcon icon = BalloonIcon.Info);
        void HideToTray();
        void ShowFromTray();
        void ExitApplication();
        bool IsVisible { get; }
        event EventHandler? ShowRequested;
        event EventHandler? ExitRequested;
    }

    public enum BalloonIcon
    {
        None,
        Info,
        Warning,
        Error
    }
}
