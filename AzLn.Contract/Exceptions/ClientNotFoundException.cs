using System;

namespace AzLn.Contract.Exceptions
{
    [Obsolete]
    public class ClientNotFoundException : Exception
    {
        public ClientNotFoundException(string message) : base(message)
        {

        }
    }
}