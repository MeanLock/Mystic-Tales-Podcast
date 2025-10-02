using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.DataAccess.Repositories
{
    public class SurveyTopicFavoriteRepository : ISurveyTopicFavoriteRepository
    {
        private readonly AppDbContext _appDbContext;

        public SurveyTopicFavoriteRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        
        public async Task<IEnumerable<SurveyTopicFavorite>> FindByAccountIdAsync(int accountId)
        {
            // Lấy tất cả survey topic favorite theo account id
            return await _appDbContext.SurveyTopicFavorites
                .Include(favorite => favorite.SurveyTopic)
                .Where(favorite => favorite.AccountId == accountId)
                .ToListAsync();
        }

        public async Task<bool> DeleteByAccountIdAsync(int accountId)
        {
            // xoá tất cả survey topic favorite theo account id

            var surveyTopicFavorites = await _appDbContext.SurveyTopicFavorites
                .Where(favorite => favorite.AccountId == accountId)
                .ToListAsync();

            if (surveyTopicFavorites == null || !surveyTopicFavorites.Any())
            {
                return false; // Không có survey topic favorite nào để xoá
            }
            else
            {
                _appDbContext.SurveyTopicFavorites.RemoveRange(surveyTopicFavorites);
            }
            await _appDbContext.SaveChangesAsync();
            return true;
        }
    }
}
