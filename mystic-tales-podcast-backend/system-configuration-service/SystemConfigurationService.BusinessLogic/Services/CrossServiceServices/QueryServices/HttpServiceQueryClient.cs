
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SystemConfigurationService.BusinessLogic.Models.CrossService;
using System.Security.Cryptography;
using System.Text;
using SystemConfigurationService.Common.AppConfigurations.SystemService.interfaces;
using Newtonsoft.Json;
using SystemConfigurationService.Common.AppConfigurations.App.interfaces;
using Newtonsoft.Json.Linq;

namespace SystemConfigurationService.BusinessLogic.Services.CrossServiceServices.QueryServices
{
    public class HttpServiceQueryClient
    {
        // CONFIG
        private readonly IAppConfig _appConfig;
        private readonly ISystemServiceConfig _systemServiceConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HttpServiceQueryClient> _logger;

        public HttpServiceQueryClient(
            IAppConfig appConfig,
            ISystemServiceConfig systemServiceConfig,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ILogger<HttpServiceQueryClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _systemServiceConfig = systemServiceConfig;
            _cache = cache;
            _logger = logger;
            _appConfig = appConfig;
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
                var response = await client.PostAsync($"{_appConfig.API_GATEWAY_URL}/{serviceInfo.Url}/api/query/batch", content, cancellationToken);

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
                    Errors = new List<string> { $"Service {serviceName} error: {ex.Message}, call url: {_appConfig.API_GATEWAY_URL}/{serviceInfo.Url}/api/query/batch" }
                };
            }
        }

        public async Task<T?> QuerySingleAsync<T>(
            string serviceName,
            string entityType,
            string queryType,
            JObject parameters,
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

            if (!result.IsSuccess)
                return default;

            // Handle JToken Results instead of Dictionary
            if (result.Results is JObject resultsObj && resultsObj.TryGetValue("single", out var data))
            {
                return JsonConvert.DeserializeObject<T>(data.ToString() ?? string.Empty);
            }

            return default;
        }

        public async Task<IEnumerable<T>> QueryMultipleAsync<T>(
            string serviceName,
            string entityType,
            string queryType,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default)
        {
            // Convert Dictionary to JObject for compatibility
            var jObjectParameters = JObject.FromObject(parameters);

            var request = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
        {
            new()
            {
                Key = "multiple",
                EntityType = entityType,
                QueryType = queryType,
                Parameters = jObjectParameters
            }
        }
            };

            var result = await ExecuteBatchAsync(serviceName, request, cancellationToken);

            if (!result.IsSuccess)
                return Enumerable.Empty<T>();

            // Handle JToken Results instead of Dictionary
            if (result.Results is JObject resultsObj && resultsObj.TryGetValue("multiple", out var data))
            {
                return JsonConvert.DeserializeObject<IEnumerable<T>>(data.ToString() ?? "[]") ?? Enumerable.Empty<T>();
            }

            return Enumerable.Empty<T>();
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
