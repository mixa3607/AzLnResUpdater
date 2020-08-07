using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzLn.Updater.Downloaders
{
    public interface IDownloadHelper
    {
        Task<byte[]> DownloadBytesAsync(string url, IEnumerable<string> segments,
            string userAgent = null, CancellationToken cancellationToken = default);

        Task<string> DownloadStringAsync(string url, IEnumerable<string> segments,
            string userAgent = null, CancellationToken cancellationToken = default);
    }
}