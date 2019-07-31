using System;
using System.Linq;
using FastMember;
using Npgsql;
using NpgsqlTypes;

namespace SigneWordBotAspCore.Base
{
    internal static class NpgsqlDataReaderExtentions
    {
        public static T ConvertToObject<T>(this NpgsqlDataReader rd) where T : class, new()
        {
            Type type = typeof(T);
            var accessor = TypeAccessor.Create(type);
            var members = accessor.GetMembers();
            var t = new T();


            for (int i = 0; i < rd.FieldCount; i++)
            {
                if (!rd.IsDBNull(i))
                {
                    string fieldName = rd.GetName(i);


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

                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine(ex);
                        }
                    }
                }
            }

            return t;
        }
    }

}
