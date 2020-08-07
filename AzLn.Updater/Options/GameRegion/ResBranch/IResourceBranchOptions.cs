using AzLn.Updater.Enums;

namespace AzLn.Updater.Options
{
    public interface IResourceBranchOptions
    {
        EResourceBranchType Type { get; }
        IResourceBranchObbOptions Obb { get; }
        IResourceBranchStoreOptions Store { get; }
    }
}