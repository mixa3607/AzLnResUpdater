namespace AzLn.Updater.Options
{
    public interface IResourceBranchStoreOptions
    {
        string RootPath { get; }
        string ResourcesPath { get; }
        string HashesPath { get; }
        string VersionPath { get; }
    }
}