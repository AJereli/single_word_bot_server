using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using SigneWordBotAspCore.Models;
using SigneWordBotAspCore.Utils;
using SigneWordBotAspCore.BotCommands;
using SigneWordBotAspCore.BotCommands.Options;
using SigneWordBotAspCore.Exceptions;
using Telegram.Bot.Types;
using TgUser = Telegram.Bot.Types.User;

namespace SigneWordBotAspCore.Services
{

    public class DataBaseService : IDataBaseService
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly IAppContext _appContext;

        private readonly NpgsqlConnection _connection;

        public DataBaseService(IAppContext appContext)
        {
            _appContext = appContext;

            Console.WriteLine(appContext.DBConnectionString);
            _connection = new NpgsqlConnection(appContext.DBConnectionString);
            try
            {
                //connection.Open();
                //CreateUser("password", 1);
                //connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                new NpgsqlParameter<string>(nameof(password), Crypting.Sha512(password)),
                new NpgsqlParameter<long>("tg_id", telegramId),
                new NpgsqlParameter<string>("first_name", tgUser.FirstName),
                new NpgsqlParameter<string>("second_name", tgUser.LastName),
                new NpgsqlParameter<string>("tg_username", tgUser.Username), 
            };
            

            const string query = @"INSERT INTO public.user (password, first_name, second_name, tg_id, tg_username) 
                             VALUES (@password, @first_name, @second_name, @tg_id, @tg_username) RETURNING id;";

            return Insert(query, sqlParams);
        }



        public ShareResult ShareBasket(TgUser user, ShareOptions shareOptions)
        {
            int result = -1;
            long tgId = -1;

            var isConnOpenByMe = TryOpenConnection();

            
            
            using (var transaction = _connection.BeginTransaction())
            {
                try
                {
                    
                    const string basketIdQuery = @"SELECT ub.basket_id FROM passwords_basket pb 
    JOIN user_basket ub ON pb.id = ub.basket_id
    JOIN public.'user' u ON u.id = ub.user_id WHERE u.tg_id = @tg_id AND pb.name = @basketName LIMIT 1;";

                    NpgsqlParameter[] basketIdParams =
                    {
                        new NpgsqlParameter<long>("tg_id", user.Id),
                        new NpgsqlParameter<string>("basketName", shareOptions.Name),
                    };

                    var basketId = SelectOne<int>(basketIdQuery, basketIdParams, transaction);
                    
                    
                    
                    const string insertQuery = @"INSERT INTO user_basket AS ub (user_id, basket_id, access_type_id) 
                                    VALUES 
                                    ((SELECT id FROM public.user AS u WHERE u.tg_username = @userName LIMIT 1),
                                    @basketId,
                                    @accessType) RETURNING id;";
                                        
                    NpgsqlParameter[] insertQueryParams =
                    {
                        new NpgsqlParameter<int>("basketId", basketId),
                        new NpgsqlParameter<string>("userName", shareOptions.UserName),
                        new NpgsqlParameter<int>("accessType", 
                            (int)(shareOptions.WritePermission ? AccessType.SharedReadWrite : AccessType.SharedRead)), 
                    };


                    result = Insert(insertQuery, insertQueryParams, transaction);



                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
                finally
                {
                    if (isConnOpenByMe)
                        _connection.Close();
                }
            }

            var sharedResult = new ShareResult();


            if (result != -1)
            {
                sharedResult.IsSuccess = true;
//                sharedResult.SharedUserTgId 
            }
            return sharedResult;
        }
        
        

        public IEnumerable<CredentialsModel> GetCredentials(TgUser user, ShowCredentialOptions credentialOptions)
        {
            IEnumerable<CredentialsModel> result;
            
            var basketName = credentialOptions.Basket;
            basketName = string.IsNullOrEmpty(basketName) ? "default" : basketName;


            IList<NpgsqlParameter> sqlParams = new List<NpgsqlParameter>();
            sqlParams.Add(new NpgsqlParameter<long>("tg_id", user.Id));

            var query = @"SELECT c.name, c.login, c.unit_password, passwords_basket.name as basket_name
                                FROM passwords_basket
                                JOIN user_basket as ub ON passwords_basket.id = ub.basket_id
                                JOIN public.user u on ub.user_id = u.id
                                JOIN credentials c on passwords_basket.id = c.basket_pass_id
                                WHERE tg_id = @tg_id ";

            if (credentialOptions.ShowAll)
            {
                query += ";";
            }
            else if (!string.IsNullOrEmpty(credentialOptions.Title))
            {
                var title = credentialOptions.Title.ToLower();
                sqlParams.Add(new NpgsqlParameter<string>("name", $"%{title}%")  );

                query += "AND LOWER(c.name) LIKE @name;";
            }
            else
            {
                sqlParams.Add(new NpgsqlParameter<string>("passwords_basket.name", basketName.ToLower()));
                query += "AND LOWER(passwords_basket.name) = @passwords_basket.name;";
            }
            result = SelectMany<CredentialsModel>(query, sqlParams);


            
            return result;
        }
        
        
        
        /// <summary>
        /// Create Basket and relations in many-to-many table
        /// </summary>
        /// <param name="user"></param>
        /// <param name="name"></param>
        /// <param name="basketPass"></param>
        /// <param name="description"></param>
        /// <returns>Return id of new relation row</returns>
        public int CreateBasket(TgUser user, string name, string basketPass = null, string description = null)
        {
            var userId = GetUserId(user.Id);

            return CreateBasket(userId, name, basketPass, description);
        }

        /// <summary>
        /// Create Basket and relations in many-to-many table
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <param name="basketPass"></param>
        /// <param name="description"></param>
        /// <returns>Return id of new relation row</returns>
        public int CreateBasket(long userId, string name, string basketPass = null, string description = null)
        {
            int basketId = -1;
            int relationId = -1;

            if (name.Length > 512)
                throw new BasketAlreadyExistException();

            if (IsBasketExist(userId, name))
                return -1;
            
            
            //TODO: 1024 chars limit for result pass
            var cryptedPass = Crypting.EncryptString(basketPass);

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
            using (var transaction = _connection.BeginTransaction())
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
                //TODO: bad practice
                catch (Exception)
                {
                    transaction?.Rollback();
                }
                finally
                {
                    _connection.Close();
                }
            }

            return relationId;
        }


