using System;

namespace AzLn.Contract.Exceptions
{
    public class PluginNotFoundException : Exception
    {
        public PluginNotFoundException(string message) : base(message)
        {

        }
    }
}