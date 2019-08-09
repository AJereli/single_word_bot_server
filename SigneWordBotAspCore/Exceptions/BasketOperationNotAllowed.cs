using System;

namespace SigneWordBotAspCore.Exceptions
{
    public class BasketOperationNotAllowed: Exception
    {
        public override string Message =>
            "Current operation not permitted with you access type. Contact with basket owner.";
    }
}