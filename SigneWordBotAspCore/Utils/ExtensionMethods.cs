using System.Linq;

namespace SigneWordBotAspCore.Utils
{
    public static class ExtensionMethods
    {
        public static string ToSnakeCase(this string camelCaseString)
        {
            return string.Concat(camelCaseString.Select((c, i) =>
            {
                if (i > 0 && char.IsUpper(c))
                {
                    return $"_{c}";
                }

                return c.ToString();
            })).ToLower();
        }


        public static string CheckAndRemove(this string str, char c, int position = 0)
        {
            return str[position].Equals(c)
                ? str.Remove(position, 1)
                : str;
        }
        
    }
}