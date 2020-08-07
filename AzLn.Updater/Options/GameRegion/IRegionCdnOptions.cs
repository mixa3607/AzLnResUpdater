namespace AzLn.Updater.Options
{
    public interface IRegionCdnOptions
    {
        string Platform { get; set; }
        string Url { get; set; }
        string UserAgent { get; set; }
        int DownloadThreads { get; set; }
    }
}