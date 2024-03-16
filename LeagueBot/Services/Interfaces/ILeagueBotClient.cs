using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeagueBot.Services.Interfaces
{
    public interface ILeagueBotClient
    {
        Task<string> StartPollAsync(string chatId);
        void NotifyPollUsers(string chatId, IEnumerable<string> userNames);
    }
}
