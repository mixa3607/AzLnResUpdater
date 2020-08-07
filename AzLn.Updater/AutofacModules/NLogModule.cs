using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving;
using AzLn.Updater.Options;
using NLog;

namespace AzLn.Updater.AutofacModules
{
    public class NLogModule : Autofac.Module
    {
        private static void InjectLoggerProperties(object instance)
        {
            var instanceType = instance.GetType();

            // Get all the injectable properties to set.
            // If you wanted to ensure the properties were only UNSET properties,
            // here's where you'd do it.
            var properties = instanceType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(ILogger) && p.CanWrite && p.GetIndexParameters().Length == 0);

            // Set the properties located.
            foreach (var propToSet in properties)
            {
                propToSet.SetValue(instance, LogManager.GetLogger(instanceType.FullName), null);
            }
        }

        /*private static void OnComponentPreparing(object sender, PreparingEventArgs e)
        {
            e.Parameters = e.Parameters.Union(
                new[]
                {
                    new ResolvedParameter(
                        (p, i) => p.ParameterType == typeof (ILogger),
                        (p, i) => PrepareLogger(i as IInstanceLookup, p.Member.DeclaringType.FullName))
                });
        }

        private static ILogger PrepareLogger(IInstanceLookup instanceLookup, string loggerName)
        {
            if (instanceLookup == null)
            {
                return LogManager.GetLogger(loggerName);
            }
            var scopeTag = instanceLookup.ActivationScope.Tag as string;
            var scope = instanceLookup.ActivationScope;
            if (scopeTag == GlobalConst.ClientUpdaterScopeName || scopeTag == GlobalConst.BranchUpdaterScopeName)
            {
                var clientOptions = scope.Resolve<AzurClientOptions>();
                loggerName += "_" + clientOptions.Info.Type;
                //logger.SetProperty("ClientName", clientOptions.Info.Name);
                if (scopeTag == GlobalConst.BranchUpdaterScopeName)
                {
                    var branchOptions = scope.Resolve<ResourceBranchOptions>();
                    loggerName += "." + branchOptions.Type;
                }
            }

            return LogManager.GetLogger(loggerName);
        }*/

        private static void OnComponentPreparing(object? sender, PreparingEventArgs e)
        {
            e.Parameters = e.Parameters.Union(
                new[]
                {
                    new ResolvedParameter(
                        (p, i) => p.ParameterType == typeof (ILogger),
                        (p, i) => LogManager.GetLogger(p.Member.DeclaringType.FullName))
                });
        }

        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
        {
            
            // Handle constructor parameters.
            registration.Preparing += OnComponentPreparing;

            // Handle properties.
            //registration.Activated += (sender, e) => InjectLoggerProperties(e.Instance);
        }
    }
}