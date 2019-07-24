using System;
namespace SigneWordBotAspCore.Services
{
    public class AppContext: IAppContext
    {
        public AppContext()
        {
        }

        public string BotToken => "895198692:AAGcsvPQogiNGxexv6rdpMC0XHej6nKJfM0";

        //public string DBConnectionString => "postgres://mttvmhgugxnosn:ebdd285a0c1bd592c7177213be02518c8c623b57a40ac1f5997d097f29f09c93@ec2-54-246-84-100.eu-west-1.compute.amazonaws.com:5432/desihi3olk2r2g";

        //private string Host => "ec2-54-246-84-100.eu-west-1.compute.amazonaws.com";

        private string Host => "localhost";
        private string DataBase => "tg_bot_db";
        private string Port => "5432";
        private string User => "username";
        private string Password => "";

        public string DBConnectionString => $"Server={Host};Port={Port};Database={DataBase};User Id={User};Password={Password};";

    }
}
