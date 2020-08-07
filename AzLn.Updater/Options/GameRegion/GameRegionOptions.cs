using System.Collections.Generic;
using AzLn.Updater.Enums;

namespace AzLn.Updater.Options
{
    public interface IGameRegionOptions : IGameRegionInfo
    {
        //EAzurClientType Type { get; }
        //string Name { get; }
        //bool EnableGit { get; }
        //bool EnableTelegram { get; }
        IRegionGateOptions Gate { get; }
        IRegionCdnOptions Cdn { get; }
        IRegionGitOptions Git { get; }
        IRegionObbOptions Obb { get; }
        IEnumerable<IResourceBranchOptions> Branches { get; }
    }

    public class GameRegionOptions : IGameRegionOptions
    {
        public EAzurClientType Type { get; set; }
        public string Name { get; set; }
        public bool UseGit { get; set; }
        public bool UseTelegram { get; set; }

        public RegionGateOptions Gate { get; set; }
        public RegionCdnOptions Cdn { get; set; }
        public RegionGitOptions Git { get; set; }
        public RegionObbOptions Obb { get; set; }
        public List<ResourceBranchOptions> Branches { get; set; }

        IEnumerable<IResourceBranchOptions> IGameRegionOptions.Branches => Branches;
        IRegionGateOptions IGameRegionOptions.Gate => Gate;
        IRegionCdnOptions IGameRegionOptions.Cdn => Cdn;
        IRegionGitOptions IGameRegionOptions.Git => Git;
        IRegionObbOptions IGameRegionOptions.Obb => Obb;
    }
}