using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueBot.Domain
{
    public class DailyPoll : BaseEntity
    {
        public bool PollIsStarted { get; set; } = false;
        public string ChatId { get; set; }
        public TimeOnly TimeToStartVote { get; set; }
        public TimeOnly TimeToStartGame { get; set; }
        public DateTime PollDate { get; set; }
        public List<PollUserResult> PollUserResults { get; set; } = new List<PollUserResult>();
    }
}
