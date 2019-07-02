using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.Services
{
    public interface IUpdateService
    {
        Task EchoAsync(Update update);
    }
}
