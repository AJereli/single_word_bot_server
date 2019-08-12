using System;
using System.Collections;
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
    public class DataBaseService : BaseDataBaseService, IDataBaseService
    {
        public DataBaseService(IAppContext appContext) : base(appContext)
        {
        }


        public int CreateUser(User tgUser, string password)
        {
            var telegramId = tgUser.Id;

            if (IsUserExist(telegramId))
            {
                return -1;
            }

            var sqlParams = new NpgsqlParameter[]
            {
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


        private ShareException IsSharingPossible(IDictionary<string, object> basketInfo, UserModel shareUser)
        {
            if (basketInfo == null)
                return new ShareException {ExceptionType = ShareExceptionType.Other};

            if (shareUser == null)
                return new ShareException {ExceptionType = ShareExceptionType.NoUser};

            var basketId = (int) basketInfo["basket_id"];

            if (basketId == -1)
                return new ShareException {ExceptionType = ShareExceptionType.NoBasket};

            if ((int) basketInfo["access_type_id"] != (int) AccessType.Owner)
                return new ShareException {ExceptionType = ShareExceptionType.NoAccess};

            if (shareUser == null)
            {
                throw new ShareException {ExceptionType = ShareExceptionType.NoUser};
            }

            return null;
        }

        public bool UnShareBasket(TgUser user, ShareOptions shareOptions)
        {
            var basketInfo = GetBasketInfo(user.Id, shareOptions.Name);
            var userInfo = GetUser(user.Username); 
            var sharedUser = GetUser(shareOptions.UserName);
            
            var shareException = IsSharingPossible(basketInfo, userInfo);
            
            if (shareException != null)
                throw shareException;


            const string query = @"DELETE FROM public.user_basket 
                                    WHERE user_id = @userId AND basket_id = @basketId";
           
            NpgsqlParameter[] sqlParams =
            {
                new NpgsqlParameter<int>("userId", sharedUser.Id),
                new NpgsqlParameter<int>("basketId", (int)basketInfo["basket_id"]),
            };
            
            
            var result = DeleteUpdate(query, sqlParams) == 1;
            

            return result;

        }

        private IDictionary<string, object> GetBasketInfo(long userId, string basketName, NpgsqlTransaction transaction = null)
        {
            
            const string basketIdQuery =
                @"SELECT ub.basket_id AS basket_id, access_type_id FROM passwords_basket pb 
                                                        JOIN user_basket ub ON pb.id = ub.basket_id
                                                        JOIN public.user u ON u.id = ub.user_id WHERE u.tg_id = @tg_id AND LOWER(pb.name) = @basketName LIMIT 1;";

            NpgsqlParameter[] basketIdParams =
            {
                new NpgsqlParameter<long>("tg_id", userId),
                new NpgsqlParameter<string>("basketName", basketName.ToLower()),
            };

            return SelectOneRaw(basketIdQuery, basketIdParams, transaction);
        }

        private UserModel GetUser(string tgName, NpgsqlTransaction transaction = null)
        {
            const string userTgIdQuery =
                "SELECT * FROM public.user AS u WHERE LOWER(u.tg_username) = @userName;";
            var userTgIdQueryParams = new NpgsqlParameter[]
            {
                new NpgsqlParameter<string>("userName", tgName)
            };

            return SelectOne<UserModel>(userTgIdQuery, userTgIdQueryParams, transaction);
        }
        
        public ShareResult ShareBasket(TgUser user, ShareOptions shareOptions)
        {
            int result = -1;
            UserModel sharedUser = null;
            var isConnOpenByMe = TryOpenConnection();
            
            using (var transaction = _connection.BeginTransaction())
            {
                try
                {

                    var basketInfo = GetBasketInfo(user.Id, shareOptions.Name, transaction);
                    sharedUser = GetUser(shareOptions.UserName, transaction);

                    var checkException = IsSharingPossible(basketInfo, sharedUser);
                    if (checkException != null) throw checkException;

                    var basketId = (int) basketInfo["basket_id"];

                    const string insertQuery = @"INSERT INTO user_basket AS ub (user_id, basket_id, access_type_id) 
                                    VALUES 
                                    (@shareUserTgId,
                                    @basketId,
                                    @accessType) RETURNING id;";

                    NpgsqlParameter[] insertQueryParams =
                    {
                        new NpgsqlParameter<long>("shareUserTgId", sharedUser.Id),
                        new NpgsqlParameter<int>("basketId", basketId),
                        new NpgsqlParameter<string>("userName", shareOptions.UserName),
                        new NpgsqlParameter<int>("accessType",
                            (int) (shareOptions.WritePermission ? AccessType.SharedReadWrite : AccessType.SharedRead)),
                    };


                    result = Insert(insertQuery, insertQueryParams, transaction);

                    transaction.Commit();
                }
                catch (Exception)
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

            if (result != -1 && sharedUser != null)
            {
                sharedResult.IsSuccess = true;
                sharedResult.SharedUserTgId = sharedUser.TelegramId;
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
                sqlParams.Add(new NpgsqlParameter<string>("name", $"%{title}%"));

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


        private IDictionary<string, object> GetBasketId(long telegramId, string basketName)
        {
            var sqlParams = new NpgsqlParameter[]
            {
                new NpgsqlParameter<long>("tg_id", telegramId),
                new NpgsqlParameter<string>("basket_name", basketName),
            };

            const string query = @"SELECT pb.id AS basket_id, access_type_id FROM public.passwords_basket AS pb 
                                    JOIN public.user_basket AS ub ON pb.id = ub.basket_id 
                                    JOIN public.user as u ON u.id = ub.user_id 
                                    WHERE u.tg_id = @tg_id AND pb.name = @basket_name;";

            return SelectOneRaw(query, sqlParams);
        }

        public int CreateCredentials(TgUser tgUser, AddCredsOption credsOption)
        {
            var basketName = credsOption.Basket ?? "default";

            var basketPassInfo = GetBasketId(tgUser.Id, basketName);

            if (basketPassInfo == null || basketPassInfo.Count < 2)
                return -1;

            if ((int) basketPassInfo["access_type_id"] == (int) AccessType.SharedRead)
            {
                throw new BasketOperationNotAllowed();
            }

            var sqlParams = new NpgsqlParameter[]
            {
                new NpgsqlParameter<int>("basket_pass_id", (int) basketPassInfo["basket_id"]),
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

    }
}