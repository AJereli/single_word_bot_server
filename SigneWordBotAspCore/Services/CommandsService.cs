using System.Collections.Generic;
using System.Linq;
using SigneWordBotAspCore.BotCommands;
using SigneWordBotAspCore.Exceptions;

namespace SigneWordBotAspCore.Services
{
    internal sealed class CommandsService: ICommandsService
    {
        private readonly IDataBaseService _dataBaseService;
        private readonly IDictionary<string, AbstractBotCommand> _commandDictionary;

        public IEnumerable<AbstractBotCommand> Commands { get; set; }

        
        public CommandsService(IDataBaseService dataBaseService)
        {
            _dataBaseService = dataBaseService;
            
            Commands = new List<AbstractBotCommand> {
                new StartCommand(),
                new HelpCommand(),
                new CreateCredentialsCommand(),
                new EnterCredentialsCommand(dataBaseService),
                
            };
            
            _commandDictionary = Commands.ToDictionary(c => c.Name, c => c);
        }
        
        /// <summary>
        /// Return true if command if exist, false otherwise
        /// </summary>
        /// <param name="commandName"></param>
        /// <returns></returns>
        public bool IsValidCommandName(string commandName)
        {
            return _commandDictionary.ContainsKey(commandName);
        }

        
        
        public AbstractBotCommand GetCommand(string commandName)
        {
            try
            {
                return _commandDictionary[commandName];
            }
            catch (KeyNotFoundException)
            {
                throw new CommandNotFoundException();
            }
        }
    }
}