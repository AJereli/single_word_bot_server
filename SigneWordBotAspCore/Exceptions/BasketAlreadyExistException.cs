using System;

namespace SigneWordBotAspCore.Exceptions
{
    public class BasketAlreadyExistException: Exception
    {
        public override string Message => "You already have a basket with this name";
    }
}