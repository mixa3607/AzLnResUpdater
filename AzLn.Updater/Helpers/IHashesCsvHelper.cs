using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AzLn.Updater.DataModels;

namespace AzLn.Updater
{
    public interface IHashesCsvHelper
    {
        Task<IReadOnlyCollection<IResourceFileInfo>> ReadAsync(IEnumerable<string> csvStrings,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<IResourceFileInfo>> ReadAsync(Stream csvStream,
            CancellationToken cancellationToken = default);
        Task WriteAsync(IEnumerable<IResourceFileInfo> resFilesInfo, string destPath);
    }
}