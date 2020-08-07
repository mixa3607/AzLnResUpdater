using AzLn.Updater.Enums;

namespace AzLn.Updater.Options
{
    public class ResourceBranchOptions : IResourceBranchOptions
    {
        public EResourceBranchType Type { get; set; }
        public ResourceBranchObbOptions Obb { get; set; }
        public ResourceBranchStoreOptions Store { get; set; }

        IResourceBranchObbOptions IResourceBranchOptions.Obb => Obb;
        IResourceBranchStoreOptions IResourceBranchOptions.Store => Store;
    }
}