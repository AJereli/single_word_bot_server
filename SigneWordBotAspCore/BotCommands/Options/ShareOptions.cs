using CommandLine;

namespace SigneWordBotAspCore.BotCommands.Options
{
    [Verb("/share", HelpText = "Share basket with other users")]

    public class ShareOptions
    {
        [Option('n', "name", Required = true, HelpText = "Name of the basket")]
        public string Name { get; set; }
        
        [Option('u', "user", Required = true)]
        public string UserName { get; set; }
        
        [Option('w',"write", Required = false)]
        public bool WritePermission { get; set; }
    }
}