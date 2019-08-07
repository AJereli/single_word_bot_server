using System;

namespace SigneWordBotAspCore.Exceptions
{
    public enum ShareExceptionType
    {
        NoBasket,
        NoUser
    }
    public class ShareException: Exception
    {
        public override string Message => "Error(s) while sharing";
        public ShareExceptionType ExceptionType { get; set; }
    }
}