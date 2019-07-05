using System;
namespace SigneWordBotAspCore.Services
{
    public interface IDataBaseService
    {
        bool CreateUser(string password, long telegram_id);
    }
}
