using System.IO.Compression;

namespace AzLn.Updater.ZipArc
{
    public static class ParallelZipFile
    {
        public static ParallelZipArchive OpenRead(string path)
        {
            return new ParallelZipArchive(ZipFile.OpenRead(path), path);
        }
    }
}