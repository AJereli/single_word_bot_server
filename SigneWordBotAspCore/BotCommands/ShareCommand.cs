using System;
using System.Threading.Tasks;
using CommandLine;
using SigneWordBotAspCore.BotCommands.Options;
using SigneWordBotAspCore.EntitiesToTgResponse;
using SigneWordBotAspCore.Exceptions;
using SigneWordBotAspCore.Services;
using SigneWordBotAspCore.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
                    options.UserName = options.UserName.CheckAndRemove('@').ToLower();

                    try
                    {
                        var sharedResult = _dataBaseService.ShareBasket(message.From, options);

                        if (sharedResult.IsSuccess)
                        {
                            await client.SendTextMessageAsync(message.Chat.Id,
                                $"Basket {options.Name} successful shared");

                            await client.SendTextMessageAsync(sharedResult.SharedUserTgId,
                                $"User {message.From.Username} just share credentials basket '{options.Name}' with you\n" +
                                $"user /show -b {options.Name} for show");
                        }
                        else
                        {
                            await client.SendTextMessageAsync(message.Chat.Id, "Some thing went wrong");
                        }
                    }
                    catch (ShareException se)
                    {
                        var response = "Something went wrong";
                        switch (se.ExceptionType)
                        {
                            case ShareExceptionType.NoBasket:
                                response = $"{se.Message}\nCant find basket with this name :(";
                                break;
                            case ShareExceptionType.NoUser:
                                response = $"{se.Message}\nCant find such user.\n" +
                                           "Maybe he has not worked with our bot yet?\n" +
                                           "You can invite him by the link: t.me/SingleWordBot";
                                break;
                            case ShareExceptionType.NoAccess:
                                response = $"{se.Message}\nYou can share this basket, because you are not basket owner";
                                break;
                            default:
                                break;
                                ;
                                
                        }
                        
                        await client.SendTextMessageAsync(message.Chat.Id,
                            response, ParseMode.Html);
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