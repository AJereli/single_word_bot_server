using System.Threading.Tasks;
using SigneWordBotAspCore.States;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
    public abstract class AbstractBotCommand
    {

        protected string _name;
        protected UserNextState _nextState;
        
        public string Name => _name;
        public UserNextState NextState => _nextState;
        
        public abstract Task Execute(Message message, TelegramBotClient client);
    }
}