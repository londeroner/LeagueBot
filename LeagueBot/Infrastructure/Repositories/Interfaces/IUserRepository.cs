using LeagueBot.Domain;

namespace LeagueBot.Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository
    {
        User GetUserByUserName(string userName);
    }
}
