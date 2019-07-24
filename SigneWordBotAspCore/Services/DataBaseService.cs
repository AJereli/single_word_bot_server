using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FastMember;
using Npgsql;
using NpgsqlTypes;
using SigneWordBotAspCore.Models;

namespace SigneWordBotAspCore.Services
{

    static class NpgsqlDataReaderExtentions
    {
        public static T ConvertToObject<T>(this NpgsqlDataReader rd) where T : class, new()
        {
            Type type = typeof(T);
            var accessor = TypeAccessor.Create(type);
            var members = accessor.GetMembers();
            var t = new T();

            //IDictionary<string, string> fieldsNameMap = new Dictionary<string, string>();

            for (int i = 0; i < rd.FieldCount; i++)
            {
                if (!rd.IsDBNull(i))
                {
                    string fieldName = rd.GetName(i);

                    //PgNameAttribute

                    //var fieldsNameMap = members.Select(m => {
                    //    var propertyAttr = m.GetAttribute(typeof(PgNameAttribute), false) as PgNameAttribute;
                    //    var propertyName = propertyAttr != null ? propertyAttr.PgName : m.Name;
                    //    return new Tuple<string, string>(fieldName, propertyName);
                    //}).ToDictionary(x => x.Item1, x => x.Item2);


                    var firstMember = members.FirstOrDefault(m => {
                        var propertyAttr = m.GetAttribute(typeof(PgNameAttribute), false) as PgNameAttribute;
                        var propertyName = propertyAttr != null ? propertyAttr.PgName : m.Name;

                        return string.Equals(fieldName, propertyName, StringComparison.OrdinalIgnoreCase);
                    });
                    if (firstMember != null)
                    {
                        try
                        {
                           
                            //var test = fieldsNameMap[fieldName];
                            accessor[t, firstMember.Name] = rd.GetValue(i);

                        }catch (Exception ex)
                        {
                            System.Console.WriteLine(ex);
                        }
                    }
                }
            }

            return t;
        }
    }

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

        public int CreateUser(string password, long telegram_id)
        {
            var sqlParams = new NpgsqlParameter[] {
                new NpgsqlParameter(nameof(password), Sha512(password)),
                new NpgsqlParameter(nameof(telegram_id), telegram_id)
            };

            string query = "INSERT INTO public.user (password, telegram_id) VALUES (@password, @telegram_id) RETURNING id;";

            return Insert(query, sqlParams);
        }


        public int CreateBasket()
        {
            int res = -1;


            return res;
        }




        public bool CreateCredentials(long telegramId, string credentialsName, string login, string password, string basketName = null)
        {
            var res = false;



            var sqlParams = new[] {
                new NpgsqlParameter("telegram_id", telegramId),
                new NpgsqlParameter("login", login),
                new NpgsqlParameter("unit_password", EncryptPassword(password)),
            };


            string query = "INSERT INTO public.credentials (unit_password, name, login, basket_pass_id) VALUES (@password, @telegram_id) RETURNING id;";


            return res;
        }


        public List<UserModel> GetUsers()
        {
            var users = new List<UserModel>();

            var query = "SELECT id, telegram_id FROM public.user;";

            try
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
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



        private T SelectOne<T>(string query, NpgsqlParameter[] parameters = null) where T : class, new()
        {
            T res = default(T);

            try
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    using (NpgsqlDataReader dataReader = command.ExecuteReader())
                    {
                        dataReader.Read();
                        res = dataReader.ConvertToObject<T>();
                        return res;
                    }

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

        private int Insert(string query, NpgsqlParameter[] parameters = null)
        {
            var res = -1;

            try
            {

                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    res = (int)command.ExecuteScalar();
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
        private string EncryptPassword(string pass)
        {
            return pass;
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
