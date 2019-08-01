using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FastMember;
using Npgsql;
using NpgsqlTypes;
using SigneWordBotAspCore.Models;
using SigneWordBotAspCore.Base;
using SigneWordBotAspCore.BotCommands;
using Telegram.Bot.Types;
using TgUser = Telegram.Bot.Types.User;

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
            const string query = "SELECT id, tg_id FROM public.user WHERE tg_id = @tg_id LIMIT 1;";
            var telegramIdParam = new NpgsqlParameter<long>("tg_id", telegramId);

            try
            {
                TryOpenConnection();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.Add(telegramIdParam);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader == null)
                        {
                            return false;
                        }

                        return reader.HasRows;
                    }
                }
            }
            catch (NpgsqlException)
            {
                return false;
            }
            finally
            {
                connection.Close();
            }
        }
        
        public int CreateUser(User tgUser, string password)
        {
            var telegramId = tgUser.Id;
            
            if (IsUserExist(telegramId))
            {
                return -1;
            }
            
            var sqlParams = new NpgsqlParameter[] {
                new NpgsqlParameter<string>(nameof(password), Sha512(password)),
                new NpgsqlParameter<long>("tg_id", telegramId),
                new NpgsqlParameter<string>("first_name", tgUser.FirstName),
                new NpgsqlParameter<string>("second_name", tgUser.LastName),
                new NpgsqlParameter<string>("tg_username", tgUser.Username), 
            };
            

            const string query = @"INSERT INTO public.user (password, first_name, second_name, tg_id, tg_username) 
                             VALUES (@password, @first_name, @second_name, @tg_id, @tg_username) RETURNING id;";

            return Insert(query, sqlParams);
        }

        
        //TODO: validation for name repeating
        public int CreateBasket(long userId, string name, string basketPass = null, string description = null)
        {
            int basketId = -1;
            int relationId = -1;
            
            string cryptedPass = null;
            
            //TODO: need exception here
            if (name.Length > 512)
                return basketId;
            
            //TODO: 1024 chars limit for result pass
            cryptedPass = EncryptString(basketPass);

            var sqlParams = new NpgsqlParameter[]
            {
                new NpgsqlParameter<string>("name", name), 
                new NpgsqlParameter<string>("basket_pass", cryptedPass),
                new NpgsqlParameter<string>("description", description)
                
            };

            const string query = @"INSERT INTO public.passwords_basket 
                            (name, basket_pass, description) 
                            VALUES (@name, @basket_pass, @description) 
                            RETURNING id;";

            TryOpenConnection();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    basketId = Insert(query, sqlParams);

                    if (basketId == -1)
                    {
                        transaction.Rollback();
                        return -1;
                    }

                    var relationsQueryParams = new NpgsqlParameter[]
                    {
                        new NpgsqlParameter<long>("user_id", userId),
                        new NpgsqlParameter<int>("basket_id", basketId),
                        new NpgsqlParameter<int>("access_type_id", (int) AccessType.Owner),
                    };
                    const string relationsInsertQuery = @"INSERT INTO public.user_basket 
                                                    (user_id, basket_id, access_type_id)
                                                    VALUES (@user_id, @basket_id, @access_type_id)
                                                    RETURNING id";

                    relationId = Insert(relationsInsertQuery, relationsQueryParams, transaction);

                    transaction.Commit();
                }
                //FIXME: bad practice
                catch (Exception ex)
                {
                    transaction?.Rollback();
                }
                finally
                {
                    connection.Close();
                }
            }

            return relationId;
        }

        private bool TryOpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        
        private int GetBasketId(string telegramId, string basketName)
        {
            var res = -1;

            


            return res;
        }


        public int CreateCredentials(TgUser tgUser, AddCredsOption credsOption)
        {
            var res = -1;
            var basketName = credsOption.Basket ?? "default";

            //TODO: needs get it
            var basketPassId = -1;
            
            
            
            var sqlParams = new NpgsqlParameter[] {
                new NpgsqlParameter<int>("basket_pass_id", basketPassId),
                new NpgsqlParameter<string>("login", credsOption.Login),
                new NpgsqlParameter<string>("unit_password", EncryptString(credsOption.Password)),
                new NpgsqlParameter<string>("name", credsOption.Title), 
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

            var query = "SELECT id, tg_id FROM public.user;";

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


        
        private int Insert(string query, 
            IEnumerable<NpgsqlParameter> parameters = null,
            NpgsqlTransaction transaction = null)
        {
            var res = -1;
            var connectionIsOpenedByMe = false;
            try
            {

                connectionIsOpenedByMe = TryOpenConnection();
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction))
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
                if (connectionIsOpenedByMe)
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
