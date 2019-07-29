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
    public class UpdateService : IUpdateService
    {
        private readonly IBotService _botService;
        private readonly ILogger<UpdateService> _logger;
        private readonly IDataBaseService dataBaseService;

        private List<IBotCommand> BotCommands { get; set; }

        private Dictionary<string, IBotCommand> NameCommandDict { get; }
        private Dictionary<long, UserNextState> StateForChaId { get; }

        public UpdateService(IBotService botService, ILogger<UpdateService> logger, IDataBaseService dataBaseService)
        {
            _botService = botService;
            _logger = logger;

            this.dataBaseService = dataBaseService;

            BotCommands = new List<IBotCommand> {
                new HelpCommand(), 
                // new StartCommand(),
                new CreateCredentialsCommand(),
                new EnterPasswordCommand(),
                new EnterCredentialsCommand(),
            };

            StateForChaId = new Dictionary<long, UserNextState>();
            NameCommandDict = new Dictionary<string, IBotCommand>();

            NameCommandDict = BotCommands.ToDictionary(c => c.Name, c => c);
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

            switch (state)
            {
                case UserNextState.WaitPassword:
                    var pendnigCommand = NameCommandDict["EnterPasswordCommand"];
                    await pendnigCommand.ExecuteSql(message, _botService.Client, dataBaseService);
                    break;

                case UserNextState.WaitCredentials:
                    var pendingCommand = NameCommandDict["EnterCredentialsCommand"];
                    await pendingCommand.ExecuteSql(message, _botService.Client, dataBaseService);
                    break;

                case UserNextState.None:
                    break;
            }

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

            var command = NameCommandDict[message.Text];

            if (command.AfterState != UserNextState.None)
            {
                StateForChaId.Add(message.Chat.Id, command.AfterState);
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
            if (update == null || update.Message == null) return false;

            if (update.Message.Type != MessageType.Text)
                return false;

            var message = update.Message.Text;

            //TODO: Make partial command support
            if (!NameCommandDict.ContainsKey(message))
            {
                return false;
            }

            return true;
        }
    }
}
