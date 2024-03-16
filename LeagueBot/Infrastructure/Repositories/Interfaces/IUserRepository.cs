using LeagueBot.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueBot.Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository
    {
        User GetUserByUserName(string userName);
    }
}
