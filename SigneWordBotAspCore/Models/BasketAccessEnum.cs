using System;
using System.Collections.Generic;

namespace SigneWordBotAspCore
{
    public partial class BasketAccessEnum
    {
        public BasketAccessEnum()
        {
            UserBasket = new HashSet<UserBasket>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public virtual ICollection<UserBasket> UserBasket { get; set; }
    }
}
