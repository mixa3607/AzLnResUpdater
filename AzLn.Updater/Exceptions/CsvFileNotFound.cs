using System;

namespace AzLn.Updater.Exceptions
{
    public class CsvFileNotFoundException : Exception
    {
        public string FilePath { get; }
        public CsvFileNotFoundException(string message, string filePath) : base(message)
        {
            FilePath = filePath;
        }
    }

    public class VersionFileNotFoundException : Exception
    {
        public string FilePath { get; }
        public VersionFileNotFoundException(string message, string filePath) : base(message)
        {
            FilePath = filePath;
        }
    }

    public class DownloadResourcesException : Exception
    {
        public DownloadResourcesException(string message) : base(message)
        {
        }
        public DownloadResourcesException()
        {
        }
    }

    public class FixResourcesException : Exception
    {
        public FixResourcesException(string message) : base(message)
        {
        }
        public FixResourcesException()
        {
        }
    }

    public class InvalidObbExceptions : Exception
    {
        public InvalidObbExceptions(string message) : base(message)
        {
        }
        public InvalidObbExceptions()
        {
        }
    }
}