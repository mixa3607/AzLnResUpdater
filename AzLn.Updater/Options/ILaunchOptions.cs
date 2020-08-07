using AzLn.Updater.Enums;

namespace AzLn.Updater.Options
{
    public interface ILaunchOptions
    {
        ELaunchType Type { get; set; }
        string[] ClientNames { get; set; }
        int UpdateRateSec { get; set; }
    }
}