using System;
using System.Data.SqlClient;
using Npgsql;

namespace SigneWordBotAspCore.Services
{
    public class DataBaseService: IDataBaseService
    {
        private readonly IAppContext appContext;

        private readonly NpgsqlConnection connection;

        public DataBaseService(IAppContext appContext)
        {
            this.appContext = appContext;
            Console.WriteLine(appContext.DBConnectionString);
            connection = new NpgsqlConnection(appContext.DBConnectionString);
            try
            {
                connection.Open();
                connection.Close();
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
         

        }

        public bool CreateUser (string password, long telegram_id)
        {
            var sqlParams = new NpgsqlParameter[] {
                new NpgsqlParameter(nameof(password), Sha512(password)),
                new NpgsqlParameter(nameof(telegram_id), telegram_id)
            };

            string query = "INSERT INTO public.user (password, telegram_id) VALUES (@password, @telegram_id);";

            return Insert(query, sqlParams);
        }


        private bool Insert (string query, NpgsqlParameter[] parameters = null)
        {
            var res = false;

            connection.Open();
            using (NpgsqlCommand command = new NpgsqlCommand("", connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                res = command.ExecuteNonQuery() != -1;
            }

            connection.Close();
            return res;
        }

        private string Sha512(string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            using (var hash = System.Security.Cryptography.SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);

                // Convert to text
                // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
                var hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString();
            }
        }

    }
}
