using System;
using AzLn.Updater.Enums;

namespace AzLn.Updater.DataModels
{
    public class ResourceBranchVersion
    {
        public EResourceBranchType Type { get; private set; }
        public string Code { get; private set; }
        public string Hash { get; private set; }
        public string RawString { get; private set; }

        private ResourceBranchVersion()
        {
        }

        public ResourceBranchVersion(string code, EResourceBranchType type)
        {
            Type = type;
            Code = code;
            Hash = "GeneratedVersion";
            RawString = $"${TypeToString(Type)}${Code}${Hash}";
        }

        public static bool TryParse(string stringVersion, out ResourceBranchVersion azurVersion)
        {
            try
            {
                azurVersion = Parse(stringVersion);
                return true;
            }
            catch (Exception)
            {
                azurVersion = null;
                return false;
            }
        }

        public static ResourceBranchVersion Parse(string stringVersion)
        {
            var splVer = stringVersion.Split('$');
            if (splVer.Length < 4)
            {
                throw new ArgumentException();
            }

            var version = new ResourceBranchVersion
            {
                Type = ParseType(splVer[1]),
                Code = string.Join('.', splVer[2..^1]),
                Hash = splVer[^1],
                RawString = stringVersion
            };

            return version;
        }

        private static EResourceBranchType ParseType(string type)
        {
            return type switch
            {
                "azhash" => EResourceBranchType.Main,
                "l2dhash" => EResourceBranchType.L2d,
                "cvhash" => EResourceBranchType.Cv,
                "pichash" => EResourceBranchType.Pic,
                "bgmhash"=> EResourceBranchType.Bgm,
                _ => throw new ArgumentException()
            };
        }

        private static string TypeToString(EResourceBranchType type)
        {
            return type switch
            {
                EResourceBranchType.Main => "azhash",
                EResourceBranchType.L2d => "l2dhash",
                EResourceBranchType.Cv => "cvhash",
                EResourceBranchType.Pic => "pichash",
                EResourceBranchType.Bgm => "bgmhash",
                _ => throw new ArgumentException(),
            };
        }

        public override string ToString()
        {
            return $"Type: {Type}, Code: {Code}, Hash: {Hash}";
        }
    }
}