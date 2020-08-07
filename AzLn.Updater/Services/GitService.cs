using System;
using System.IO;
using AzLn.Updater.DataModels;
using AzLn.Updater.Options;
using LibGit2Sharp;
using NLog;

namespace AzLn.Updater
{
    public class GitService : IGitService
    {
        private readonly IRegionGitOptions _options;
        private readonly ILogger _logger;

        private Repository _repository;

        public GitService(ILogger logger, IRegionGitOptions options)
        {
            _logger = logger;
            _options = options;
        }

        public void Init()
        {
            _logger.Debug("Start git init");
            if (Repository.IsValid(_options.Path))
            {
                _logger.Debug("Git repo already created");
            }
            else
            {
                Repository.Init(_options.Path);
                _logger.Debug("Git repo created");
            }

            _repository = new Repository(_options.Path);
            _logger.Debug("Check upstream");
            var upstream = _repository.Network.Remotes["origin"];
            if (upstream == null)
            {
                _logger.Debug("Add origin");
                _repository.Network.Remotes.Add("origin", _options.Url, "");
            }
            else if (upstream.Url != _options.Url)
            {
                _logger.Debug("Update origin url");
                _repository.Network.Remotes.Update("origin", x => x.Url = _options.Url);
            }

            FixLinkingArtifacts();
            _logger.Debug("Repo initialized");
        }

        public void CommitBranchUpdate(UpdateBranchResult branchUpdate)
        {
            if (!branchUpdate.Files.HaveChanges)
            {
                return;
            }

            _logger.Debug("Add new files");
            Commands.Stage(_repository, "*");
            var sign = new Signature(_options.UserName, _options.Email, DateTimeOffset.Now);
            var message =
                $"{branchUpdate.BranchType}: {branchUpdate.FromVersion.Code} => {branchUpdate.ToVersion.Code}";
            try
            {
                _repository.Commit(message, sign, sign, new CommitOptions() {AllowEmptyCommit = false});
            }
            catch (Exception)
            {
                //ign
            }

            FixLinkingArtifacts();
        }

        public void Push()
        {
            if (!_options.AutoPush)
            {
                _logger.Debug("Auto push disabled. skip");
                return;
            }

            FixLinkingArtifacts();
            var options = new PushOptions
            {
                CredentialsProvider = (url, usernameFromUrl, types) =>
                    new UsernamePasswordCredentials()
                    {
                        Username = _options.Login,
                        Password = _options.Password
                    }
            };
            _repository.Network.Push(_repository.Branches["master"], options);
        }

        private void FixLinkingArtifacts()
        {
            var dirs = Directory.GetDirectories(_options.Path, "_git2_*");
            foreach (var dir in dirs)
            {
                Directory.Delete(dir);
            }
        }
    }
}