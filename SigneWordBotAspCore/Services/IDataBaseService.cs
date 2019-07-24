using System;
namespace SigneWordBotAspCore.Services
{
    public interface IDataBaseService
    {
        int CreateUser(string password, long telegram_id);
        bool CreateCredentials(long telegramId, string credentialsName, string login, string password, string basketName = null);    }
}
