using System.Collections.Generic;
using System.Linq;

namespace AzLn.Updater.DataModels
{
    public interface ICompareResult
    {
        bool HaveChanges { get; }
        IEnumerable<IResourceFileInfo> AddedFiles { get; }
        IEnumerable<IResourceFileInfo> DeletedFiles { get; }
        IEnumerable<IResourceFileInfo> UpdatedFiles { get; }
    }

    public class CompareResult : ICompareResult
    {
        public bool HaveChanges => AddedFiles.Any() || DeletedFiles.Any() || UpdatedFiles.Any();
        public IEnumerable<IResourceFileInfo> AddedFiles { get; set; } = new IResourceFileInfo[0];
        public IEnumerable<IResourceFileInfo> DeletedFiles { get; set; } = new IResourceFileInfo[0];
        public IEnumerable<IResourceFileInfo> UpdatedFiles { get; set; } = new IResourceFileInfo[0];
    }
}