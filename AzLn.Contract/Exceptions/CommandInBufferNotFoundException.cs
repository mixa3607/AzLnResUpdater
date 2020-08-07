using System;

namespace AzLn.Contract.Exceptions
{
    public class CommandInBufferNotFoundException : Exception
    {
        public CommandInBufferNotFoundException(string message) : base(message)
        {

        }
    }
}
