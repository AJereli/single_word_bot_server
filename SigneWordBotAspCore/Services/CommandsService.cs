using System;
using System.Collections.Generic;
using System.Linq;
using SigneWordBotAspCore.BotCommands;
using SigneWordBotAspCore.Exceptions;

namespace SigneWordBotAspCore.Services
{
    public sealed class CommandsService: ICommandsService
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly IDataBaseService _dataBaseService;
        private readonly IDictionary<string, AbstractBotCommand> _commandDictionary;

        public IEnumerable<AbstractBotCommand> Commands { get; set; }

        
        public CommandsService(IDataBaseService dataBaseService)
        {
            _dataBaseService = dataBaseService;
            
            Commands = new List<AbstractBotCommand> {
                new StartCommand(),
                new HelpCommand(this),
                new CreateCredentialsCommand(dataBaseService),
                new EnterCredentialsCommand(dataBaseService),
                new EnterPasswordCommand(dataBaseService),
                new CreateBasketCommand(dataBaseService),
                new ShareCommand(dataBaseService),
                new ShowCommand(dataBaseService),
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
                //Check for part of command name
                if (!_commandDictionary.ContainsKey(commandName))
                {
                    return _commandDictionary[commandName.Split(' ')[0]];
                }

                return _commandDictionary[commandName];
            }
            catch (KeyNotFoundException)
            {
                throw new CommandNotFoundException();
            }
            catch (IndexOutOfRangeException)
            {
                throw new CommandNotFoundException();
                
            }
        }
    }
}