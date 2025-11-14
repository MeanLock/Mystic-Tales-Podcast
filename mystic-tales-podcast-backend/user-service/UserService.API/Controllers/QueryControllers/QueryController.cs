using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UserService.API.Filters.ExceptionFilters;
using UserService.BusinessLogic.DTOs.Channel;
using UserService.BusinessLogic.DTOs.Episode;
using UserService.BusinessLogic.DTOs.Show;
using UserService.BusinessLogic.DTOs.SystemConfiguration;
using UserService.BusinessLogic.Models.CrossService;
using UserService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace UserService.API.Controllers.QueryControllers
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

        [HttpGet("test-query")]
        public async Task<IActionResult> TestQuery()
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "activeSystemConfigProfile",
                            QueryType = "findall",
                            EntityType = "SystemConfigProfile",
                                Parameters = JObject.FromObject(new
                                {
                                    where = new
                                    {
                                        IsActive = true
                                    },
                                    include = "AccountConfig,AccountViolationLevelConfigs, BookingConfig, PodcastSubscriptionConfigs, PodcastSuggestionConfig, ReviewSessionConfig",

                                }),
                            Fields = new[] { "Id", "Name", "IsActive", "AccountConfig", "AccountViolationLevelConfigs", "BookingConfig", "PodcastSubscriptionConfigs", "PodcastSuggestionConfig", "ReviewSessionConfig" }
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("SystemConfigurationService", batchRequest);

            // kiểm tra null
            if (result.Results["activeSystemConfigProfile"] == null)
            {
                return NotFound(new { message = "Active SystemConfigProfile not found" });
            }
            else
            {
                Console.WriteLine(result.Results["activeSystemConfigProfile"].ToString());
            }
            var config = (result.Results["activeSystemConfigProfile"].First as JObject).ToObject<SystemConfigProfileDTO>();
            return Ok(new
            {
                result = config
            });
        }
    }
}
