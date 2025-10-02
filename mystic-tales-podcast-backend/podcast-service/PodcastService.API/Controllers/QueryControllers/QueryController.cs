using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PodcastService.API.Filters.ExceptionFilters;
using PodcastService.BusinessLogic.Models.CrossService;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace PodcastService.API.Controllers.QueryControllers
{
    [Route("api/query")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class QueryController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        public QueryController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
        }

        [HttpPost("batch")]
        public async Task<IActionResult> ExecuteBatch([FromBody] BatchQueryRequest request)
        {
            var results = new JObject();
            var errors = new List<string>();

            foreach (var query in request.Queries)
            {
                try
                {
                    var result = await _genericQueryService.HandleQueryAsync(query);
                    results[query.Key] = result;
                }
                catch (Exception ex)
                {
                    results[query.Key] = JObject.FromObject(new { error = ex.Message });
                    errors.Add($"Query {query.Key}: {ex.Message}");
                }
            }

            return Ok(new BatchQueryResult { Results = results, Errors = errors });
        }


        /// <summary>
        /// Gọi lại chính API /api/query/batch thông qua HttpServiceQueryClient (self-call)
        /// </summary>
        [HttpPost("self-batch")]
        public async Task<IActionResult> SelfBatch([FromBody] string serviceName)
        {
            // "TransactionService" là tên service bạn đã cấu hình trong appsettings cho chính API này
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "accounts",
                            EntityType = "Account",
                            QueryType = "FindAll",
                            Parameters = new JObject
                            {
                                { "RoleId", 4 },
                                { "include", "Role" }
                            },
                            Fields = new string[] { "Id", "Email", "RoleId", "Role" }
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync(serviceName, batchRequest);
            return Ok(new
            {
                accounts = result.Results["accounts"][0]["Role"],
            });
        }
    }
}
