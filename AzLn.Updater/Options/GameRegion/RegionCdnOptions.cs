namespace AzLn.Updater.Options
{
    public class RegionCdnOptions : IRegionCdnOptions
    {
        public string Platform { get; set; }
        public string Url { get; set; }
        public string UserAgent { get; set; }
        public int DownloadThreads { get; set; }
    }
}