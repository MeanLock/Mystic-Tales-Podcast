using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SurveyTalkService.API.Controllers.UserControllers;
using SurveyTalkService.API.Filters.ExceptionFilters;
using SurveyTalkService.BusinessLogic.DTOs.Auth;
using SurveyTalkService.BusinessLogic.DTOs.Feedback;
using SurveyTalkService.BusinessLogic.Services.DbServices.MiscServices;
using SurveyTalkService.BusinessLogic.Services.DbServices.UserServices;
using SurveyTalkService.DataAccess.Entities;
using SurveyTalkService.BusinessLogic.Helpers.FileHelpers;
using SurveyTalkService.DataAccess.Repositories.interfaces;
using SurveyTalkService.BusinessLogic.Models.CrossService;
using SurveyTalkService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace SurveyTalkService.API.Controllers.QueryControllers
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
            var results = new Dictionary<string, object>();
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
                    results[query.Key] = new { error = ex.Message };
                    errors.Add($"Query {query.Key}: {ex.Message}");
                }
            }

            return Ok(new BatchQueryResult { Results = results, Errors = errors });
        }


        /// <summary>
        /// Gọi lại chính API /api/query/batch thông qua HttpServiceQueryClient (self-call)
        /// </summary>
        [HttpPost("self-batch")]
        public async Task<IActionResult> SelfBatch()
        {
            // "SurveyTalkService" là tên service bạn đã cấu hình trong appsettings cho chính API này
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
        {
            new BatchQueryItem
            {
                Key = "accounts",
                EntityType = "Account",
                QueryType = "FindAll",
                Parameters = new Dictionary<string, object>
                {
                    { "RoleId", 4 },
                        { "include", "Role" }
                    },
                    Fields = new string[] { "Id", "Email", "RoleId", "Role" }
                    }
                }
            };
            var serviceName = "ThisService";
            var result = await _httpServiceQueryClient.ExecuteBatchAsync(serviceName, batchRequest);
            return Ok(result);
        }
    }
}
