using System;
using System.Threading.Tasks;
using SigneWordBotAspCore.Services;
using SigneWordBotAspCore.States;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
    internal sealed class EnterCredentialsCommand: AbstractBotCommand
    {
        private readonly IDataBaseService _dataBaseService;
        
        public EnterCredentialsCommand(IDataBaseService dbService)
        {
            _name = "EnterCredentialsCommand";

            _dataBaseService = dbService;
        }
        

        public override async Task Execute(Message message, TelegramBotClient client)
        {
            var credentials = message.Text.Split('\n');
            var login = credentials[0];
            var password = credentials[1];


        }
    }
}
