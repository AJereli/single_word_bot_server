using System;
using System.Threading.Tasks;
using CommandLine;
using SigneWordBotAspCore.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
    [Verb("/show", HelpText = "Show credential")]
    public class ShowCredentialOptions
    {
        [Option('a', "all", Required = false, HelpText = "Show all credentials from all baskets")]
        public bool ShowAll { get; set; }
        
        
        [Option('t', "title", Required = false, HelpText = "Enter title of credentials ")]
        public string Title { get; set; }
        

        [Option('b', "basket", Required = false, HelpText = "Select basket (optional)")]
        public string Basket { get; set; }
    }
    
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