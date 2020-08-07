using System.IO;

namespace AzLn.Updater.Options
{
    public class ResourceBranchStoreOptions : IResourceBranchStoreOptions
    {
        public string RootPath { get; set; }

        public string ResourcesPath
        {
            get => Path.Combine(RootPath, _resourcesPath);
            set => _resourcesPath = value;
        }

        public string HashesPath
        {
            get => Path.Combine(RootPath, _hashesPath);
            set => _hashesPath = value;
        }

        public string VersionPath
        {
            get => Path.Combine(RootPath, _versionPath);
            set => _versionPath = value;
        }

        private string _resourcesPath = "";
        private string _hashesPath = "";
        private string _versionPath = "";
    }
}