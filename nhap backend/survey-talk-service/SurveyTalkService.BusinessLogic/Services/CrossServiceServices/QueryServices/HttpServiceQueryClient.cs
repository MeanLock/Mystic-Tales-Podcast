using SurveyTalkService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SurveyTalkService.DataAccess.Data;
using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SurveyTalkService.BusinessLogic.Models.CrossService;
using System.Security.Cryptography;
using System.Text;
using SurveyTalkService.Common.AppConfigurations.SystemService.interfaces;
using Newtonsoft.Json;

namespace SurveyTalkService.BusinessLogic.Services.CrossServiceServices.QueryServices
{
    public class HttpServiceQueryClient
    {
        // CONFIG
        private readonly ISystemServiceConfig _systemServiceConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HttpServiceQueryClient> _logger;

        public HttpServiceQueryClient(
            ISystemServiceConfig systemServiceConfig,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ILogger<HttpServiceQueryClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _systemServiceConfig = systemServiceConfig;
            _cache = cache;
            _logger = logger;
        }

        public async Task<BatchQueryResult> ExecuteBatchAsync(
            string serviceName,
            BatchQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var serviceInfo = _systemServiceConfig.GetServiceInfo(serviceName);
            var cacheKey = GenerateCacheKey(serviceName, request);

            // Check cache

            var client = _httpClientFactory.CreateClient();

            // Forward authorization header
            ForwardAuthorizationHeader(client);

            var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            });

            var content = new StringContent(json, Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));

            try
            {
                var response = await client.PostAsync($"{serviceInfo.Url}/api/query/batch", content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Service {ServiceName} returned {StatusCode}", serviceName, response.StatusCode);
                    return new BatchQueryResult
                    {
                        Errors = new List<string> { $"Service {serviceName} returned {response.StatusCode}" }
                    };
                }

                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonConvert.DeserializeObject<BatchQueryResult>(responseJson, new JsonSerializerSettings
                {
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                }) ?? new BatchQueryResult();


                return result;
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Request to service {ServiceName} timed out", serviceName);
                return new BatchQueryResult
                {
                    Errors = new List<string> { $"Service {serviceName} request timed out" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling service {ServiceName}", serviceName);
                return new BatchQueryResult
                {
                    Errors = new List<string> { $"Service {serviceName} error: {ex.Message}" }
                };
            }
        }

        public async Task<T?> QuerySingleAsync<T>(
            string serviceName,
            string entityType,
            string queryType,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default)
        {
            var request = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
            {
                new()
                {
                    Key = "single",
                    EntityType = entityType,
                    QueryType = queryType,
                    Parameters = parameters
                }
            }
            };

            var result = await ExecuteBatchAsync(serviceName, request, cancellationToken);

            if (!result.IsSuccess || !result.Results.TryGetValue("single", out var data))
                return default;

            return JsonConvert.DeserializeObject<T>(data.ToString() ?? string.Empty);
        }

        public async Task<IEnumerable<T>> QueryMultipleAsync<T>(
            string serviceName,
            string entityType,
            string queryType,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default)
        {
            var request = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
            {
                new()
                {
                    Key = "multiple",
                    EntityType = entityType,
                    QueryType = queryType,
                    Parameters = parameters
                }
            }
            };

            var result = await ExecuteBatchAsync(serviceName, request, cancellationToken);

            if (!result.IsSuccess || !result.Results.TryGetValue("multiple", out var data))
                return Enumerable.Empty<T>();

            return JsonConvert.DeserializeObject<IEnumerable<T>>(data.ToString() ?? "[]") ?? Enumerable.Empty<T>();
        }



        private void ForwardAuthorizationHeader(HttpClient client)
        {
            // This would need access to current HTTP context
            // Implementation depends on your authentication setup
        }

        private string GenerateCacheKey(string serviceName, BatchQueryRequest request)
        {
            var content = JsonConvert.SerializeObject(request);
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
            return $"batch_{serviceName}_{Convert.ToBase64String(hash)}";
        }
    }
}
