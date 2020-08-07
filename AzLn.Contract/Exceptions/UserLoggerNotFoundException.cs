using System;

namespace AzLn.Contract.Exceptions
{
    public class UserLoggerNotFoundException : Exception
    {
        public UserLoggerNotFoundException(string message) : base(message)
        {

        }
    }
}