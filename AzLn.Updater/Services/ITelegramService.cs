using System.Threading.Tasks;
using AzLn.Updater.DataModels;
using AzLn.Updater.Enums;

namespace AzLn.Updater
{
    public interface ITelegramService
    {
        void Init();
        Task SendBranchUpdateAsync(UpdateBranchResult updateResult, EAzurClientType type, string name);
    }
}