using System.Collections.Generic;

namespace LeagueBot.Services.Interfaces
{
    public interface ILeagueBotClient
    {
        void StartPoll(string chatId);
        void NotifyPollUsers(string chatId, IEnumerable<string> userNames);
    }
}
