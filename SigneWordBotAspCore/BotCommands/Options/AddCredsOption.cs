using CommandLine;

namespace SigneWordBotAspCore.BotCommands.Options
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
}