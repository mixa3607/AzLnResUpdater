namespace AzLn.Updater.Options
{
    public class RegionGitOptions : IRegionGitOptions
    {
        public bool AutoPush { get; set; }
        public string Branch { get; set; }
        public string Url { get; set; }
        public string Path { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}