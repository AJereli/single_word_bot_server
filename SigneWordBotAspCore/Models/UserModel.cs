using NpgsqlTypes;

namespace SigneWordBotAspCore.Models
{
    public class UserModel
    {
        [PgName("id")]
        public int Id { get; set; }
        [PgName("telegram_id")]
        public long TelegramId { get; set; }
    }
}
