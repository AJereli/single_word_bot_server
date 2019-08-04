using System;
using System.Linq;
using FastMember;
using Npgsql;
using NpgsqlTypes;
using static System.Activator;
namespace SigneWordBotAspCore.Base
{
    internal static class NpgsqlDataReaderExtentions
    {
        public static T ConvertToObject<T>(this NpgsqlDataReader rd) 
        {
            Type type = typeof(T);
            var accessor = TypeAccessor.Create(type);
            var members = accessor.GetMembers();

//            var c = typeof(T).GetConstructor();
            dynamic t = CreateInstance(typeof(T));
            

            for (int i = 0; i < rd.FieldCount; i++)
            {
                if (!rd.IsDBNull(i))
                {
                    var fieldName = rd.GetName(i);

                    var firstMember = members.FirstOrDefault(m => {
                        var propertyName = m.GetAttribute(typeof(PgNameAttribute), false) is PgNameAttribute propertyAttr ? propertyAttr.PgName : m.Name.ToSnakeCase();

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
                        t = (T)rd.GetValue(i);
                    }
                }
            }

            return t;
        }
    }

}
