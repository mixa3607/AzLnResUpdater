using AzLn.Updater.Enums;

namespace AzLn.Updater.DataModels
{
    public class UpdateBranchResult
    {
        public ICompareResult Files { get; set; }
        public EResourceBranchType BranchType { get; set; }
        public ResourceBranchVersion FromVersion { get; set; }
        public ResourceBranchVersion ToVersion { get; set; }
    }
}