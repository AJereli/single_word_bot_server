using System;

namespace SigneWordBotAspCore.Exceptions
{
    internal sealed class CommandNotFoundException: Exception
    {
        public override string Message => "No such command was found";
    }
}