using System;

namespace AzLn.Updater.DataModels
{
    public class ResourceFileInfo : IResourceFileInfo
    {
        public string Path { get; set; } = "EMPTY";
        public long Size { get; set; }
        public string Md5Hash { get; set; } = "EMPTY";

        public ResourceFileInfo()
        {
        }

        public ResourceFileInfo(string rawCsvString)
        {
            LoadFromCsvString(rawCsvString);
        }

        public void LoadFromCsvString(string rawCsvString)
        {
            var splFile = rawCsvString.Split(',');
            if (splFile.Length != 3)
            {
                throw new ArgumentException("Splitting error " + rawCsvString);
            }

            Path = splFile[0];
            Size = long.Parse(splFile[1]);
            Md5Hash = splFile[2];
        }

        public override string ToString()
        {
            return $"{Path},{Size},{Md5Hash}";
        }
    }
}