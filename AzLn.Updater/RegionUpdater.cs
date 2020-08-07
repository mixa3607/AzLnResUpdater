using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using AzLn.Contract.Extensions;
using AzLn.GameClient;
using AzLn.Protocol;
using AzLn.Protocol.P10;
using AzLn.Updater.DataModels;
using AzLn.Updater.Enums;
using AzLn.Updater.Exceptions;
using AzLn.Updater.Factories;
using AzLn.Updater.Options;
using NLog;

namespace AzLn.Updater
{
    public class RegionUpdater : IRegionUpdater
    {
        public IGameRegionInfo RegionInfo => _regionOptions;
        public EResourceBranchType[] ResBranches => _branchUpdaters.Keys.ToArray();

        private readonly IGameRegionOptions _regionOptions;
        private readonly IRegionGateOptions _gateOptions;
        private readonly ILogger _logger;
        private readonly Dictionary<EResourceBranchType, IBranchUpdater> _branchUpdaters;
        private readonly IAzurClient _client;
        private readonly IGitService _gitService;
        private readonly ITelegramService _telegramService;

        //private ResourceBranchVersion[] _versions = new ResourceBranchVersion[0];

        public RegionUpdater(ILogger logger, IAzurClient client, IGameRegionOptions regionOptions,
            IBranchUpdaterFactory branchUpdaterFactory, IGitService gitService, 
            ITelegramService telegramService, IRegionGateOptions gateOptions)
        {
            _logger = logger;

            _regionOptions = regionOptions;
            _gateOptions = gateOptions;

            _telegramService = telegramService;
            _gitService = gitService;
            _branchUpdaters = new Dictionary<EResourceBranchType, IBranchUpdater>();
            branchUpdaterFactory.GetUpdaters(_regionOptions.Branches).ForEach(x => _branchUpdaters[x.BranchType] = x);
            _client = client;
        }

        private void GitPostUpdate(UpdateBranchResult updateResult)
        {
            if (!RegionInfo.UseGit)
            {
                _logger.Debug("Git disabled in config. skip");
                return;
            }
            _gitService.CommitBranchUpdate(updateResult);
            _gitService.Push();
        }

        public void InitGit()
        {
            if (!RegionInfo.UseGit)
            {
                _logger.Debug("Git disabled in config. skip");
                return;
            }
            _gitService.Init();
        }

        public void InitTelegram()
        {
            if (!RegionInfo.UseTelegram)
            {
                _logger.Debug("Git disabled in config. skip");
                return;
            }
            _telegramService.Init();
        }

        private void TelegramPostUpdate(UpdateBranchResult updateResult)
        {
            if (!RegionInfo.UseTelegram)
            {
                _logger.Debug("Telegram disabled in config. skip");
                return;
            }

            _telegramService.SendBranchUpdateAsync(updateResult, RegionInfo.Type, RegionInfo.Name).Wait();
        }

        public UpdateBranchResult UpdateBranchFromObb(EResourceBranchType branchType)
        {
            var result = _branchUpdaters[branchType].UpdateFromObb();
            if (result.Files.HaveChanges)
            {
                GitPostUpdate(result);
                TelegramPostUpdate(result);
            }
            return result;
        }

        public async Task<UpdateBranchResult> UpdateBranchAsync(EResourceBranchType branchType, IReadOnlyCollection<ResourceBranchVersion> versions, bool force = false)
        {
            var result = await _branchUpdaters[branchType].UpdateAsync(versions.First(x => x.Type == branchType), force);
            if (result.Files.HaveChanges)
            {
                GitPostUpdate(result);
                TelegramPostUpdate(result);
            }
            return result;
        }

        public ResourceBranchVersion GetBranchVersionFromObb(EResourceBranchType branchType)
        {
            return _branchUpdaters[branchType].GetVersionFromObb();
        }

        public ResourceBranchVersion GetBranchVersion(EResourceBranchType branchType)
        {
            return _branchUpdaters[branchType].GetVersion();
        }

        public void ValidateAndFixBranch(EResourceBranchType branchType)
        {
            var validatingResult = ValidateBranch(branchType);
            if (validatingResult.HaveChanges)
            {
                FixBranch(branchType, validatingResult);
            }

            var ver = GetBranchVersion(branchType);
            var result = new UpdateBranchResult()
            {
                BranchType = branchType,
                Files = validatingResult,
                FromVersion = ver,
                ToVersion = ver
            };
            if (result.Files.HaveChanges)
            {
                GitPostUpdate(result);
                //TelegramPostUpdate(result);
            }
        }

        public ICompareResult ValidateBranch(EResourceBranchType branchType)
        {
            try
            {
                return _branchUpdaters[branchType].ValidateResources();
            }
            catch (CsvFileNotFoundException e)
            {
                _logger.Error("Csv file {File} not found", e.FilePath);
                throw;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unhandled exception on validating branch {BranchType}", branchType);
                throw;
            }
        }

        public void FixBranch(EResourceBranchType branchType, ICompareResult validatingResult)
        {
            _branchUpdaters[branchType].FixResourcesAsync(validatingResult).Wait();
        }

        public async Task<IReadOnlyCollection<ResourceBranchVersion>> GetVersionsFromGateAsync()
        {
            using var tcp = new TcpClient(_gateOptions.Host, _gateOptions.Port);
            using var client = _client;
            await client.StartAsync(tcp.GetStream());
            var cs10800 = new Cs10800()
            {
                Platform = "0",
                State = 21,
                Index = 0
            };
            client.Write(cs10800);
            var command = await client.ReadAsync();
            await client.StopAsync();
            if (!(command.Type == AzurClientData.EDataType.Command && ((ICommand)command.Data).CommandId == 10801))
            {
                throw new Exception($"Receive unknown data but expected 10801 command");
            }
            var versions = new List<ResourceBranchVersion>();
            foreach (var s in ((Sc10801)command.Data).Version)
            {
                if (ResourceBranchVersion.TryParse(s, out var ver))
                    versions.Add(ver);
            }
            return versions;
        }

        public void InitAllBranches()
        {
            _branchUpdaters.ForEach(x => x.Value.InitPaths());
        }

        public IEnumerable<ICompareResult> ValidateAllBranches()
        {
            var validateResults = new List<ICompareResult>();
            foreach (var branchUpdater in _branchUpdaters.Select(x => x.Value))
            {
                try
                {
                    validateResults.Add(branchUpdater.ValidateResources());
                }
                catch (CsvFileNotFoundException e)
                {
                    _logger.Error("Csv file {File} not found. Skip branch", e.FilePath);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Unhandled exception on validating branch {BranchType}", branchUpdater.BranchType);
                    throw;
                }
            }

            return validateResults;
        }
    }
}