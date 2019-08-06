using System;
using System.Threading.Tasks;
using CommandLine;
using SigneWordBotAspCore.BotCommands.Options;
using SigneWordBotAspCore.EntitiesToTgResponse;
using SigneWordBotAspCore.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
    public class ShareCommand : AbstractBotCommand
    {
        private readonly IDataBaseService _dataBaseService;

        public ShareCommand(IDataBaseService dataBaseService)
        {
            _dataBaseService = dataBaseService;

            _name = "/share";
        }

        public override async Task Execute(Message message, TelegramBotClient client)
        {
            var commandPart = message.Text.Split(' ');

            var res = Parser.Default.ParseArguments<ShareOptions>(commandPart)
                .WithParsed(async options =>
                {
                    var sharedResult = _dataBaseService.ShareBasket(message.From, options);

                    if (sharedResult.IsSuccess)
                    {
                        await client.SendTextMessageAsync(message.Chat.Id, 
                            $"Basket {options.Name} successful shared");
                        
                    }
                    else
                    {
                        await client.SendTextMessageAsync(message.Chat.Id, "Some thing went wrong");
                    }
                    
                })
                .WithNotParsed(async errors =>
                {
                    var errorResponse = ResponseFormatter.Default.ErrorResponse(errors);
                    if (!string.IsNullOrEmpty(errorResponse))
                    {
                        await client.SendTextMessageAsync(message.Chat.Id, errorResponse);
                    }
                });
        }
    }
}