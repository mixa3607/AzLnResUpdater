namespace AzLn.Updater.Options
{
    public interface IResourceBranchObbOptions
    {
        string ResourcesPath { get; set; }
        string HashesPath { get; set; }
        string VersionPath { get; set; }
    }
}