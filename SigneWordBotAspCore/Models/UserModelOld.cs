using System.Collections.Generic;
using NpgsqlTypes;

namespace SigneWordBotAspCore.Models
{
    public class UserModelOld
    {
        [PgName("id")]
        public int Id { get; set; }
        
        [PgName("tg_id")]
        public long TelegramId { get; set; }
        
        [PgName("first_name")]
        public string FirstName { get; set; }
        
        [PgName("second_name")]
        public string SecondName { get; set; }
        
        [PgName("tg_username")]
        public string Username { get; set; }
        
        public ICollection<UserBasket> UserBaskets { get; set; }

    }
}
