using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using SigneWordBotAspCore.Utils;

namespace SigneWordBotAspCore.Services
{
    public abstract class BaseDataBaseService
    {
        
        protected readonly NpgsqlConnection _connection;


        protected BaseDataBaseService(IAppContext appContext)
        {
            _connection = new NpgsqlConnection(appContext.DBConnectionString);
        }
        
        
        public IEnumerable<T> SelectMany<T>(string query, IEnumerable<NpgsqlParameter> sqlParams = null,
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
        
        
        public IDictionary<string, object> SelectOneRaw(string query,
            IEnumerable<NpgsqlParameter> parameters = null,
            NpgsqlTransaction transaction = null)
        {
            Dictionary<string, object> res = null;
            
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
                            res = new Dictionary<string, object>();
                            dataReader.Read();
                            res = Enumerable.Range(0, dataReader.FieldCount)
                                .ToDictionary(i => dataReader.GetName(i), i =>
                                {
                                    var value = !dataReader.IsDBNull(i) ? dataReader.GetValue(i) : null;
                                    return value;
                                });
                        }
                    }
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


        public T SelectOne<T>(string query, 
            IEnumerable<NpgsqlParameter> parameters = null,
            NpgsqlTransaction transaction = null)
        {
            return SelectOneRaw(query, parameters, transaction).ConverToObject<T>();
        }

        /// <summary>
        /// Run update or delete query WITHOUT self-based transaction
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <returns>Return count of affected rows</returns>
        protected int DeleteUpdate(string query,
            IEnumerable<NpgsqlParameter> parameters = null,
            NpgsqlTransaction transaction = null)
        {
            var rowsAffected = -1;
            var connectionIsOpenedByMe = false;

            try
            {
                connectionIsOpenedByMe = TryOpenConnection();

                using (var command = new NpgsqlCommand(query, _connection, transaction))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters.ToArray());

                    rowsAffected = command.ExecuteNonQuery();

                }
            }
            catch (NpgsqlException e)
            {
                return -1;
            }
            finally
            {
                if (connectionIsOpenedByMe)
                    _connection.Close();
            }
            return rowsAffected;
        }

        /// <summary>
        /// Run update or delete query with self-based transaction
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns>Return count of affected rows</returns>
        protected int DeleteUpdateTransaction(string query,
            IEnumerable<NpgsqlParameter> parameters = null)
        {
            var rowsAffected = -1;
            
            
            var connectionIsOpenedByMe = TryOpenConnection();

            using (var transaction = _connection.BeginTransaction())
            {
                try
                {
                    rowsAffected = DeleteUpdate(query, parameters, transaction);
                }
                catch (NpgsqlException ex)
                {
                    transaction.Rollback();
                }
                finally
                {
                    if (connectionIsOpenedByMe) _connection.Close();
                }
            }

            return rowsAffected;
        }


        protected int Insert(string query,
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

        protected bool TryOpenConnection()
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