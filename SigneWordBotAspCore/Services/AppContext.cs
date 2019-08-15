using System;


namespace SigneWordBotAspCore.Services
{
    public class AppContext : IAppContext
    {
        private readonly string _host;
        private readonly string _dbName;
        private readonly string _dbUser;
        private readonly string _dbPass;
        private readonly string _botToken;
        private readonly string _port = "5432";

        public AppContext()
        {
#if DEBUG
            _host = "localhost";
            _dbUser = "username";
            _dbPass = "";
            _dbName = "signle_word_db";
            var json = System.IO.File.ReadAllText("bot_token.json");
            var dec = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            var obj = Newtonsoft.Json.Linq.JObject.Parse(dec.ToString());
            _botToken = obj["bot_token"].ToString();
#else
            _host = Environment.GetEnvironmentVariable("pg_host");
            _dbUser = Environment.GetEnvironmentVariable("pg_user");
            _dbPass = Environment.GetEnvironmentVariable("pg_pass");
            _dbName = Environment.GetEnvironmentVariable("pg_db");
            _botToken = Environment.GetEnvironmentVariable("bot_token");
#endif
        }

        public string BotToken => _botToken;


//dotnet ef dbcontext scaffold "Server=localhost;Port=5432;Database=signle_word_db;User Id=username;Password=;" Npgsql.EntityFrameworkCore.PostgreSQL


        public string DBConnectionString =>
            $"Server={_host};Port={_port};Database={_dbName};User Id={_dbUser};Password={_dbPass};";
    }
}