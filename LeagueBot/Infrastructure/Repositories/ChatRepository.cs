using LeagueBot.Domain;
using LeagueBot.Infrastructure.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace LeagueBot.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationContext _context;

        public ChatRepository(ApplicationContext context)
        {
            _context = context;
        }

        public IEnumerable<Chat> GetAllActiveChats()
        {
            return _context.Chats.Where(x => x.IsActiveForPoll);
        }

        public void RemoveChatByChatId(string chatId)
        {
            _context.Chats.Remove(_context.Chats.FirstOrDefault(x => x.ChatId == chatId));
            _context.SaveChanges();
        }

        public void ActivateChat(string chatId)
        {
            _context.Chats.FirstOrDefault(x => x.ChatId == chatId).IsActiveForPoll = true;
            _context.SaveChanges();
        }

        public void DeactivateChat(string chatId)
        {
            _context.Chats.FirstOrDefault(x => x.ChatId == chatId).IsActiveForPoll = false;
            _context.SaveChanges();
        }
    }
}
