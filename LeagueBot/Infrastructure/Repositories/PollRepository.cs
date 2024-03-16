using System;
using System.Collections.Generic;
using System.Linq;
using LeagueBot.Domain;
using LeagueBot.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LeagueBot.Infrastructure.Repositories
{
    public class PollRepository : IPollRepository
    {
        private readonly ApplicationContext _context;
        
        public PollRepository(ApplicationContext context)
        {
            _context = context;
        }

        public void ChangePollState(DailyPoll dailyPoll)
        {
            dailyPoll.PollIsStarted = true;
            _context.SaveChanges();
        }

        public DailyPoll GetTodayPollByChatId(string chatId)
        {
            return _context.DailyPolls.Include(x => x.PollUserResults).ThenInclude(p => p.User).Where(x => x.ChatId == chatId && x.PollDate.Date == DateTime.Now.Date).FirstOrDefault();
        }

    }
}
