using System;
using System.Threading.Tasks;
using CommandLine;
using SigneWordBotAspCore.BotCommands.Options;
using SigneWordBotAspCore.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
   
    
    internal class ShowCommand: AbstractBotCommand
    {
        private readonly IDataBaseService _dataBaseService;

        public ShowCommand(IDataBaseService dataBaseService)
        {
            _dataBaseService = dataBaseService;

            _name = "/show";
        }
        
        public override async Task Execute(Message message, TelegramBotClient client)
        {
            var commandPart = message.Text.Split(' ');
            
            var res = Parser.Default.ParseArguments<ShowCredentialOptions>(commandPart)
                .WithParsed(async credentialOptions =>
                {
                    var credentials = _dataBaseService.GetCredentials(message.From, credentialOptions);
                    
                    //TODO: make difference text
                    await client.SendTextMessageAsync(message.Chat.Id,
                        $@"---Credentials from default basket---",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Default);
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