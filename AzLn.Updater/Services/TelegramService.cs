using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzLn.Contract.Extensions;
using AzLn.Updater.DataModels;
using AzLn.Updater.Enums;
using AzLn.Updater.Options;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AzLn.Updater
{
    public class TelegramService : ITelegramService
    {
        private readonly ITelegramOptions _options;
        private readonly IDrawer _drawer;
        private readonly ILogger _logger;
        private readonly object _initLocker = new object();

        private TelegramBotClient? _botClient;

        public TelegramService(ILogger logger, ITelegramOptions options, IDrawer drawer)
        {
            _drawer = drawer;
            _options = options;
            _logger = logger;
        }

        public void Init()
        {
            lock (_initLocker)
            {
                if (_botClient != null)
                    return;
                _botClient = new TelegramBotClient(_options.BotKey);
                _botClient.GetMeAsync().Wait();
            }
        }

        public async Task SendBranchUpdateAsync(UpdateBranchResult updateResult, EAzurClientType type, string name)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Name: " + name);
            sb.AppendLine("Type: " + type);
            sb.AppendLine($"{updateResult.FromVersion.Type}: {updateResult.FromVersion.Code} => {updateResult.ToVersion.Code}");
            sb.AppendLine($"A: {updateResult.Files.AddedFiles.Count()}|U: {updateResult.Files.UpdatedFiles.Count()}|D: {updateResult.Files.DeletedFiles.Count()}");
            await _botClient.SendTextMessageAsync(new ChatId(_options.ChatId), sb.ToString());


            await using var textStream = new MemoryStream();
            await using var streamWriter = new StreamWriter(textStream);

            await WriteFilesToStream("Added files", updateResult.Files.AddedFiles, streamWriter);
            await WriteFilesToStream("Updated files", updateResult.Files.UpdatedFiles, streamWriter);
            await WriteFilesToStream("Deleted files", updateResult.Files.DeletedFiles, streamWriter);
            streamWriter.Flush();
            textStream.Position = 0;

            var thumbStream = new MemoryStream();
            _drawer.DrawText(updateResult.Files.AddedFiles.Count().ToString(),
                    updateResult.Files.UpdatedFiles.Count() + "|" + updateResult.Files.DeletedFiles.Count())
                .Save(thumbStream, ImageFormat.Png);
            thumbStream.Position = 0;

            await _botClient.SendDocumentAsync(new ChatId(_options.ChatId),
                new InputMedia(textStream, updateResult.BranchType + ".csv"), disableNotification: true,
                thumb: new InputMedia(thumbStream, "th"));
        }

        private async Task WriteFilesToStream(string sectionName, IEnumerable<IResourceFileInfo> azurFiles,
            StreamWriter writer)
        {
            await writer.WriteLineAsync($"{sectionName} ({azurFiles.Count()})");
            azurFiles.ForEach(writer.WriteLine);
            await writer.WriteLineAsync();
        }
    }
}