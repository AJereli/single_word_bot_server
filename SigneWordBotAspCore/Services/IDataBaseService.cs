using System;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.Services
{
    public interface IDataBaseService
    {
        int CreateUser(User tgUser, string password); 
        int CreateBasket(long userId, string name, string basketPass = null, string description = null);
        
    }
}
