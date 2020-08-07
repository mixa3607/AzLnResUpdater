using System.Collections.Generic;
using AzLn.GameClient.Options;

namespace AzLn.Updater.Options
{
    public interface IUpdaterOptions
    {
        ITelegramOptions Telegram { get; }
        IFileHasherOptions FileHasher { get; }
        IDrawerOptions Drawer { get; }
        ILaunchOptions Launch { get; }
        IEnumerable<IGameRegionOptions> GameRegions { get; }
        IConnectionOptions Connection { get; }
    }
}