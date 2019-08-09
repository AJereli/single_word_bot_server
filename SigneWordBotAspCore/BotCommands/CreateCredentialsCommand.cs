using System;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SigneWordBotAspCore.BotCommands.Options;
using SigneWordBotAspCore.Exceptions;
using SigneWordBotAspCore.Services;
using SigneWordBotAspCore.States;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
    public class CreateCredentialsCommand : AbstractBotCommand
    {
        private readonly IDataBaseService _dataBaseService;

        public CreateCredentialsCommand(IDataBaseService dataBaseService)
        {
            _name = "/addCredentials";
            _dataBaseService = dataBaseService;

//            CommandLine.Par

            // _nextState = UserNextState.WaitCredentials;
        }

        public override async Task Execute(Message message, TelegramBotClient client)
        {
            var commandPart = message.Text.Split(' ');

            var res = Parser.Default.ParseArguments<AddCredsOption>(commandPart)
                .WithParsed(async credentialOptions =>
                {
                    var responseMessage = "чот сломалось";

                    try
                    {
                        var result = _dataBaseService.CreateCredentials(message.From, credentialOptions);
                        if (result != -1)
                        {
                            responseMessage = "Credentials was created";
                        }
                    }
                    catch (BasketAlreadyExistException e)
                    {
                        responseMessage = e.Message;
                    }
                    finally
                    {
                        await client.SendTextMessageAsync(message.Chat.Id,
                            responseMessage,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Default);
                    }
                })
                .WithNotParsed(errors =>
                {
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"{error.GetType()}");
                        if (error is MissingRequiredOptionError typedError)
                        {
                            Console.WriteLine(typedError.NameInfo.NameText);
                        }
                    }
                });
        }
    }
}