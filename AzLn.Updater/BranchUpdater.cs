using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzLn.Contract.Extensions;
using AzLn.Updater.DataModels;
using AzLn.Updater.Downloaders;
using AzLn.Updater.Enums;
using AzLn.Updater.Exceptions;
using AzLn.Updater.Options;
using NLog;

namespace AzLn.Updater
{
    public interface IBranchUpdater
    {
        EResourceBranchType BranchType { get; }
        void InitPaths();
        //void ValidateAndFixResources();
        Task FixResourcesAsync(ICompareResult compareResult);
        ICompareResult ValidateResources();

        ResourceBranchVersion GetVersion();
        UpdateBranchResult UpdateFromObb();
        ResourceBranchVersion GetVersionFromObb();
        Task<UpdateBranchResult> UpdateAsync(ResourceBranchVersion version, bool force = false);
    }

    public class BranchUpdater : IBranchUpdater
    {
        public EResourceBranchType BranchType { get; }
        private readonly IResourceBranchStoreOptions _storeOptions;
        private readonly ILogger _logger;
        private readonly IFileHasherService _hasher;
        private readonly IHashesCsvHelper _csvReader;
        private readonly IResourceFilesInfoComparerService _comparer;
        private readonly ICdnDownloadService _cdnDownloader;
        private readonly IObbService _obbService;

        public BranchUpdater(ILogger logger, IResourceBranchOptions options, IFileHasherService hasher,
            IHashesCsvHelper csvReader, IResourceFilesInfoComparerService comparer,
            ICdnDownloadService cdnDownloader, IObbService obbService)
        {
            _obbService = obbService;
            _storeOptions = options.Store;
            BranchType = options.Type;
            _logger = logger;
            _hasher = hasher;
            _csvReader = csvReader;
            _comparer = comparer;
            _cdnDownloader = cdnDownloader;
            _logger.Info("Branch updater create");
        }

        public void InitPaths()
        {
            _logger.Debug("Init all paths");
            new List<string>()
                {
                    Path.GetDirectoryName(_storeOptions.HashesPath),
                    Path.GetDirectoryName(_storeOptions.VersionPath),
                    _storeOptions.RootPath,
                    _storeOptions.ResourcesPath
                }
                .Distinct()
                .Where(dirForCheck => !Directory.Exists(dirForCheck))
                .ForEach(x => Directory.CreateDirectory(x));
            _logger.Debug("All directories created");
        }

        public async Task<UpdateBranchResult> UpdateAsync(ResourceBranchVersion newVersion, bool force = false)
        {
            var currentVersion = new ResourceBranchVersion("-1", BranchType);
            try
            {
                currentVersion = GetVersion();
            }
            catch (VersionFileNotFoundException e)
            {
                _logger.Warn(e.Message);
                force = true;
            }
            if (newVersion.RawString == currentVersion.RawString && !force)
            {
                _logger.Debug("Versions is eq ({Version}). Skip updating", newVersion.RawString);
                return new UpdateBranchResult()
                {
                    BranchType = BranchType,
                    Files = new CompareResult(),
                    FromVersion = currentVersion,
                    ToVersion = newVersion
                };
            }
            _logger.Debug("Start resources validating");

            var newInfos = _cdnDownloader.DownloadHashesAsync(newVersion).Result;
            IReadOnlyCollection<IResourceFileInfo> savedInfos = Array.Empty<IResourceFileInfo>();
            try
            {
                savedInfos = GetAzurFilesFromCsvFile();
            }
            catch (CsvFileNotFoundException e)
            {
                _logger.Warn(e.Message);
            }
            var compareResult = _comparer.Compare(savedInfos, newInfos);

            if (!compareResult.HaveChanges)
            {
                _logger.Debug("All resources is updates");
            }
            else
            {
                if (compareResult.DeletedFiles.Any())
                {
                    _logger.Warn("Detect deleted files regarding {HashesPath}", _storeOptions.HashesPath);
                    compareResult.DeletedFiles.ForEach(x => _logger.Trace("Detect deleted {FileInfo}", x));
                }

                if (compareResult.AddedFiles.Any())
                {
                    _logger.Warn("Detect added files regarding {HashesPath}", _storeOptions.HashesPath);
                    compareResult.DeletedFiles.ForEach(x => _logger.Trace("Detect added {FileInfo}", x));
                }

                if (compareResult.UpdatedFiles.Any())
                {
                    _logger.Warn("Detect updated files regarding {HashesPath}", _storeOptions.HashesPath);
                    compareResult.DeletedFiles.ForEach(x => _logger.Trace("Detect updated {FileInfo}", x));
                }
                await UpdateResources(compareResult);
                WriteVersion(newVersion.RawString);
                await _csvReader.WriteAsync(newInfos, _storeOptions.HashesPath);
            }
            return new UpdateBranchResult()
            {
                BranchType = BranchType,
                Files = compareResult,
                FromVersion = currentVersion,
                ToVersion = newVersion
            };
        }

