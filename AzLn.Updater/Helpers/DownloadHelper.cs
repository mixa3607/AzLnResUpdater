using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using NLog;

namespace AzLn.Updater.Downloaders
{
    public class DownloadHelper : IDownloadHelper
    {
        private readonly ILogger _logger;

        public DownloadHelper(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> DownloadBytesAsync(string url, IEnumerable<string> segments,
            string userAgent = null, CancellationToken cancellationToken = default)
        {
            var request = CreateRequest(url, segments, userAgent);
            try
            {
                return await request
                    .GetAsync(cancellationToken)
                    .ReceiveBytes();
            }
            catch (FlurlHttpException e)
            {
                _logger.Warn("Http ({HttpCode}) problem with download {Url}", request.Url, e.Call.HttpStatus);
                throw;
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Problem with download {Url}", request.Url);
                throw;
            }
        }

        public async Task<string> DownloadStringAsync(string url, IEnumerable<string> segments,
            string userAgent = null, CancellationToken cancellationToken = default)
        {
            var request = CreateRequest(url, segments, userAgent);
            try
            {
                return await request
                    .GetAsync(cancellationToken)
                    .ReceiveString();
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Problem with download {Url}", request.Url);
                throw;
            }
        }

        private IFlurlRequest CreateRequest(string url, IEnumerable<string> segments, string userAgent = null)
        {
            return url.AppendPathSegments(segments).WithHeader("User-Agent", userAgent);
        }
    }
}