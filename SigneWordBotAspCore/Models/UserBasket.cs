using System;
using System.Collections.Generic;

namespace SigneWordBotAspCore
{
    public partial class UserBasket
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BasketId { get; set; }
        public int AccessTypeId { get; set; }

        public virtual BasketAccessEnum AccessType { get; set; }
        public virtual PasswordsBasketModel BasketModel { get; set; }
        public virtual UserModel UserModel { get; set; }
    }
}
