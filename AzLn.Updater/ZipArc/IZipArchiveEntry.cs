namespace AzLn.Updater.ZipArc
{
    public interface IZipArchiveEntry
    {
        string Name { get; }
        string FullName { get; }
        long Length { get; }
        void ExtractToFile(string destPath, bool overwrite = false);
    }
}