using System;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SigneWordBotAspCore.BotCommands.Options;
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
            // _nextState = UserNextState.WaitCredentials;
        }

        public override async Task Execute(Message message, TelegramBotClient client)
        {
            var commandPart = message.Text.Split(' ');

            var res = Parser.Default.ParseArguments<AddCredsOption>(commandPart)
                .WithParsed(async credentialOptions =>
                {
                    var result =_dataBaseService.CreateCredentials(message.From, credentialOptions);
                    
                    if (result != -1)
                        await client.SendTextMessageAsync(message.Chat.Id,
                            "Credentials was created",
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Default);
                    else
                    {
                        await client.SendTextMessageAsync(message.Chat.Id, "чот сломалось");
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