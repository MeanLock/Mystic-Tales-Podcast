using SubscriptionService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SubscriptionService.DataAccess.Data;
using System.Linq.Expressions;

namespace SubscriptionService.BusinessLogic.Models.CrossService
{
    public class CrossServiceQueryOptions<T, TResult> where T : class
    {
        public Expression<Func<T, object>>[] IncludeProperties { get; set; } = Array.Empty<Expression<Func<T, object>>>();
        public string[] SelectFields { get; set; } = Array.Empty<string>();
        public List<ExternalQueryDefinition> ExternalQueries { get; set; } = new();
        public Expression<Func<T, Dictionary<string, object>, TResult>> ResultMapper { get; set; } = null!;
        public TimeSpan? CacheDuration { get; set; }
        public bool UseParallelExecution { get; set; } = true;
    }

    public class ExternalQueryDefinition
    {
        public string Key { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string QueryType { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public string[] Fields { get; set; } = Array.Empty<string>();
        public Expression<Func<object, Dictionary<string, object>>>? ParameterMapper { get; set; }
    }
}
