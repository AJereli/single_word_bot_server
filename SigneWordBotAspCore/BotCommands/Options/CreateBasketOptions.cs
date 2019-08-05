using CommandLine;

namespace SigneWordBotAspCore.BotCommands.Options
{
    [Verb("/createBasket", HelpText = "Create new basket")]
    public class CreateBasketOptions
    {
        [Option('n', "name", Required = true, HelpText = "Name of the basket")]
        public string Name { get; set; }

        [Option('p', "pass", Required = false, HelpText = "Password for the basket")]
        public string Password { get; set; }

        [Option('d', "desc", Required = false, HelpText = "Descriptions of the basket")]
        public string Description { get; set; }

    }
}