        private int GetBasketId(long telegramId, string basketName)
        {

            var sqlParams = new NpgsqlParameter[] {
                new NpgsqlParameter<long>("tg_id", telegramId),
                new NpgsqlParameter<string>("basket_name", basketName),
            };
            
            const string query = @"SELECT pb.id AS basket_id FROM public.passwords_basket AS pb 
                                    JOIN public.user_basket AS ub ON pb.id = ub.basket_id 
                                    JOIN public.user as u ON u.id = ub.user_id 
                                    WHERE u.tg_id = @tg_id AND pb.name = @basket_name;";

            return SelectOne<int>(query, sqlParams);
        }
        
        public int CreateCredentials(TgUser tgUser, AddCredsOption credsOption)
        {
            var basketName = credsOption.Basket ?? "default";

            var basketPassId = GetBasketId(tgUser.Id, basketName);

            var sqlParams = new NpgsqlParameter[] {
                new NpgsqlParameter<int>("basket_pass_id", basketPassId),
                new NpgsqlParameter<string>("login", credsOption.Login),
                new NpgsqlParameter<string>("unit_password", Crypting.EncryptString(credsOption.Password)),
                new NpgsqlParameter<string>("name", credsOption.Title), 
            };


            const string query = @"INSERT INTO public.credentials 
                             (unit_password, name, login, basket_pass_id) 
                             VALUES (@unit_password, @name, @login, @basket_pass_id) 
                             RETURNING id;";

            return Insert(query, sqlParams);
        }


        private IEnumerable<T> SelectMany<T>(string query, IEnumerable<NpgsqlParameter> sqlParams,
            NpgsqlTransaction transaction = null)
        {
            IList<T> res = new List<T>();

            var connectionOpenedByMe = false;
            try
            {
                connectionOpenedByMe = TryOpenConnection();

                
                using (var command = new NpgsqlCommand(query, _connection))
                {
                    if (sqlParams != null)
                    {
                        command.Parameters.AddRange(sqlParams.ToArray());
                    }
                    
                    using (var dataReader = command.ExecuteReader())
                    {
                        
                        while (dataReader.Read())
                        {
                            var t = dataReader.ConvertToObject<T>();
                            res.Add(t);
                        }

                    }
                }
            }
            catch (Exception)
            {
                transaction?.Rollback();
            }
            finally
            {
                if (connectionOpenedByMe)
                    _connection.Close();
            }

            return res;
        }

