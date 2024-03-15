using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueBot.Domain
{
    public class PollUserResult : BaseEntity
    {
        public User User { get; set; }
        public int DailyPollId { get; set; }
        public DailyPoll DailyPoll { get; set; }
        public PollResult PollResult { get; set; }
    }

    public enum PollResult
    {
        [Description]
        Yes = 0,
        No = 1
    }
}
