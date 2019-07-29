using System;
using System.Threading.Tasks;
using SigneWordBotAspCore.Services;
using SigneWordBotAspCore.States;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
    public class CreateCredentialsCommand: IBotCommand
    {
        public CreateCredentialsCommand()
        {
        }



        public string Name => "/createCredentials";

        public UserNextState AfterState => UserNextState.WaitCredentials;

        public async Task Execute(Message message, TelegramBotClient client)
        {
            var chatId = message.Chat.Id;

            try
            {
#if DEBUG
                Console.WriteLine("Enter password general password for your main basket of passwords.");
            }
            finally
            {
            }
#else
                await client.SendTextMessageAsync(chatId,
               "Enter password general password for your main basket of passwords.",
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Default);
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
#endif
        }

        public Task ExecuteSql(Message message, TelegramBotClient client, IDataBaseService dbService)
        {
            throw new NotImplementedException();
        }
    }
}
