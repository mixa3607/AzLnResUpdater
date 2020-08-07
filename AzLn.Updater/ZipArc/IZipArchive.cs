using System;
using System.Collections.Generic;

namespace AzLn.Updater.ZipArc
{
    public interface IZipArchive : IDisposable
    {
        IReadOnlyCollection<IZipArchiveEntry> Entries { get; }
    }
}