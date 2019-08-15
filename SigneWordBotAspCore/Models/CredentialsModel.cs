using System;
using System.Collections.Generic;

namespace SigneWordBotAspCore
{
    public partial class CredentialsModel
    {
        public int Id { get; set; }
        public string UnitPassword { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public int BasketPassId { get; set; }

        public virtual PasswordsBasketModel BasketModelPass { get; set; }
    }
}
