using System;
using System.Threading.Tasks;
using SigneWordBotAspCore.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
    public class EnterPasswordCommand: IBotCommand
    {

        public string Name => "EnterPasswordCommand";

        public Task Execute(Message message, TelegramBotClient client)
        {
            throw new NotImplementedException();
        }

        public async Task ExecuteSql(Message message, TelegramBotClient client, IDataBaseService dbService)
        {
            var res = dbService.CreateUser(message.Text, message.Chat.Id);

            if (res)
            {
                await client.SendTextMessageAsync(message.Chat.Id,
                    "Your password created successful\n"+
                    "Now you can user /createCredential command now");
            }

        }
    }
}
