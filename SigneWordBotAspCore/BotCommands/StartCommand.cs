using System;
using System.Threading.Tasks;
using SigneWordBotAspCore.Services;
using SigneWordBotAspCore.States;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
    public class StartCommand : AbstractBotCommand
    {
        public StartCommand()
        {
            _name = "/start";
            _nextState = UserNextState.WaitPassword;
        }

        public UserNextState AfterState => UserNextState.WaitPassword;

        public override async Task Execute(Message message, TelegramBotClient client)
        {
            var chatId = message.Chat.Id;


            Console.WriteLine("Enter general password for your main basket of passwords.");
            await client.SendTextMessageAsync(chatId,
                "Enter password general password for your main basket of passwords.",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Default);
        }
    }
}