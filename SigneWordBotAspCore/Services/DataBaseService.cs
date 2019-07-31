using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FastMember;
using Npgsql;
using NpgsqlTypes;
using SigneWordBotAspCore.Models;
using SigneWordBotAspCore.Base;


namespace SigneWordBotAspCore.Services
{

   
    public class DataBaseService : IDataBaseService
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
                GetUsers();
                //connection.Open();
                //CreateUser("password", 1);
                //connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public bool IsUserExist(long telegramId)
        {
            var query = "SELECT id, telegram_id FROM user WHERE telegram_id = @telegram_id;";
            var telegramIdParam = new NpgsqlParameter<long>("telegram_id", telegramId);

            try
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.Add(telegramIdParam);
                    using (var reader = command.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }
            finally
            {
                connection.Close();
            }
        }
        
        public int CreateUser(string password, long telegramId)
        {
            if (IsUserExist(telegramId))
            {
                return -1;
            }
            
            var sqlParams = new NpgsqlParameter[] {
                new NpgsqlParameter(nameof(password), Sha512(password)),
                new NpgsqlParameter("telegram_id", telegramId)
            };

            string query = "INSERT INTO public.user (password, telegram_id) VALUES (@password, @telegram_id) RETURNING id;";

            return Insert(query, sqlParams);
        }

        
        //TODO: validation for name repeating
        public int CreateBasket(long userId, string name, string basketPass = null, string description = null)
        {
            int res = -1;
            string cryptedPass = null;
            
            if (name.Length > 512)
                return res;
            
            //TODO: 1024 chars limit for result pass
            cryptedPass = EncryptString(basketPass);

            var sqlParams = new NpgsqlParameter[]
            {
                new NpgsqlParameter<long>("user_id", userId),
                new NpgsqlParameter<string>("name", name), 
                new NpgsqlParameter<string>("basket_pass", cryptedPass),
                new NpgsqlParameter<string>("description", description)
                
            };

            string query = @"INSERT INTO public.passwords_basket 
                            (user_id, name, basket_pass, description) 
                            VALUES (@user_id, @name, @basket_pass, @description) 
                            RETURNING id;";

            res = Insert(query, sqlParams);

            return res;
        }

        private int GetBasketId(string telegramId, string basketName)
        {
            var res = -1;

            


            return res;
        }


        public int CreateCredentials(long telegramId, string credentialsName, string login, string password, string basketName = null)
        {
            var res = -1;

            if (!string.IsNullOrEmpty(basketName))
            {
                
            }
            
            

            var sqlParams = new[] {
                new NpgsqlParameter("telegram_id", telegramId),
                new NpgsqlParameter("login", login),
                new NpgsqlParameter("unit_password", EncryptString(password)),
            };


            string query = @"INSERT INTO public.credentials 
                             (unit_password, name, login, basket_pass_id) 
                             VALUES (@password, @telegram_id) 
                             RETURNING id;";


            return res;
        }


        public List<UserModel> GetUsers()
        {
            var users = new List<UserModel>();

            var query = "SELECT id, telegram_id FROM public.user;";

            try
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var user = dataReader.ConvertToObject<UserModel>();
                            users.Add(user);
                        }

                    }
                }
            }
            catch (NpgsqlException npgEx)
            {
                Console.WriteLine(npgEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                connection.Close();
            }

            return users;
        }


        
        private int Insert(string query, IEnumerable<NpgsqlParameter> parameters = null)
        {
            var res = -1;

            try
            {

                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters.ToArray());

                    var rawResult = command.ExecuteScalar();
                    if (rawResult is int result)
                        res = result;
                }
            }
            catch (NpgsqlException npgEx)
            {
                Console.WriteLine(npgEx);
            }
            finally
            {
                connection.Close();
            }

            return res;
        }

        //TODO: impl this
        /// <summary>
        /// May return null!
        /// </summary>
        /// <param name="input">String that needs to encode</param>
        /// <returns>result of encryption</returns>
        private string EncryptString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            return input;
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
