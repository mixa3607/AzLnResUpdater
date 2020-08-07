using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzLn.Updater.DataModels;

namespace AzLn.Updater
{
    public class HashesCsvHelper : IHashesCsvHelper
    {
        public async Task<IReadOnlyCollection<IResourceFileInfo>> ReadAsync(IEnumerable<string> csvStrings, CancellationToken cancellationToken = default)
        {
            var files = new List<IResourceFileInfo>();
            foreach (var csvString in csvStrings)
            {
                files.Add(new ResourceFileInfo(csvString));
            }
            return files;
        }

        public async Task<IReadOnlyCollection<IResourceFileInfo>> ReadAsync(Stream csvStream, CancellationToken cancellationToken = default)
        {
            var files = new List<IResourceFileInfo>();
            var csvReader = new StreamReader(csvStream);
            do
            {
                var line = await csvReader.ReadLineAsync();
                if (line == null) continue;
                files.Add(new ResourceFileInfo(line));
            } while (!csvReader.EndOfStream);

            return files;
        }

        public async Task WriteAsync(IEnumerable<IResourceFileInfo> resFilesInfo, string destPath)
        {
            var csvFile = File.OpenWrite(destPath);
            foreach (var resourceFileInfo in resFilesInfo)
            {
                var bytes = Encoding.UTF8.GetBytes(resourceFileInfo.ToString()!);
                await csvFile.WriteAsync(bytes);
                await csvFile.WriteAsync(new byte[]{ 0x0a }); //lf
            }
            csvFile.Close();
        }
    }
}