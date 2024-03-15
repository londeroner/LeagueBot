using LeagueBot.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeagueBot.Services
{
    public class LifeTimeCycle : ILifeTimeCycle
    {
        private readonly Timer _timer;
        public LifeTimeCycle()
        {

        }

        public void Update(object param)
        {
            throw new NotImplementedException();
        }
    }
}
