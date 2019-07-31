using System;
namespace SigneWordBotAspCore.Services
{
    public interface IDataBaseService
    {
        int CreateUser(string password, long telegramId); 
        int CreateBasket(long userId, string name, string basketPass = null, string description = null);
        int CreateCredentials(long telegramId, string credentialsName, string login, string password, string basketName = null);    }
}
