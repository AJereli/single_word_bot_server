using System;
using System.Collections.Generic;

namespace SigneWordBotAspCore
{
    public partial class Note
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public int UserId { get; set; }

        public virtual UserModel UserModel { get; set; }
    }
}
