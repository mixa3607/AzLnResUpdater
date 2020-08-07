namespace AzLn.Updater.Options
{
    public class TelegramOptions : ITelegramOptions
    {
        public string BotKey { get; set; }
        public long ChatId { get; set; }
    }
}