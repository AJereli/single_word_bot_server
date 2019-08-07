using System;
using System.Threading.Tasks;
using CommandLine;
using SigneWordBotAspCore.BotCommands.Options;
using SigneWordBotAspCore.EntitiesToTgResponse;
using SigneWordBotAspCore.Exceptions;
using SigneWordBotAspCore.Services;
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
                        switch (se.ExceptionType)
                        {
                            case ShareExceptionType.NoBasket:
                                await client.SendTextMessageAsync(message.Chat.Id,
                                    $"{se.Message}\nCant find basket with this name :(");
                                break;
                            case ShareExceptionType.NoUser:
                                await client.SendTextMessageAsync(message.Chat.Id,
                                    $"{se.Message}\nCant find such user.\n" +
                                    "Maybe he didn’t work with our bot?\n" +
                                    "Everybody may join by the link: t.me/SingleWordBot", ParseMode.Html);
                                break;
                            default:
                                break;
                                ;
                        }
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