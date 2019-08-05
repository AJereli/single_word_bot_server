using CommandLine;

namespace SigneWordBotAspCore.BotCommands.Options
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
}