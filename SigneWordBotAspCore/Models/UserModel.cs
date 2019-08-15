using System;
using System.Collections.Generic;

namespace SigneWordBotAspCore
{
    public partial class UserModel
    {
        public UserModel()
        {
            Note = new HashSet<Note>();
            UserBasket = new HashSet<UserBasket>();
        }

        public int Id { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public long TgId { get; set; }
        public string TgUsername { get; set; }

        public virtual ICollection<Note> Note { get; set; }
        public virtual ICollection<UserBasket> UserBasket { get; set; }
    }
}