        private void WriteVersion(string versionCode)
        {
            File.WriteAllText(_storeOptions.VersionPath, versionCode);
        }

        public async Task UpdateResources(ICompareResult compareResult)
        {
            _logger.Debug("Start resources repairing process");
            _logger.Debug("Deleting non indexed resources");
            foreach (var fileInfo in compareResult.DeletedFiles)
            {
                try
                {
                    File.Delete(Path.Combine(_storeOptions.ResourcesPath, fileInfo.Path));
                }
                catch (Exception e)
                {
                    _logger.Warn(e, "Problem with deleting {FileInfo}. Aborting", fileInfo);
                    throw;
                }
            }

            _logger.Debug("Downloading updated and not existed files");
            try
            {
                await _cdnDownloader.DownloadResourcesAsync(compareResult.AddedFiles.Concat(compareResult.UpdatedFiles),
                    _storeOptions.ResourcesPath);
            }
            catch (DownloadResourcesException)
            {
                throw;
            }
        }

        public ResourceBranchVersion GetVersion()
        {
            if (!File.Exists(_storeOptions.VersionPath))
            {
                throw new VersionFileNotFoundException(
                    $"Version file {_storeOptions.VersionPath} not found", _storeOptions.VersionPath);
            }

            var verLines = File.ReadAllLines(_storeOptions.VersionPath);
            if (verLines.Length != 1)
            {
                throw new Exception("Bad version file");
            }

            return ResourceBranchVersion.Parse(verLines[0]);
        }

        private IReadOnlyCollection<IResourceFileInfo> GetAzurFilesFromCsvFile()
        {
            if (!File.Exists(_storeOptions.HashesPath))
            {
                throw new CsvFileNotFoundException($"File {_storeOptions.HashesPath} not found",
                    _storeOptions.HashesPath);
            }

            using var fileStream = File.OpenRead(_storeOptions.HashesPath);
            return _csvReader.ReadAsync(fileStream).Result;
        }

        public async Task ValidateAndFixResourcesAsync()
        {
            var validatingResult = ValidateResources();
            if (validatingResult.HaveChanges)
            {
                await FixResourcesAsync(validatingResult);
            }
        }

        public ICompareResult ValidateResources()
        {
            _logger.Debug("Start resources validating");
            var currentInfos = _hasher.HashDirectory(_storeOptions.ResourcesPath);
            var savedInfos = GetAzurFilesFromCsvFile();
            var compareResult = _comparer.Compare(savedInfos, currentInfos);

            if (!compareResult.HaveChanges)
            {
                _logger.Debug("All resources is valid");
                return compareResult;
            }

            if (compareResult.DeletedFiles.Any())
            {
                _logger.Warn("Detect deleted files regarding {HashesPath}", _storeOptions.HashesPath);
                compareResult.DeletedFiles.ForEach(x => _logger.Trace("Detect deleted {FileInfo}", x));
            }

            if (compareResult.AddedFiles.Any())
            {
                _logger.Warn("Detect added files regarding {HashesPath}", _storeOptions.HashesPath);
                compareResult.DeletedFiles.ForEach(x => _logger.Trace("Detect added {FileInfo}", x));
            }

            if (compareResult.UpdatedFiles.Any())
            {
                _logger.Warn("Detect updated files regarding {HashesPath}", _storeOptions.HashesPath);
                compareResult.DeletedFiles.ForEach(x => _logger.Trace("Detect updated {FileInfo}", x));
            }

            return compareResult;
        }

        public async Task FixResourcesAsync(ICompareResult compareResult)
        {
            _logger.Debug("Start resources repairing process");
            _logger.Debug("Deleting non indexed resources");
            foreach (var fileInfo in compareResult.AddedFiles)
            {
                try
                {
                    File.Delete(Path.Combine(_storeOptions.ResourcesPath, fileInfo.Path));
                }
                catch (Exception e)
                {
                    _logger.Warn(e, "Problem with deleting {FileInfo}. Aborting", fileInfo);
                    throw;
                }
            }

            _logger.Debug("Downloading updated and not existed files");
            try
            {
                await _cdnDownloader.DownloadResourcesAsync(compareResult.DeletedFiles.Concat(compareResult.UpdatedFiles),
                    _storeOptions.ResourcesPath);
            }
            catch (DownloadResourcesException)
            {
                throw new FixResourcesException();
            }
        }


        public UpdateBranchResult UpdateFromObb()
        {
            return _obbService.Extract();
        }

        public ResourceBranchVersion GetVersionFromObb()
        {
            return _obbService.GetVersion();
        }
    }
}