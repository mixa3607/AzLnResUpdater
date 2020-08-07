namespace AzLn.Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            IUpdater updater = new Updater();
            updater.Start(args);
        }
    }
}