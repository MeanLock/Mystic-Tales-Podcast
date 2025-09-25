// using SurveyTalkService.DataAccess.Repositories.interfaces;
// using Microsoft.EntityFrameworkCore;
// using SurveyTalkService.DataAccess.Data;
// using System.Linq.Expressions;

// namespace SurveyTalkService.DataAccess.Repositories
// {
//     public class CrossServiceRepository<T> : GenericRepository<T>, ICrossServiceRepository<T>
//     where T : class
//     {
//         private readonly IServiceQueryClient _serviceQueryClient;
//         private readonly IMemoryCache _cache;
//         private readonly ILogger<CrossServiceRepository<T>> _logger;

//         public CrossServiceRepository(
//             AppDbContext context,
//             IServiceQueryClient serviceQueryClient,
//             IMemoryCache cache,
//             ILogger<CrossServiceRepository<T>> logger) : base(context)
//         {
//             _serviceQueryClient = serviceQueryClient;
//             _cache = cache;
//             _logger = logger;
//         }

//         public async Task<TResult> FindWithExternalDataAsync<TResult>(
//             object id,
//             CrossServiceQueryOptions<T, TResult> options)
//         {
//             // 1. Get main entity với existing logic
//             var entity = await FindByIdAsync(id, options.IncludeProperties);
//             if (entity == null) return default(TResult);

//             // 2. Execute external queries parallel
//             var externalData = await ExecuteExternalQueriesAsync(new[] { entity }, options.ExternalQueries);

//             // 3. Map result
//             var entityExternalData = externalData.FirstOrDefault()?.Value ?? new Dictionary<string, object>();
//             return options.ResultMapper.Compile()(entity, entityExternalData);
//         }

//         public async Task<IEnumerable<TResult>> FindAllWithExternalDataAsync<TResult>(
//             Expression<Func<T, bool>>? predicate,
//             CrossServiceQueryOptions<T, TResult> options)
//         {
//             // 1. Get main entities
//             var query = FindAll(predicate, options.IncludeProperties);
//             var entities = await query.ToListAsync();

//             if (!entities.Any()) return Enumerable.Empty<TResult>();

//             // 2. Execute external queries for all entities
//             var externalDataMap = await ExecuteExternalQueriesAsync(entities, options.ExternalQueries);

//             // 3. Map results
//             var results = new List<TResult>();
//             foreach (var entity in entities)
//             {
//                 var entityKey = GetEntityKey(entity);
//                 var entityExternalData = externalDataMap.GetValueOrDefault(entityKey, new Dictionary<string, object>());
//                 var result = options.ResultMapper.Compile()(entity, entityExternalData);
//                 results.Add(result);
//             }

//             return results;
//         }

//         private async Task<Dictionary<string, Dictionary<string, object>>> ExecuteExternalQueriesAsync(
//             IEnumerable<T> entities,
//             List<ExternalQueryDefinition> externalQueries)
//         {
//             if (!externalQueries.Any()) return new Dictionary<string, Dictionary<string, object>>();

//             var entityKeys = entities.Select(GetEntityKey).ToList();
//             var resultMap = entityKeys.ToDictionary(k => k, k => new Dictionary<string, object>());

//             // Group queries by service để optimize HTTP calls
//             var serviceGroups = externalQueries.GroupBy(q => q.ServiceName);

//             var tasks = serviceGroups.Select(async serviceGroup =>
//             {
//                 var serviceName = serviceGroup.Key;
//                 var queries = serviceGroup.ToList();

//                 // Build batch request cho service
//                 var batchRequest = BuildBatchRequestForService(entities, queries);

//                 try
//                 {
//                     var batchResult = await _serviceQueryClient.ExecuteBatchAsync(serviceName, batchRequest);

//                     // Map results back to entities
//                     MapBatchResultToEntities(batchResult, resultMap, queries);
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError(ex, "External query failed for service {ServiceName}", serviceName);
//                     // Continue với partial data thay vì fail completely
//                 }
//             });

//             await Task.WhenAll(tasks);
//             return resultMap;
//         }

//         private BatchQueryRequest BuildBatchRequestForService(IEnumerable<T> entities, List<ExternalQueryDefinition> queries)
//         {
//             var batchQueries = new List<BatchQueryItem>();

//             foreach (var entity in entities)
//             {
//                 var entityKey = GetEntityKey(entity);

//                 foreach (var queryDef in queries)
//                 {
//                     // Map parameters từ entity sang external query
//                     var parameters = queryDef.ParameterMapper?.Compile()(entity) ?? queryDef.Parameters;

//                     batchQueries.Add(new BatchQueryItem
//                     {
//                         Key = $"{entityKey}_{queryDef.Key}",
//                         EntityType = queryDef.EntityType,
//                         QueryType = queryDef.QueryType,
//                         Parameters = parameters,
//                         Fields = queryDef.Fields
//                     });
//                 }
//             }

//             return new BatchQueryRequest { Queries = batchQueries };
//         }

//         private string GetEntityKey(T entity)
//         {
//             // Extract entity ID cho mapping
//             var idProperty = typeof(T).GetProperty("Id");
//             if (idProperty != null)
//             {
//                 return idProperty.GetValue(entity)?.ToString() ?? "";
//             }
//             return entity.GetHashCode().ToString();
//         }
//     }
// }
