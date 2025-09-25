using SurveyTalkService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SurveyTalkService.DataAccess.Data;
using System.Linq.Expressions;

namespace SurveyTalkService.BusinessLogic.Models.CrossService
{
    public class BatchQueryResult
    {
        public Dictionary<string, object> Results { get; set; } = new();
        public long ExecutionTimeMs { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool IsSuccess => !Errors.Any();
    }
}
