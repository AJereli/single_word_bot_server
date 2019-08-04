using System.Linq;

namespace SigneWordBotAspCore.Base
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
    }
}