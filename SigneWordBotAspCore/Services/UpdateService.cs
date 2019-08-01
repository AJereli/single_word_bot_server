using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SigneWordBotAspCore.BotCommands;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using SigneWordBotAspCore.States;
using System;
using Newtonsoft.Json;

namespace SigneWordBotAspCore.Services
{
    internal class UpdateService : IUpdateService
    {
        private readonly IBotService _botService;
        private readonly ILogger<UpdateService> _logger;
        private readonly ICommandsService _commandsService;
        private List<AbstractBotCommand> BotCommands { get; set; }

        private Dictionary<long, UserNextState> StateForChaId { get; }

        public UpdateService(IBotService botService, ILogger<UpdateService> logger, ICommandsService commandsService)
        {
            _botService = botService;
            _logger = logger;
            _commandsService = commandsService;

            BotCommands = new List<AbstractBotCommand>(commandsService.Commands);
            StateForChaId = new Dictionary<long, UserNextState>();
            
        }



        public async Task DoCommand(Update update)
        {
            if (!IsTextMessage(update)) {
                await _botService.Client.SendTextMessageAsync(
                        update.Message.Chat.Id, $"Unsupported command");
                return;
            }


            var message = update.Message;

            await TryExecuteStateMachineCommand(message);

            await TryExecuteMainCommand(update);

        }

        private async Task TryExecuteStateMachineCommand(Message message)
        {
            if (!StateForChaId.ContainsKey(message.Chat.Id))
            {
                return;
            }

            var state = StateForChaId[message.Chat.Id];
            AbstractBotCommand pendingCommand = null;
            switch (state)
            {
                case UserNextState.WaitPassword: 
                    pendingCommand = _commandsService.GetCommand("EnterPasswordCommand");
                    break;

                case UserNextState.WaitCredentials:
                    pendingCommand = _commandsService.GetCommand("EnterCredentialsCommand");
                    
                    break;

                case UserNextState.None:
                    break;
                
                default:
                    break;
            }

            if (pendingCommand != null) await pendingCommand?.Execute(message, _botService.Client);
            
            StateForChaId.Remove(message.Chat.Id);
        }

        private async Task TryExecuteMainCommand(Update update)
        {
            if (!IsValidUpdateCommand(update))
            {
                await _botService.Client.SendTextMessageAsync(
                        update.Message.Chat.Id, $"Unsupported command2");
                return;
            }
            var message = update.Message;

            var command = _commandsService.GetCommand(message.Text);

            if (command.NextState != UserNextState.None)
            {
                StateForChaId.Add(message.Chat.Id, command.NextState);
            }
            
            await command.Execute(message, _botService.Client);


        }


        public async Task EchoAsync(Update update)
        {
            string json = JsonConvert.SerializeObject(update);

            System.Console.WriteLine(json);
            _logger.LogInformation(json);

            if (update.Type != UpdateType.Message)
            {
                return;
            }
            //update.Message.
            var message = update.Message;

            //_logger.LogInformation("Received Message from {0}", message.Chat.Id);

            if (message.Type == MessageType.Text)
            {
                // Echo each Message
                await _botService.Client.SendTextMessageAsync(message.Chat.Id, "test: \n" + json);
            }
            else if (message.Type == MessageType.Photo)
            {
                // Download Photo
                var fileId = message.Photo.LastOrDefault()?.FileId;
                var file = await _botService.Client.GetFileAsync(fileId);

                var filename = file.FileId + "." + file.FilePath.Split('.').Last();

                using (var saveImageStream = System.IO.File.Open(filename, FileMode.Create))
                {
                    await _botService.Client.DownloadFileAsync(file.FilePath, saveImageStream);
                }

                await _botService.Client.SendTextMessageAsync(message.Chat.Id, "Thx for the Pics");
            }
        }

        private bool IsTextMessage(Update update)
        {
            if (update == null || update.Message == null) return false;

            return update.Message.Type == MessageType.Text;

        }


        private bool IsValidUpdateCommand(Update update)
        {
            if (update?.Message == null) return false;

            if (update.Message.Type != MessageType.Text)
                return false;

            var message = update.Message.Text;

            //TODO: Make partial command support
            if (!_commandsService.IsValidCommandName(message))
            {
                return false;
            }

            return true;
        }
    }
}
