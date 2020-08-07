using System.Collections.Generic;
using AzLn.GameClient.Options;

namespace AzLn.Updater.Options
{
    public class UpdaterOptions : IUpdaterOptions
    {
        public TelegramOptions Telegram { get; set; }
        public FileHasherOptions FileHasher { get; set; }
        public DrawerOptions Drawer { get; set; }
        public LaunchOptions Launch { get; set; }
        public List<GameRegionOptions> GameRegions { get; set; } = new List<GameRegionOptions>();
        public ConnectionOptions Connection { get; set; }

        ITelegramOptions IUpdaterOptions.Telegram => Telegram;
        IFileHasherOptions IUpdaterOptions.FileHasher => FileHasher;
        IDrawerOptions IUpdaterOptions.Drawer => Drawer;
        ILaunchOptions IUpdaterOptions.Launch => Launch;
        IEnumerable<IGameRegionOptions> IUpdaterOptions.GameRegions => GameRegions;
        IConnectionOptions IUpdaterOptions.Connection => Connection;
    }
}