using System.Collections.Generic;
using System.Linq;
using Autofac;
using AzLn.Updater.Options;

namespace AzLn.Updater.Factories
{
    public interface IBranchUpdaterFactory
    {
        public IBranchUpdater GetUpdater(IResourceBranchOptions options);
        public IEnumerable<IBranchUpdater> GetUpdaters(IEnumerable<IResourceBranchOptions> optionsSets);
    }

    public class BranchUpdaterFactory : IBranchUpdaterFactory
    {
        private readonly ILifetimeScope _scope;

        public BranchUpdaterFactory(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public IBranchUpdater GetUpdater(IResourceBranchOptions options)
        {
            var scope = _scope.BeginLifetimeScope(GlobalConst.BranchUpdaterScopeName, c =>
            {
                c.Register(ctx => options); //bug
                c.Register(ctx => options.Store);
                c.Register(ctx => options.Obb);
            });
            return scope.Resolve<IBranchUpdater>();
        }

        public IEnumerable<IBranchUpdater> GetUpdaters(IEnumerable<IResourceBranchOptions> optionsSets)
        {
            return optionsSets.Select(GetUpdater);
        }
    }
}