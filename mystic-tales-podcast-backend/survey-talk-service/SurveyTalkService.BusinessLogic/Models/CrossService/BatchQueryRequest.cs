using SurveyTalkService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SurveyTalkService.DataAccess.Data;
using System.Linq.Expressions;

namespace SurveyTalkService.BusinessLogic.Models.CrossService
{
    public class BatchQueryRequest
    {
        public List<BatchQueryItem> Queries { get; set; } = new();
        public BatchQueryOptions? Options { get; set; }
    }

    public class BatchQueryItem
    {
        public string Key { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string QueryType { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public string[] Fields { get; set; } = Array.Empty<string>();
    }

    public class BatchQueryOptions
    {
        public bool UseParallelExecution { get; set; } = true;
        public int TimeoutMs { get; set; } = 30000;
        public bool ContinueOnError { get; set; } = true;
    }
}
