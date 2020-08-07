using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzLn.Updater.DataModels;

namespace AzLn.Updater.Downloaders
{
    public interface ICdnDownloadService
    {
        Task<byte[]> DownloadResourceAsync(string hash, CancellationToken cancellationToken);

        Task DownloadResourceAsync(string hash, string destinationFile,
            CancellationToken cancellationToken);

        Task<string> DownloadHashesAsStringAsync(ResourceBranchVersion version, CancellationToken cancellationToken);

        Task<string[]> DownloadHashesAsStringsAsync(ResourceBranchVersion version,
            CancellationToken cancellationToken);

        Task<IReadOnlyCollection<IResourceFileInfo>> DownloadHashesAsync(ResourceBranchVersion version, CancellationToken cancellationToken = default);

        Task DownloadResourcesAsync(IEnumerable<IResourceFileInfo> azurFiles, string resourcesPath,
            CancellationToken cancellationToken = default);
    }
}