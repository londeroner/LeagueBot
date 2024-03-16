using LeagueBot.Domain;
using System.Collections.Generic;

namespace LeagueBot.Infrastructure.Repositories.Interfaces
{
    public interface IPollRepository
    {
        DailyPoll GetTodayPollByChatId(string chatId);
        void ChangePollState(DailyPoll dailyPoll);
    }
}
