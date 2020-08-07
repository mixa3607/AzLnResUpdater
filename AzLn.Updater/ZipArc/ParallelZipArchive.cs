using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Compression;

namespace AzLn.Updater.ZipArc
{
    public class ParallelZipArchive : IZipArchive
    {
        internal readonly string Path;
        internal readonly ConcurrentBag<ZipArchive> FreeReaders = new ConcurrentBag<ZipArchive>();

        public IReadOnlyCollection<IZipArchiveEntry> Entries { get; }

        public ParallelZipArchive(ZipArchive archive, string path)
        {
            Path = path;
            FreeReaders.Add(archive);

            var i = 0;
            var pEntries = new ParallelZipArchiveEntry[archive.Entries.Count];
            foreach (var zipArchiveEntry in archive.Entries)
            {
                pEntries[i] = new ParallelZipArchiveEntry(i, zipArchiveEntry, this);
                i++;
            }

            Entries = new ReadOnlyCollection<ParallelZipArchiveEntry>(pEntries);
        }

        public void Dispose()
        {
            foreach (var archive in FreeReaders)
                archive.Dispose();
        }
    }
}