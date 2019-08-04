using System;
using NpgsqlTypes;

namespace SigneWordBotAspCore.Models
{
    public class CredentialsModel
    {
        [PgName("id")]
        public int Id { get; set; }
        [PgName("unit_password")]
        public string UnitPassword { get; set; }
        [PgName("name")]
        public string Name { get; set; }
        [PgName("login")]
        public string Login { get; set; }
        public int BasketPassId { get; set; }
        
        [PgName("basket_name")]
        public string BasketName { get; set; }
    }
}
