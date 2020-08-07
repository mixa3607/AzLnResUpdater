using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AzLn.Updater.DataModels;
using AzLn.Updater.Options;

namespace AzLn.Updater
{
    public class FileHasherService : IFileHasherService
    {
        private readonly IHashComputer _hasher;
        private readonly IFileHasherOptions _options;

        public FileHasherService(IHashComputer hasher, IFileHasherOptions options)
        {
            _hasher = hasher;
            _options = options;
        }

        public IResourceFileInfo HashFile(string pathToFile, string rootPath)
        {
            var fileStream = File.OpenRead(pathToFile);
            return new ResourceFileInfo()
            {
                Path = Path.GetRelativePath(rootPath, pathToFile).Replace("\\", "/"),
                Size = fileStream.Length,
                Md5Hash = _hasher.GetMd5Hex(fileStream)
            };
        }

        public IEnumerable<IResourceFileInfo> HashDirectory(string pathToDir, bool recursively = true,
            CancellationToken cancellationToken = default)
        {
            return HashDirectory(pathToDir, pathToDir, recursively, cancellationToken);
        }

        public IEnumerable<IResourceFileInfo> HashDirectory(string pathToDir, string rootPath, bool recursively = true,
            CancellationToken cancellationToken = default)
        {
            var files = new ConcurrentBag<string>(Directory.GetFiles(pathToDir, "*",
                recursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
            var azurFiles = new ConcurrentBag<IResourceFileInfo>();
            Parallel.ForEach(files, new ParallelOptions()
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = _options.ThreadsCount
                },
                path => { azurFiles.Add(HashFile(path, rootPath)); });
            return azurFiles;
        }
    }
}