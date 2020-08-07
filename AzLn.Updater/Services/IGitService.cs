using AzLn.Updater.DataModels;

namespace AzLn.Updater
{
    public interface IGitService
    {
        void Init();
        void CommitBranchUpdate(UpdateBranchResult branchUpdate);
        void Push();
    }
}