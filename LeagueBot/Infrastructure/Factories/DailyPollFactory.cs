using LeagueBot.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueBot.Infrastructure.Factories
{
    public static class DailyPollFactory
    {
        public static DailyPoll CreateDailyPoll(string ChatId, 
            TimeOnly TimeToStartVote, TimeOnly TimeToStartGame)
        {
            return new DailyPoll()
            {
                ChatId = ChatId,
                TimeToStartVote = TimeToStartVote,
                TimeToStartGame = TimeToStartGame,
                PollDate = DateTime.Now.Date
            };
        }
    }
}
