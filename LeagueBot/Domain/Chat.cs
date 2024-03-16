using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueBot.Domain
{
    public class Chat : BaseEntity
    {
        public string ChatId { get; set; }
        public bool IsActiveForPoll { get; set; }
    }
}
