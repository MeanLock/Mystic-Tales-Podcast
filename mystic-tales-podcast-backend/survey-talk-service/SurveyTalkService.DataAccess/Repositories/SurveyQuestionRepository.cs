using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SurveyTalkService.DataAccess.Entities;
using SurveyTalkService.BusinessLogic.DTOs.Survey.Filters;

namespace SurveyTalkService.DataAccess.Repositories
{
    public class SurveyQuestionRepository : ISurveyQuestionRepository
    {
        private readonly AppDbContext _appDbContext;

        public SurveyQuestionRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<SurveyQuestion>> FindBySurveyIdAndIsDeletedContainAsync(int surveyId, bool isDeletedContain)
        {
            return await _appDbContext.SurveyQuestions
                .Where(sq => sq.SurveyId == surveyId && (isDeletedContain == true || sq.DeletedAt == null))
                .Include(sq => sq.QuestionType)
                .Include(sq => sq.SurveyOptions)
                .ToListAsync();
        }

        public async Task DeleteByIdAsync(Guid id, DateTime? deletedAt = null)
        {
            var surveyQuestion = await _appDbContext.SurveyQuestions.FindAsync(id);
            if (surveyQuestion == null)
            {
                throw new Exception($"không tìm thấy câu hỏi với id: {id}");
            }
            // nếu deletedAt là null, thì xóa vĩnh viễn
            if (deletedAt == null)
            {
                _appDbContext.SurveyQuestions.Remove(surveyQuestion);
                // xoá các tùy chọn liên quan
                var surveyOptions = await _appDbContext.SurveyOptions
                    .Where(so => so.SurveyQuestionId == id)
                    .ToListAsync();
                _appDbContext.SurveyOptions.RemoveRange(surveyOptions);
            }
            else
            {
                // nếu deletedAt không phải null, thì đánh dấu đã xóa
                surveyQuestion.DeletedAt = deletedAt;
                _appDbContext.SurveyQuestions.Update(surveyQuestion);
            }
            await _appDbContext.SaveChangesAsync();
        }

    }
}
