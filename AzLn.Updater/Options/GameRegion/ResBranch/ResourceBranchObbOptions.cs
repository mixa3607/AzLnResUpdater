namespace AzLn.Updater.Options
{
    public class ResourceBranchObbOptions : IResourceBranchObbOptions
    {
        public string ResourcesPath { get; set; }

        public string HashesPath { get; set; }
        public string VersionPath { get; set; }
    }
}