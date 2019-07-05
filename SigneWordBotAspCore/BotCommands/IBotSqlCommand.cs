using System;
using System.Threading.Tasks;
using SigneWordBotAspCore.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
    public interface IBotSqlCommand
    {
        Task ExecuteSql(Message message, TelegramBotClient client, IDataBaseService dbService);
    }
}
