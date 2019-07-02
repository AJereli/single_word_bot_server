using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
    public interface IBotCommand
    {
        string Name { get; }
        Task Execute(Message message, TelegramBotClient client);


    }
}
