﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SigneWordBotAspCore.BotCommands;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SigneWordBotAspCore.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly IBotService _botService;
        private readonly ILogger<UpdateService> _logger;

        private List<IBotCommand> BotCommands { get; set; }

        private Dictionary<string, IBotCommand> NameCommandDict { get; set; }

        public UpdateService(IBotService botService, ILogger<UpdateService> logger)
        {
            _botService = botService;
            _logger = logger;

            BotCommands = new List<IBotCommand> {
                new HelpCommand()
            };
            
            NameCommandDict = BotCommands.ToDictionary(c => c.Name, c => c);
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

        public async Task DoCommand(Update update)
        {
            if (!IsValidUpdateCommand(update)) {
                await _botService.Client.SendTextMessageAsync(
                        update.Message.Chat.Id, $"Unsupported command");
                return;
            }
            var message = update.Message;

            var command = NameCommandDict[message.Text];


            await command.Execute(message, _botService.Client);

        }

        public async Task EchoAsync(Update update)
        {
            if (update.Type != UpdateType.Message)
            {
                return;
            }
            //update.Message.
            var message = update.Message;

            _logger.LogInformation("Received Message from {0}", message.Chat.Id);

            if (message.Type == MessageType.Text)
            {
                // Echo each Message
                await _botService.Client.SendTextMessageAsync(message.Chat.Id, "test: " + message.Text);
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
    }
}
