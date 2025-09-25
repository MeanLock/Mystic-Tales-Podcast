// using System.Linq.Expressions;
// using SurveyTalkService.DataAccess.Data;
// using SurveyTalkService.DataAccess.Entities;

// namespace SurveyTalkService.DataAccess.Repositories.interfaces
// {
//     public interface ICrossServiceRepository<T> : IGenericRepository<T> where T : class
//     {
//         // Existing methods từ Generic Repository
//         IQueryable<T> FindAll(Expression<Func<T, bool>>? predicate = null,
//             params Expression<Func<T, object>>[] includeProperties);

//         Task<T?> FindByIdAsync(object id, params Expression<Func<T, object>>[] includeProperties);

//         // ⭐ New methods cho Cross-Service queries
//         Task<TResult> FindWithExternalDataAsync<TResult>(
//             object id,
//             CrossServiceQueryOptions<T, TResult> options);

//         Task<IEnumerable<TResult>> FindAllWithExternalDataAsync<TResult>(
//             Expression<Func<T, bool>>? predicate,
//             CrossServiceQueryOptions<T, TResult> options);

//         Task<PagedResult<TResult>> FindPagedWithExternalDataAsync<TResult>(
//             Expression<Func<T, bool>>? predicate,
//             CrossServiceQueryOptions<T, TResult> options,
//             int page, int pageSize);
//     }

//     public class CrossServiceQueryOptions<T, TResult> where T : class
//     {
//         // Local entity selection
//         public Expression<Func<T, object>>[] IncludeProperties { get; set; } = Array.Empty<Expression<Func<T, object>>>();
//         public string[] SelectFields { get; set; } = Array.Empty<string>();

//         // External service queries
//         public List<ExternalQueryDefinition> ExternalQueries { get; set; } = new();

//         // Result mapping
//         public Expression<Func<T, Dictionary<string, object>, TResult>> ResultMapper { get; set; }

//         // Caching options
//         public TimeSpan? CacheDuration { get; set; }
//         public bool UseParallelExecution { get; set; } = true;
//     }

//     public class ExternalQueryDefinition
//     {
//         public string Key { get; set; } // Key trong result dictionary
//         public string ServiceName { get; set; } // "account-service", "subscription-service"
//         public string EntityType { get; set; } // "Account", "PodcastSubscription"
//         public string QueryType { get; set; } // "GetById", "GetByIds", "Search"
//         public Dictionary<string, object> Parameters { get; set; } = new();
//         public string[] Fields { get; set; } = Array.Empty<string>();

//         // Dynamic parameter mapping từ main entity
//         public Expression<Func<object, Dictionary<string, object>>> ParameterMapper { get; set; }
//     }
// }
