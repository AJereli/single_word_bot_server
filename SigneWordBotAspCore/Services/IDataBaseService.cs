using System.Collections.Generic;
using SigneWordBotAspCore.BotCommands;
using SigneWordBotAspCore.BotCommands.Options;
using SigneWordBotAspCore.Models;
using Telegram.Bot.Types;
using TgUser = Telegram.Bot.Types.User;

namespace SigneWordBotAspCore.Services
{
    public interface IDataBaseService
    {
        int CreateUser(User tgUser, string password);
        
        int CreateBasket(TgUser user, string name, string basketPass = null, string description = null);

        
        int CreateBasket(long userId, string name, string basketPass = null, string description = null);
        int CreateCredentials(TgUser tgUser, AddCredsOption credsOption);
        IEnumerable<CredentialsModel> GetCredentials(TgUser user, ShowCredentialOptions credentialOptions);
        ShareResult ShareBasket(TgUser user, ShareOptions shareOptions);
    }
}
