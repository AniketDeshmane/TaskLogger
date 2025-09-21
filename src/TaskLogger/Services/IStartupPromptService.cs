using System;

namespace TaskLogger.Services
{
    public interface IStartupPromptService
    {
        bool ShouldPromptForStartup();
        void MarkStartupPrompted();
        void PromptForStartup(Action<bool> onResult);
    }
}
