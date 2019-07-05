using System;
using System.Threading.Tasks;
using SigneWordBotAspCore.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
    public class StartCommand: IBotCommand
    {

        public string Name => "/start";

        public async Task Execute(Message message, TelegramBotClient client)
        {
            var chatId = message.Chat.Id;

            try
            {
                await client.SendTextMessageAsync(chatId,
               "Enter password general password for your main basket of passwords.",
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Default);
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           

        }

       
        public Task ExecuteSql(Message message, TelegramBotClient client, IDataBaseService dbService)
        {
            throw new NotImplementedException();
        }
    }
}
