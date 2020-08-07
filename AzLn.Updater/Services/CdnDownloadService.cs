using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AzLn.Updater.DataModels;
using AzLn.Updater.Exceptions;
using AzLn.Updater.Options;
using Flurl.Http;
using NLog;

namespace AzLn.Updater.Downloaders
{
    public class CdnDownloadService : ICdnDownloadService
    {
        private readonly ILogger _logger;
        private readonly IRegionCdnOptions _options;
        private readonly IHashesCsvHelper _csvHelper;
        private readonly IDownloadHelper _downloader;
        private readonly Semaphore _downloadingSemaphore;

        public CdnDownloadService(ILogger logger, IRegionCdnOptions options, IHashesCsvHelper csvHelper, IDownloadHelper downloader)
        {
            _logger = logger;
            _options = options;
            _csvHelper = csvHelper;
            _downloader = downloader;
            _downloadingSemaphore = new Semaphore(_options.DownloadThreads, _options.DownloadThreads);
        }

        public async Task<byte[]> DownloadResourceAsync(string hash, CancellationToken cancellationToken)
        {
            return await _downloader.DownloadBytesAsync(_options.Url,
                new[] {_options.Platform, "resource", hash}, _options.UserAgent, cancellationToken);
        }

        public async Task<string> DownloadHashesAsStringAsync(ResourceBranchVersion version, CancellationToken cancellationToken)
        {
            _logger.Debug("Start downloading hashes file: {Version}", version);
            var str = await _downloader.DownloadStringAsync(_options.Url,
                new[] { _options.Platform, "hash", version.RawString }, _options.UserAgent, cancellationToken);
            _logger.Debug("Version file {Version} downloaded", version);
            return str;

        }

        public async Task<string[]> DownloadHashesAsStringsAsync(ResourceBranchVersion version,
            CancellationToken cancellationToken)
        {
            var hStrs = (await DownloadHashesAsStringAsync(version, cancellationToken))
                .Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None);
            return hStrs;
        }

        public async Task<IReadOnlyCollection<IResourceFileInfo>> DownloadHashesAsync(ResourceBranchVersion version, CancellationToken cancellationToken = default)
        {
            var fileStrs = await DownloadHashesAsStringsAsync(version, cancellationToken);
            return await _csvHelper.ReadAsync(fileStrs, cancellationToken);
        }

        public async Task DownloadResourceAsync(string hash, string destinationFile, CancellationToken cancellationToken)
        {
            _logger.Debug("Start downloading file {Hash} to {Path}", hash, destinationFile);
            var dir = Path.GetDirectoryName(destinationFile);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var bytes = await DownloadResourceAsync(hash, cancellationToken);
            await File.WriteAllBytesAsync(destinationFile, bytes, cancellationToken);
            _logger.Debug("Hash {Hash} downloaded", hash);
        }

        public async Task DownloadResourcesAsync(IEnumerable<IResourceFileInfo> filesInfo, string resourcesPath,
            CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task>();
            try
            {
                foreach (var fileInfo in filesInfo)
                {
                    _downloadingSemaphore.WaitOne();
                    var dTask = Task.Run(async () =>
                    {
                        try
                        {
                            await DownloadResourceAsync(fileInfo.Md5Hash, Path.Combine(resourcesPath, fileInfo.Path), cancellationToken);
                        }
                        catch (FlurlHttpException)
                        {
                            _logger.Warn("Download resources problem");
                            throw;
                        }
                        finally
                        {
                            _downloadingSemaphore.Release();
                        }
                    }, cancellationToken);
                    tasks.Add(dTask);
                }

                await Task.WhenAll(tasks.ToArray());
            }
            catch (AggregateException)
            {
                throw new DownloadResourcesException();
            }
        }
    }
}