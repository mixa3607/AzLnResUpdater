using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AzLn.Contract.Extensions;
using AzLn.GameClient;
using AzLn.Updater.AutofacModules;
using AzLn.Updater.Downloaders;
using AzLn.Updater.Enums;
using AzLn.Updater.Factories;
using AzLn.Updater.Options;
using Microsoft.Extensions.Configuration;
using NLog;

namespace AzLn.Updater
{
    public interface IUpdater
    {
        void Start(string[] args);
        Task StartAsync(string[] args);
        Task StopAsync();
    }

    public class Updater : IUpdater
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private CancellationTokenSource _cts = new CancellationTokenSource();
        private Task _task = Task.CompletedTask;

        public void Start(string[] args)
        {
            Run(Build(args)).Wait();
        }

        public async Task StartAsync(string[] args)
        {
            _task = Run(Build(args), _cts.Token);
        }

        public async Task StopAsync()
        {
            _cts.Cancel();
            await _task;
        }

        public static IUpdaterOptions BuildConfiguration(string[] args)
        {
            Logger.Info("Build config");
            var rootDir = Directory.GetCurrentDirectory();
            var regionConfigsDir = Path.Combine(rootDir, "configs");
            var regionConfigsPaths = Directory.GetFiles(regionConfigsDir, "*.json");

            //var env = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder().SetBasePath(rootDir);
            var mainOptions = builder
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                //.AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build()
                .Get<UpdaterOptions>();
            foreach (var regionCfgPath in regionConfigsPaths)
            {
                Logger.Trace("Loading {Path} config", regionCfgPath);
                var regionBuilder = new ConfigurationBuilder()
                    .SetBasePath(rootDir)
                    .AddJsonFile(regionCfgPath, false, false)
                    .Build();
                var regionOpts= regionBuilder.Get<GameRegionOptions>();
                mainOptions.GameRegions.Add(regionOpts);
                Logger.Debug("Config {Path} loaded", regionCfgPath);
            }

            Logger.Info("Config builded");
            return mainOptions;
        }


        public static IContainer Build(string[] args)
        {
            Logger.Info("Build container");
            var builder = new ContainerBuilder();
            builder.RegisterModule<NLogModule>();

            var options = BuildConfiguration(args);
            builder.Register(x => options).As<IUpdaterOptions>().SingleInstance();
            builder.Register(x => x.Resolve<IUpdaterOptions>().Launch).SingleInstance();
            builder.Register(x => x.Resolve<IUpdaterOptions>().Telegram).SingleInstance();
            builder.Register(x => x.Resolve<IUpdaterOptions>().GameRegions).SingleInstance();
            builder.Register(x => x.Resolve<IUpdaterOptions>().Connection).SingleInstance();

            builder.RegisterType<Drawer>().As<IDrawer>().SingleInstance();
            builder.Register(x => x.Resolve<IUpdaterOptions>().FileHasher).As<IFileHasherOptions>().SingleInstance();

            builder.RegisterType<FileHasherService>().As<IFileHasherService>().SingleInstance();
            builder.Register(x => x.Resolve<IUpdaterOptions>().Drawer).As<IDrawerOptions>().SingleInstance();

            builder.RegisterType<DownloadHelper>().As<IDownloadHelper>().SingleInstance();
            builder.RegisterType<HashComputer>().As<IHashComputer>().SingleInstance();
            builder.RegisterType<HashesCsvHelper>().As<IHashesCsvHelper>().SingleInstance();
            builder.RegisterType<TelegramService>().As<ITelegramService>().SingleInstance();
            //builder.RegisterType<AzurVersionsFactory>().As<IAzurVersionsFactory>().SingleInstance();
            builder.RegisterType<ResourceFilesInfoComparerService>().As<IResourceFilesInfoComparerService>().SingleInstance();

            builder.RegisterType<CdnDownloadService>().As<ICdnDownloadService>()
                .InstancePerMatchingLifetimeScope(GlobalConst.ClientUpdaterScopeName);
            builder.RegisterType<RegionUpdater>().As<IRegionUpdater>()
                .InstancePerMatchingLifetimeScope(GlobalConst.ClientUpdaterScopeName);
            builder.RegisterType<GitService>().As<IGitService>()
                .InstancePerMatchingLifetimeScope(GlobalConst.ClientUpdaterScopeName);
            builder.RegisterType<AzurClient>().As<IAzurClient>()
                .InstancePerMatchingLifetimeScope(GlobalConst.ClientUpdaterScopeName);
            builder.RegisterType<BranchUpdaterFactory>().As<IBranchUpdaterFactory>()
                .InstancePerMatchingLifetimeScope(GlobalConst.ClientUpdaterScopeName);

            builder.RegisterType<BranchUpdater>().As<IBranchUpdater>()
                .InstancePerMatchingLifetimeScope(GlobalConst.BranchUpdaterScopeName);
            builder.RegisterType<ObbService>().As<IObbService>()
                .InstancePerMatchingLifetimeScope(GlobalConst.BranchUpdaterScopeName);
            Logger.Info("Container builded");
            return builder.Build();
        }

