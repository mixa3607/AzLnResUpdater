namespace AzLn.Updater.DataModels
{
    public interface IResourceFileInfo
    {
        //void LoadFromCsvString(string rawCsvString);
        string Path { get; }
        long Size { get; }
        string Md5Hash { get; }
    }
}