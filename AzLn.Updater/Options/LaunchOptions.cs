using AzLn.Updater.Enums;

namespace AzLn.Updater.Options
{
    public class LaunchOptions : ILaunchOptions
    {
        public ELaunchType Type { get; set; }
        public string[] ClientNames { get; set; }
        public int UpdateRateSec { get; set; }
    }
}