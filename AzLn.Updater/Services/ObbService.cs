using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzLn.Updater.DataModels;
using AzLn.Updater.Exceptions;
using AzLn.Updater.Options;
using AzLn.Updater.ZipArc;

namespace AzLn.Updater
{
    public class ObbService : IObbService
    {
        private readonly IResourceBranchOptions _options;
        private readonly string _obbPath;
        private readonly int _extractionThreads;
        private readonly IHashesCsvHelper _csvHelper;

        public ObbService(IResourceBranchOptions options, IHashesCsvHelper csvHelper,
            IRegionObbOptions clientObbOptions)
        {
            _options = options;
            _obbPath = clientObbOptions.Path;
            _extractionThreads = clientObbOptions.ExtractionThreads;
            _csvHelper = csvHelper;
        }

        public ResourceBranchVersion GetVersion()
        {
            var obb = ZipFile.OpenRead(_obbPath);
            var versionEntry = obb.GetEntry(_options.Obb.VersionPath);
            if (versionEntry == null)
            {
                throw new VersionFileNotFoundException($"File {_options.Obb.VersionPath} not found in {_obbPath} obb",
                    _options.Obb.VersionPath);
            }

            var versionEntryStream = versionEntry.Open();
            var bytes = new byte[versionEntryStream.Length];
            versionEntryStream.Read(bytes, 0, bytes.Length);
            return new ResourceBranchVersion(Encoding.UTF8.GetString(bytes), _options.Type);
        }

        public UpdateBranchResult Extract(bool cleanBeforeExtract = true)
        {
            ExtractCsvFile();
            ExtractVersionFile();

            var hashes = GetFileInfos().ToArray();
            var obb = ParallelZipFile.OpenRead(_obbPath);
            var resources = FindEntries(obb, hashes);

            if (hashes.Length != resources.Count)
            {
                throw new InvalidObbExceptions("hashes count not eq resources count");
            }

            //clean
            if (cleanBeforeExtract)
            {
                Directory.GetDirectories(_options.Store.ResourcesPath).AsParallel().ForAll(x => Directory.Delete(x, true));
                Directory.GetFiles(_options.Store.ResourcesPath).AsParallel().ForAll(File.Delete);
            }

            ExtractResources(resources);
            return new UpdateBranchResult()
            {
                BranchType = _options.Type,
                Files = new CompareResult()
                {
                    AddedFiles = hashes,
                    DeletedFiles = Array.Empty<IResourceFileInfo>(),
                    UpdatedFiles = Array.Empty<IResourceFileInfo>()
                },
                FromVersion = new ResourceBranchVersion("", _options.Type),
                ToVersion = GetVersion()
            };
        }

        private IReadOnlyCollection<IZipArchiveEntry> FindEntries(IZipArchive obb, IReadOnlyCollection<IResourceFileInfo> azurFiles)
        {
            var sortedEntries = new SortedDictionary<string, IZipArchiveEntry>(
                obb.Entries.ToDictionary(x => Path.GetRelativePath(_options.Obb.ResourcesPath, x.FullName).Replace('\\', '/')));
            var selectedEntries = new List<IZipArchiveEntry>(azurFiles.Count);
            foreach (var azurFileInfo in azurFiles)
            {
                if (sortedEntries.TryGetValue(azurFileInfo.Path, out var val))
                {
                    selectedEntries.Add(val);
                }
            }

            return selectedEntries;
        }

        private IEnumerable<IResourceFileInfo> GetFileInfos()
        {
            var obb = ZipFile.OpenRead(_obbPath);
            var hashesEntry = obb.GetEntry(_options.Obb.HashesPath);
            if (hashesEntry == null)
            {
                throw new CsvFileNotFoundException($"File {_options.Obb.HashesPath} not found in {_obbPath} obb",
                    _options.Obb.HashesPath);
            }

            using var entryStream = hashesEntry.Open();
            return _csvHelper.ReadAsync(entryStream).Result;
        }

        private void ExtractVersionFile()
        {
            File.WriteAllText(_options.Store.VersionPath,GetVersion().RawString);
        }

        private void ExtractCsvFile()
        {
            var obb = ZipFile.OpenRead(_obbPath);
            obb.GetEntry(_options.Obb.HashesPath).ExtractToFile(_options.Store.HashesPath, true);
        }

        private void ExtractResources(IEnumerable<IZipArchiveEntry> resourceEntries,
            CancellationToken cancellationToken = default)
        {
            Parallel.ForEach(resourceEntries, new ParallelOptions()
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = _extractionThreads
                },
                ExtractResource);
        }

        private void ExtractResource(IZipArchiveEntry resourceEntry)
        {
            var relativePath = Path.GetRelativePath(_options.Obb.ResourcesPath, resourceEntry.FullName);
            var destPath = Path.Combine(_options.Store.ResourcesPath, relativePath);
            var dstDir = Path.GetDirectoryName(destPath);
            if (!Directory.Exists(dstDir))
            {
                Directory.CreateDirectory(dstDir);
            }

            resourceEntry.ExtractToFile(destPath, true);
        }
    }
}