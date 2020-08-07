using System.Data;
using System.IO.Compression;

namespace AzLn.Updater.ZipArc
{
    public class ParallelZipArchiveEntry : IZipArchiveEntry
    {
        public string Name => _entry.Name;
        public string FullName => _entry.FullName;
        public long Length => _entry.Length;

        private readonly ParallelZipArchive _parent;
        private readonly ZipArchiveEntry _entry;
        private readonly int _index;

        public ParallelZipArchiveEntry(int index, ZipArchiveEntry entry, ParallelZipArchive parent)
        {
            _index = index;
            _parent = parent;
            _entry = entry;
        }

        public void ExtractToFile(string destPath, bool overwrite = false)
        {
            //Trace.TraceInformation($"Number of readers: {_parent.FreeReaders.Count}");
            if (!_parent.FreeReaders.TryTake(out var archive))
                archive = ZipFile.OpenRead(_parent.Path);

            var entry = archive.Entries[_index];
            if (entry.FullName != FullName)
            {
                throw new DataException($"{entry.FullName} != {FullName}");
            }

            entry.ExtractToFile(destPath, overwrite);
            _parent.FreeReaders.Add(archive);
        }
    }
}