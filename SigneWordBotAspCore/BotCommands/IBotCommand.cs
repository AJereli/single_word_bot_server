using System;
using System.Threading.Tasks;
using SigneWordBotAspCore.States;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
    public interface IBotCommand: IBotSqlCommand
    {
        string Name { get; }
        Task Execute(Message message, TelegramBotClient client);
        UserStartState AfterState { get; }

    }
}