        private int GetUserId(string name, NpgsqlTransaction transaction = null)
        {
            const string query = "SELECT id FROM public.user AS u WHERE u.tg_username = @user;";
            NpgsqlParameter[] sqlParams = {
                new NpgsqlParameter<string>("user", name), 
            };
            return SelectOne<int>(query, sqlParams, transaction);
        }
        
        
        private int GetUserId(long telegramId)
        {
            const string query = "SELECT id FROM public.user WHERE tg_id = @tg_id LIMIT 1;";
            var telegramIdParam = new NpgsqlParameter<long>("tg_id", telegramId);

            return SelectOne<int>(query, new[] {telegramIdParam});
        }
        
        private bool IsUserExist(long telegramId)
        {
            const string query = "SELECT exists(SELECT 1 FROM public.user WHERE tg_id=@tg_id);";
            var telegramIdParam = new NpgsqlParameter<long>("tg_id", telegramId);

            var isConnectionOpenedByMe = false;
            try
            {
                isConnectionOpenedByMe = TryOpenConnection();

                using (var command = new NpgsqlCommand(query, _connection))
                {
                    command.Parameters.Add(telegramIdParam);

                    var isExists = command.ExecuteScalar();
                    if (isExists == null)
                        return false;
                    else return (bool) isExists;

                }
            }
            catch (NpgsqlException)
            {
                return false;
            }
            finally
            {
                if (isConnectionOpenedByMe)
                    _connection.Close();
            }
        }

        private bool IsBasketExist(long userId, string basketName)
        {
            const string query = @"SELECT exists(SELECT ub.user_id as user_id, pb.id, name FROM user_basket AS ub 
                                                    JOIN passwords_basket pb ON pb.id = ub.basket_id 
                                                    WHERE pb.name = @name
                                                      AND ub.user_id = @user_id
                                                      AND ub.access_type_id = 1)";
            
            var sqlParams = new NpgsqlParameter[]
            {
                new NpgsqlParameter<string>("name", basketName),
                new NpgsqlParameter<long>("user_id", userId), 
            };

            return SelectOne<bool>(query, sqlParams);

        }
        
        
        public List<UserModel> GetUsers()
        {
            var users = new List<UserModel>();

            var query = "SELECT id, tg_id FROM public.user;";

            try
            {
                _connection.Open();
                using (var command = new NpgsqlCommand(query, _connection))
                {
                    using (var dataReader = command.ExecuteReader())
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
                _connection.Close();
            }

            return users;
        }


        private T SelectOne<T>(string query, 
            IEnumerable<NpgsqlParameter> parameters = null,
            NpgsqlTransaction transaction = null)
        {
            var res = default(T);
            
            var connectionIsOpenedByMe = false;
            
            try
            {
                connectionIsOpenedByMe = TryOpenConnection();
                using (var command = new NpgsqlCommand(query, _connection, transaction))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters.ToArray());

                    using (var dataReader = command.ExecuteReader())
                    {
                        if (dataReader.HasRows)
                        {
                            dataReader.Read();
                            var readResult = dataReader.ConvertToObject<T>();
                            
                            res = readResult;
                           
                        }
                    }
                }
            }
            catch (NpgsqlException npgEx)
            {
                res = default(T);
                Console.WriteLine(npgEx);
            }
            finally
            {
                if (connectionIsOpenedByMe)
                    _connection.Close();
            }

            return res;
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
                using (var command = new NpgsqlCommand(query, _connection, transaction))
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
                    _connection.Close();
            }

            return res;
        }
        private bool TryOpenConnection()   
        {                                  
            try                            
            {                              
                _connection.Open();         
                return true;               
            }                              
            catch (Exception)            
            {                              
                return false;              
            }                              
        }

    }
}
