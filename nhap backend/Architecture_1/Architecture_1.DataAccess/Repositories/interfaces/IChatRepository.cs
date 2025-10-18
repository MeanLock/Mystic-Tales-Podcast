using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;

namespace Architecture_1.DataAccess.Repositories.interfaces
{
    public interface IChatRepository
    {
        Task<Chat> FindByIdAsync(int id);
        Task<IEnumerable<Chat>> FindByAccountId(int accountId);
        Task<IEnumerable<Account>> GetChatRoomsByAccountId(int accountId);

        Task<IEnumerable<Chat>> FindBy1v1(int accountId1, int accountId2);
        Task<Chat> GetLastChatBy1v1(int accountId1, int accountId2);
    }
}
