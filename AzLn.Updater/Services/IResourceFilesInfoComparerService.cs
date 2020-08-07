using System.Collections.Generic;
using AzLn.Updater.DataModels;

namespace AzLn.Updater
{
    public interface IResourceFilesInfoComparerService
    {
        ICompareResult Compare(IEnumerable<IResourceFileInfo> oldList, IEnumerable<IResourceFileInfo> newList);
    }
}