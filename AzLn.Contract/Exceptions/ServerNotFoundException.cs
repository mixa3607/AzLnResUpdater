using System;

namespace AzLn.Contract.Exceptions
{
    public class ServerNotFoundException : Exception
    {
        public ServerNotFoundException(string message) : base(message)
        {

        }
    }
}