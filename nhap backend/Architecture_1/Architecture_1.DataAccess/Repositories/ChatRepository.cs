using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;
using Architecture_1.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;

namespace Architecture_1.DataAccess.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDbContext _dbContext;

        public ChatRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Chat> FindByIdAsync(int id)
        {
            return await _dbContext.Chats
                .Include(chat => chat.FromNavigation)
                .Include(chat => chat.ToNavigation)
                .FirstOrDefaultAsync(chat => chat.Id == id);
        }

        public async Task<IEnumerable<Chat>> FindByAccountId(int accountId)
        {
            return await _dbContext.Chats
                .Include(chat => chat.From)
                .Include(chat => chat.To)
                .Where(chat => chat.From == accountId || chat.To == accountId)
                .OrderByDescending(chat => chat.Created)
                .ToListAsync();
        }

        public async Task<IEnumerable<Chat>> FindBy1v1(int accountId1, int accountId2)
        {
            return await _dbContext.Chats
                .Include(chat => chat.FromNavigation)
                .Include(chat => chat.ToNavigation)
                .Where(chat => (chat.From == accountId1 && chat.To == accountId2) ||
                               (chat.From == accountId2 && chat.To == accountId1))
                .OrderByDescending(chat => chat.Created)
                .ToListAsync();
        }


        public async Task<IEnumerable<Account>> GetChatRoomsByAccountId(int accountId)
        {
            try
            {
                IEnumerable<Chat> chatList = await _dbContext.Chats
                                .Include(chat => chat.FromNavigation)
                                .Include(chat => chat.ToNavigation)
                                .Where(chat => (chat.From == accountId || chat.To == accountId))
                                .OrderByDescending(chat => chat.Created)
                                .ToListAsync();
                List<Account> accounts = chatList.Select(chat => chat.From == accountId ? chat.ToNavigation : chat.FromNavigation)
                .DistinctBy(acc => acc.Id)
                .ToList();
                return accounts;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Enumerable.Empty<Account>();
            }



        }
        public async Task<Chat> GetLastChatBy1v1(int fromAccountId, int toAccountId)
        {
            return await _dbContext.Chats
                            .Include(chat => chat.FromNavigation)
                            .Include(chat => chat.ToNavigation)
                            .Where(chat => (chat.From == fromAccountId && chat.To == toAccountId) ||
                                           (chat.From == toAccountId && chat.To == fromAccountId))
                            .OrderByDescending(chat => chat.Created)
                            .FirstOrDefaultAsync();

        }
    }
}
