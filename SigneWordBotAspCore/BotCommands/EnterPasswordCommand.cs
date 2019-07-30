using System;
using System.Threading.Tasks;
using SigneWordBotAspCore.Services;
using SigneWordBotAspCore.States;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
    public class EnterPasswordCommand: AbstractBotCommand
    {

        private readonly IDataBaseService _dataBaseService;
        
        public EnterPasswordCommand(IDataBaseService dataBaseService)
        {
            _dataBaseService = dataBaseService;

            _name = "EnterPasswordCommand";
        }

        public override async Task Execute(Message message, TelegramBotClient client)
        {
            var res = _dataBaseService.CreateUser(message.Text, message.Chat.Id);

            if (res != -1)
            {
                await client.SendTextMessageAsync(message.Chat.Id,
                    "Your password created successful\n"+
                    "Now you can user /createCredentials command now" +
                    "Use this template for credentials sending:" +
                    "<login>" +
                    "<password>");
            }
        }

        
    }
}
