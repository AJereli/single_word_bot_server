using System;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SigneWordBotAspCore.Services;
using SigneWordBotAspCore.States;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{

    [Verb("/addCredentials", HelpText = "Add new pair of credentials in basket")]
    public class AddCredsOption
    {
        [Option('p', "password", Required = true, HelpText = "Set pass")]
        public string Password { get; set; }

        [Option('l', "login", Required = true, HelpText = "Set login")]
        public string Login { get; set; }

        
        [Option('t', "title", Required = true, HelpText = "Name or short description about this credentials")]
        public string Title { get; set; }
        

        [Option('b', "basket", Required = false, HelpText = "Select basket (optional)")]
        public string Basket { get; set; }
    }

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
