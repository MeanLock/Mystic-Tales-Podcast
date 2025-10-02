using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.Entities;

namespace PodcastService.DataAccess.Repositories.interfaces
{
    public interface IAccountProfileRepository
    {
        Task<AccountProfile> FindByAccountIdAsync(int accountId);
        Task UpdateAsync(AccountProfile accountProfile);
    }
}
