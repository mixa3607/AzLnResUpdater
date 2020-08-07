namespace AzLn.Updater.Options
{
    public interface ITelegramOptions
    {
        string BotKey { get; set; }
        long ChatId { get; set; }
    }
}