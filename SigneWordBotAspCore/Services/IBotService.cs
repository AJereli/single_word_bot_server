using Telegram.Bot;

namespace SigneWordBotAspCore.Services { 
    public interface IBotService
    {
        TelegramBotClient Client { get; }
    }
}