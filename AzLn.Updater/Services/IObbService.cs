using AzLn.Updater.DataModels;

namespace AzLn.Updater
{
    public interface IObbService
    {
        ResourceBranchVersion GetVersion();
        UpdateBranchResult Extract(bool cleanBeforeExtract = true);
    }
}