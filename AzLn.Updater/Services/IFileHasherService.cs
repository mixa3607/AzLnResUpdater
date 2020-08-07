using System.Collections.Generic;
using System.Threading;
using AzLn.Updater.DataModels;

namespace AzLn.Updater
{
    public interface IFileHasherService
    {
        IResourceFileInfo HashFile(string pathToFile, string rootPath);

        IEnumerable<IResourceFileInfo> HashDirectory(string pathToDir, string rootPath, bool recursively = true,
            CancellationToken cancellationToken = default);

        IEnumerable<IResourceFileInfo> HashDirectory(string pathToDir, bool recursively = true,
            CancellationToken cancellationToken = default);
    }
}