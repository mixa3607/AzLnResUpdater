using AzLn.Updater.Enums;

namespace AzLn.Updater.Options
{
    public interface IGameRegionInfo
    {
        public EAzurClientType Type { get; set; }
        public string Name { get; set; }
        public bool UseGit { get; set; }
        public bool UseTelegram { get; set; }
    }
}