using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
    public class HelpCommand: IBotCommand
    {
        public HelpCommand()
        {
        }

        public string Name => "/help";

        public async Task Execute(Message message, TelegramBotClient client)
        {
            var chatId = message.Chat.Id;
            await client.SendTextMessageAsync(chatId,
                "/help for show this message\n" +
                "/start for creating account\n" +
                "/login for signin\n",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

        }
    }
}
