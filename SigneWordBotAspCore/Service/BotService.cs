using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace SigneWordBotAspCore.Services
{
    public class BotService : IBotService
    {

        public BotService()
        {
            Client = new TelegramBotClient("895198692:AAGcsvPQogiNGxexv6rdpMC0XHej6nKJfM0");
        }

        public TelegramBotClient Client { get; }
    }
}
