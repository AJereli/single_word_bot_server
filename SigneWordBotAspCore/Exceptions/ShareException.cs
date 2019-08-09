using System;

namespace SigneWordBotAspCore.Exceptions
{
    public enum ShareExceptionType
    {
        Other,
        NoBasket,
        NoUser,
        NoAccess
    }
    public class ShareException: Exception
    {
        public override string Message => "Error(s) while sharing";
        public ShareExceptionType ExceptionType { get; set; }
    }
}