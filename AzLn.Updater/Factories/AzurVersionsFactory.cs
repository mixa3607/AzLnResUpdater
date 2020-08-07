using System.Collections.Generic;
using AzLn.Protocol.P10;
using AzLn.Updater.DataModels;

namespace AzLn.Updater.Factories
{
    //public interface IAzurVersionsFactory
    //{
    //    ResourceBranchVersion[] GetVersions(Sc10801 command);
    //}
    //
    //public class AzurVersionsFactory : IAzurVersionsFactory
    //{
    //    public ResourceBranchVersion[] GetVersions(Sc10801 command)
    //    {
    //        var vers = new List<ResourceBranchVersion>();
    //        foreach (var s in command.Version)
    //        {
    //            if (ResourceBranchVersion.TryParse(s, out var ver))
    //            {
    //                vers.Add(ver);
    //            }
    //        }
    //        //return new AzurVersions(command);
    //        return vers.ToArray();
    //    }
    //}
}