using LeagueBot.Domain;
using System.Collections.Generic;

namespace LeagueBot.Infrastructure.Repositories.Interfaces
{
    public interface IPollRepository
    {
        DailyPoll GetTodayPollByChatId(string chatId);
        void ChangePollState(DailyPoll dailyPoll);
        void AddPollAnswer(User user, int answer, string pollId);
        void StartPoll(DailyPoll dailyPoll, string pollId);
        void StopPoll(DailyPoll dailyPoll);
    }
}
