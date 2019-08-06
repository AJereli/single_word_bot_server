namespace SigneWordBotAspCore.Models
{
    public enum AccessType
    {
        Owner = 1,
        SharedRead,
        SharedReadWrite
    }

    public static class AccessTypeMethods
    {
        public static int ToInt(this AccessType at)
        {
            return (int) at;
        }
    }
}