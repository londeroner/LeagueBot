using System;
using System.Collections.Generic;
using System.Linq;
using LeagueBot.Domain;
using LeagueBot.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeagueBot.Infrastructure.Repositories
{
    public class PollRepository : IPollRepository
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<PollRepository> _logger;
        
        public PollRepository(ILogger<PollRepository> logger, ApplicationContext context)
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

        public void AddPollAnswer(User user, int answer, string pollId)
        {
            var poll = _context.DailyPolls.Include(x => x.PollUserResults).ThenInclude(x => x.User).FirstOrDefault(x => x.PollId == pollId);

            if (poll == null)
                return;

            poll.PollUserResults.Add(new PollUserResult() { DailyPoll = poll, PollResult = (PollResult)answer, User = user });
            _context.SaveChanges();
        }

        public void StartPoll(DailyPoll dailyPoll, string pollId)
        {
            dailyPoll.PollId = pollId;
            dailyPoll.PollIsStarted = true;
            _context.SaveChanges();
        }

        public void StopPoll(DailyPoll dailyPoll)
        {
            dailyPoll.PollIsFinished = true;
            _context.SaveChanges();
        }
    }
}
