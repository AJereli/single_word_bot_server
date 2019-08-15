using System;
using System.Collections.Generic;

namespace SigneWordBotAspCore
{
    public partial class PasswordsBasketModel
    {
        public PasswordsBasketModel()
        {
            Credentials = new HashSet<CredentialsModel>();
            UserBasket = new HashSet<UserBasket>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string BasketPass { get; set; }
        public string Description { get; set; }

        public virtual ICollection<CredentialsModel> Credentials { get; set; }
        public virtual ICollection<UserBasket> UserBasket { get; set; }
    }
}
