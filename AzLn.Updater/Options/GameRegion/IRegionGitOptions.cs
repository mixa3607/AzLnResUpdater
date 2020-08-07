namespace AzLn.Updater.Options
{
    public interface IRegionGitOptions
    {
        bool AutoPush { get; set; }
        string Branch { get; set; }
        string Url { get; set; }
        string Path { get; set; }
        string Login { get; set; }
        string Password { get; set; }
        string UserName { get; set; }
        string Email { get; set; }
    }
}