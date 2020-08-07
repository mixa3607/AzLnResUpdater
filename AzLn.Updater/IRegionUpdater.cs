using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AzLn.Updater.DataModels;
using AzLn.Updater.Enums;
using AzLn.Updater.Options;

namespace AzLn.Updater
{
    public interface IRegionUpdater
    {
        IGameRegionInfo RegionInfo { get; }
        EResourceBranchType[] ResBranches { get; }

        void InitAllBranches();
        void InitGit();
        void InitTelegram();
        //IEnumerable<ICompareResult> ValidateAllBranches();
        void ValidateAndFixBranch(EResourceBranchType branchType);

        ICompareResult ValidateBranch(EResourceBranchType branchType);
        void FixBranch(EResourceBranchType branchType, ICompareResult validatingResult);

        Task<IReadOnlyCollection<ResourceBranchVersion>> GetVersionsFromGateAsync();

        ResourceBranchVersion GetBranchVersionFromObb(EResourceBranchType branchType);
        UpdateBranchResult UpdateBranchFromObb(EResourceBranchType branchType);
        ResourceBranchVersion GetBranchVersion(EResourceBranchType branchType);
        Task<UpdateBranchResult> UpdateBranchAsync(EResourceBranchType branchType, IReadOnlyCollection<ResourceBranchVersion> versions, bool force = false);
    }
}