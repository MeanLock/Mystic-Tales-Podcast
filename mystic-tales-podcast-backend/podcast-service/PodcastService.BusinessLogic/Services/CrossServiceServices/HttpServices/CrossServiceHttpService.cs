
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PodcastService.BusinessLogic.Models.CrossService;
using System.Security.Cryptography;
using System.Text;
using PodcastService.Common.AppConfigurations.SystemService.interfaces;
using Newtonsoft.Json;
using PodcastService.Common.AppConfigurations.App.interfaces;
using Newtonsoft.Json.Linq;

namespace PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices
{
    public class CrossServiceHttpService
    {
        // CONFIG
        private readonly IAppConfig _appConfig;
        private readonly ISystemServiceConfig _systemServiceConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CrossServiceHttpService> _logger;

        public CrossServiceHttpService(
            IAppConfig appConfig,
            ISystemServiceConfig systemServiceConfig,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ILogger<CrossServiceHttpService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _systemServiceConfig = systemServiceConfig;
            _cache = cache;
            _logger = logger;
            _appConfig = appConfig;
        }

        // ...existing code...
        public async Task<T?> PostManualAsync<T>(string serviceName, object body, string relativePath = "api/manual", CancellationToken cancellationToken = default) where T : class
        {
            var serviceInfo = _systemServiceConfig.GetServiceInfo(serviceName);
            var client = _httpClientFactory.CreateClient();

            // Forward authorization header if any
            ForwardAuthorizationHeader(client);

            var json = JsonConvert.SerializeObject(body, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            });
            Console.WriteLine($"[PostManual] Request Body: {json}");


            var content = new StringContent(json, Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));

            try
            {
                var url = $"{_appConfig.API_GATEWAY_URL}/{serviceInfo.Url}/{relativePath}".TrimEnd('/');
                Console.WriteLine($"[PostManual] POST {url}");
                var response = await client.PostAsync(url, content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Service {ServiceName} returned {StatusCode} for POST {Url}", serviceName, response.StatusCode, url);
                    return default;
                }

                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                 Console.WriteLine($"[PostManual] Response JSON: {responseJson}");
                var result = JsonConvert.DeserializeObject<T>(responseJson, new JsonSerializerSettings
                {
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                });

                return result;
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Request to service {ServiceName} timed out for POST {Path}", serviceName, relativePath);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling service {ServiceName} POST {Path}", serviceName, relativePath);
                return default;
            }
        }

        private void ForwardAuthorizationHeader(HttpClient client)
        {
            // This would need access to current HTTP context
            // Implementation depends on your authentication setup
        }
    }
}
