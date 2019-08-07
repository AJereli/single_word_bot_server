using System;
using System.Linq;
using System.Threading.Tasks;
using SigneWordBotAspCore.BotCommands.Options;
using SigneWordBotAspCore.Services;
using SigneWordBotAspCore.States;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Reflection;
using CommandLine;
using Microsoft.EntityFrameworkCore.Internal;

namespace SigneWordBotAspCore.BotCommands
{
    public class HelpCommand : AbstractBotCommand
    {
        private readonly ICommandsService _commandsService;

        private readonly string _commandDescription;
        
        public HelpCommand(ICommandsService commandsService)
        {
            _name = "/help";
            _commandsService = commandsService;

            _commandDescription = MakeCommandsDescription();
        }

        private string MakeCommandsDescription()
        {
            const string nspace = "SigneWordBotAspCore.BotCommands.Options";

            var types = Assembly.GetExecutingAssembly().GetTypes().AsParallel().Where(t => t.IsClass && t.Namespace == nspace);
            return types.Select(t =>
            {
                var classAttr = t.GetCustomAttribute<VerbAttribute>(false);

                if (classAttr == null)
                    return null;

                var pr = t.GetProperties().SelectMany(o => o.GetCustomAttributes<OptionAttribute>()).Select(o =>
                {
                    var required = o.Required ? " <b>Required</b>" : "";
                    return $"-{o.ShortName}  or  --{o.LongName}{required} {o.HelpText}";
                });

                var d = $"{classAttr.Name} {classAttr.HelpText ?? ""}\nParameters of the command:\n{pr.Join(Environment.NewLine)}";


                return d;
            }).Join(Environment.NewLine);
        }
        
        
        public override async Task Execute(Message message, TelegramBotClient client)
        {
            var chatId = message.Chat.Id;
            
            await client.SendTextMessageAsync(chatId,
                "/help for show this message\n" +
                "/start for creating account\n" + 
                _commandDescription,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}