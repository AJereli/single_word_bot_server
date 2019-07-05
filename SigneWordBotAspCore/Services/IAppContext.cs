using System;
namespace SigneWordBotAspCore.Services
{
    public interface IAppContext
    {
        string BotToken { get; }
        string DBConnectionString { get; }
    }
}
