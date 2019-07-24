using System;
using System.Threading.Tasks;
using SigneWordBotAspCore.Services;
using SigneWordBotAspCore.States;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
    public class EnterCredentialsCommand: IBotCommand
    {
        public EnterCredentialsCommand()
        {
        }
        public UserStartState AfterState => UserStartState.None;

        public string Name => "EnterCredentialsCommand";

        public Task Execute(Message message, TelegramBotClient client)
        {
            throw new NotImplementedException();
        }

        public async Task ExecuteSql(Message message, TelegramBotClient client, IDataBaseService dbService)
        {
            var credentials = message.Text.Split('\n');
            var login = credentials[0];
            var password = credentials[1];


        }
    }
}