        public static IEnumerable<IRegionUpdater> GetUpdaters(IContainer container)
        {
            Logger.Info("Create region updaters");
            var regionCfgs = container.Resolve<IEnumerable<IGameRegionOptions>>();
            var updaters = new List<IRegionUpdater>();
            foreach (var regionCfg in regionCfgs)
            {
                using (MappedDiagnosticsLogicalContext.SetScoped("RegionType", regionCfg.Type))
                {
                    MappedDiagnosticsLogicalContext.Set("RegionName", regionCfg.Name);
                    var scope = container.BeginLifetimeScope(GlobalConst.ClientUpdaterScopeName,
                        b =>
                        {
                            b.Register(x => regionCfg);
                            b.Register(x => regionCfg.Branches);
                            b.Register(x => regionCfg.Gate);
                            b.Register(x => regionCfg.Obb);
                            b.Register(x => regionCfg.Git);
                            b.Register(x => regionCfg.Cdn);
                        });
                    var updater = scope.Resolve<IRegionUpdater>();
                    Logger.Info("Init {Name} updater", updater.RegionInfo.Name);
                    updater.InitGit();
                    updater.InitTelegram();
                    updater.InitAllBranches();
                    Logger.Info("{Name} updater initialized", updater.RegionInfo.Name);
                    updaters.Add(updater);
                }
            }

            Logger.Info("All updaters created ({Total})", updaters.Count);
            return updaters;
        }

        public static async Task Run(IContainer container, CancellationToken cancellationToken = default)
        {
            var updaters = GetUpdaters(container);
            var launchOpts = container.Resolve<ILaunchOptions>();
            switch (launchOpts.Type)
            {
                case ELaunchType.ExtractObb:
                    UpdateFromObb(updaters, launchOpts);
                    break;
                case ELaunchType.Repair:
                    Repair(updaters, launchOpts);
                    break;
                case ELaunchType.UpdateOnce:
                    await UpdateOnceAsync(updaters, launchOpts, cancellationToken);
                    break;
                case ELaunchType.UpdateLoop:
                    await UpdateLoop(updaters, launchOpts, cancellationToken);
                    break;
                default:
                    Logger.Fatal("Unknown action type");
                    break;
            }
        }

        private static async Task UpdateLoop(IEnumerable<IRegionUpdater> clientUpdaters, ILaunchOptions options,
            CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var iterStartTime = DateTime.Now;
                await UpdateOnceAsync(clientUpdaters, options, cancellationToken);
                var execTime = DateTime.Now - iterStartTime;
                if (execTime.TotalSeconds > options.UpdateRateSec)
                {
                    Logger.Warn("Total execution time more then updateRate ({Rate}sec). Increase value in config",
                        options.UpdateRateSec);
                }

                var sleepSecs = options.UpdateRateSec - (int)execTime.TotalSeconds;
                while (sleepSecs < 0)
                {
                    sleepSecs += options.UpdateRateSec;
                }

                await Task.Delay(sleepSecs * 1000, cancellationToken);
            }
        }

        private static async Task UpdateOnceAsync(IEnumerable<IRegionUpdater> clientUpdaters, ILaunchOptions options, CancellationToken cancellationToken)
        {
            var matchedClientUpdaters = clientUpdaters
                .Where(x => options.ClientNames.Any(y => y == x.RegionInfo.Name));
            foreach (var updater in matchedClientUpdaters)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var versions = await updater.GetVersionsFromGateAsync();
                foreach (var resBranchType in updater.ResBranches)
                {
                    if (cancellationToken.IsCancellationRequested) //
                        return;
                    await updater.UpdateBranchAsync(resBranchType, versions);
                }
            }
        }

        private static void Repair(IEnumerable<IRegionUpdater> clientUpdaters, ILaunchOptions options)
        {
            var matchedClientUpdaters = clientUpdaters
                .Where(x => options.ClientNames.Any(y => y == x.RegionInfo.Name));
            foreach (var updater in matchedClientUpdaters)
            {
                updater.ResBranches.ForEach(x => updater.ValidateAndFixBranch(x));
            }
        }

        private static void UpdateFromObb(IEnumerable<IRegionUpdater> clientUpdaters, ILaunchOptions options)
        {
            var matchedClientUpdaters = clientUpdaters
                .Where(x => options.ClientNames.Any(y => y == x.RegionInfo.Name));
            foreach (var updater in matchedClientUpdaters)
            {
                updater.ResBranches.ForEach(x => updater.UpdateBranchFromObb(x));
            }
        }
    }
}
