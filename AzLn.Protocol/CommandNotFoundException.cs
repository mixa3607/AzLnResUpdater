using System;

namespace AzLn.Protocol
{
    public class CommandNotFoundException : Exception
    {
        public CommandNotFoundException(string message) : base(message)
        {

        }
    }
}