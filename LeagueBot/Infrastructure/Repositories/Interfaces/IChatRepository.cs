using LeagueBot.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueBot.Infrastructure.Repositories.Interfaces
{
    public interface IChatRepository
    {
        IEnumerable<Chat> GetAllActiveChats();
        void RemoveChatByChatId(string chatId);
        void ActivateChat(string chatId);
        void DeactivateChat(string chatId);
    }
}
