using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace SigneWordBotAspCore.Services
{
    public class BotService : IBotService
    {
        private readonly IAppContext appContext;

        public TelegramBotClient Client { get; }


        public BotService(IAppContext appContext)
        {
            this.appContext = appContext;

            Client = new TelegramBotClient(appContext.BotToken);
            System.Console.WriteLine("Client was created");
            Client.SetWebhookAsync("https://single-word-server.herokuapp.com/api/update").Wait();
        }

    }
}
