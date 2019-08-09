using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FastMember;
using Npgsql;
using NpgsqlTypes;
using static System.Activator;

namespace SigneWordBotAspCore.Utils
{
    internal static class NpgsqlDataReaderExtentions
    {
        public static T ConverToObject<T>(this IDictionary<string, object> dict)
        {
            var type = typeof(T);
            var accessor = TypeAccessor.Create(type);
            var members = accessor.GetMembers();

            dynamic t = CreateInstance(typeof(T));
            
            
            foreach (var (key, value) in dict)
            {
                if (value == null) continue;
                
                var firstMember = members.FirstOrDefault(m =>
                {
                    var propertyName =
                        m.GetAttribute(typeof(PgNameAttribute), false) is PgNameAttribute propertyAttr
                            ? propertyAttr.PgName
                            : m.Name.ToSnakeCase();

                    return string.Equals(key, propertyName, StringComparison.OrdinalIgnoreCase);
                });
                if (firstMember != null)
                {
                    try
                    {
                        accessor[t, firstMember.Name] = value;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                else if (typeof(T).IsValueType)
                {
                    t = (T) value;
                }
            }


            return t;
        }

        public static T ConvertToObject<T>(this NpgsqlDataReader rd)
        {
            var type = typeof(T);
            var accessor = TypeAccessor.Create(type);
            var members = accessor.GetMembers();

            dynamic t = CreateInstance(typeof(T));


            for (var i = 0; i < rd.FieldCount; i++)
            {
                if (rd.IsDBNull(i)) continue;
                
                var fieldName = rd.GetName(i);

                var firstMember = members.FirstOrDefault(m =>
                {
                    var propertyName =
                        m.GetAttribute(typeof(PgNameAttribute), false) is PgNameAttribute propertyAttr
                            ? propertyAttr.PgName
                            : m.Name.ToSnakeCase();

                    return string.Equals(fieldName, propertyName, StringComparison.OrdinalIgnoreCase);
                });
                if (firstMember != null)
                {
                    try
                    {
                        accessor[t, firstMember.Name] = rd.GetValue(i);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                else if (typeof(T).IsValueType)
                {
                    t = (T) rd.GetValue(i);
                }
            }

            return t;
        }
    }
}