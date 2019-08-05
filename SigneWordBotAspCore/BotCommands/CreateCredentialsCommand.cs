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

    public class CreateCredentialsCommand: AbstractBotCommand
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
            var chatId = message.Chat.Id;

            var commandPart = message.Text.Split(' ');
            
            var res = Parser.Default.ParseArguments<AddCredsOption>(commandPart)
                .WithParsed(credentialOptions =>
                    {
                        _dataBaseService.CreateCredentials(message.From, credentialOptions);
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
            
            try
            {
#if DEBUG
//                Console.WriteLine("Enter general password for your main basket of passwords.");
            }
            finally
            {
            }
#else
                await client.SendTextMessageAsync(chatId,
               "Enter password general password for your main basket of passwords.",
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Default);
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
#endif
        }

      
    }
}
