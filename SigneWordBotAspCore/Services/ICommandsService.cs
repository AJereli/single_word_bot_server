using System.Collections.Generic;
using SigneWordBotAspCore.BotCommands;

namespace SigneWordBotAspCore.Services
{
    internal interface ICommandsService
    {
        IEnumerable<AbstractBotCommand> Commands { get; set; }
        bool IsValidCommandName(string commandName);
        AbstractBotCommand GetCommand(string commandName);
    }
}