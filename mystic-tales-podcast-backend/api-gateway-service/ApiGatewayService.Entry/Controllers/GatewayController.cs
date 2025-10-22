using Microsoft.AspNetCore.Mvc;
using ApiGatewayService.Infrastructure.Services.Yarp;

namespace ApiGatewayService.Entry.Controllers
{
    /// <summary>
    /// Gateway Controller - Proxy tất cả requests đến services
    /// </summary>
    [ApiController]
    [Route("api-manual-proxy/{serviceName}")]
    public class GatewayController : ControllerBase
    {
        private readonly YarpServiceDiscoveryService _serviceDiscovery;
        private readonly HttpClient _httpClient;
        private readonly ILogger<GatewayController> _logger;

        public GatewayController(
            YarpServiceDiscoveryService serviceDiscovery,
            IHttpClientFactory httpClientFactory,
            ILogger<GatewayController> logger)
        {
            _serviceDiscovery = serviceDiscovery;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        /// <summary>
        /// Proxy tất cả requests đến target service
        /// </summary>
        [HttpGet("{**path}")]
        [HttpPost("{**path}")]
        [HttpPut("{**path}")]
        [HttpDelete("{**path}")]
        [HttpPatch("{**path}")]
        public async Task<IActionResult> ProxyRequest(string serviceName, string path = "")
        {
            try
            {
                _logger.LogInformation("Gateway: Proxying {Method} request to service '{ServiceName}', path: '{Path}'", 
                    Request.Method, serviceName, path);

                // Lấy service endpoints
                var serviceEndpoints = await _serviceDiscovery.GetServiceEndpointsAsync();
                
                if (!serviceEndpoints.ContainsKey(serviceName))
                {
                    _logger.LogWarning("Service '{ServiceName}' not found in discovered services", serviceName);
                    return NotFound($"Service '{serviceName}' not found");
                }

                var endpoints = serviceEndpoints[serviceName];
                if (!endpoints.Any())
                {
                    _logger.LogWarning("No healthy endpoints found for service '{ServiceName}'", serviceName);
                    return Problem("Service temporarily unavailable", statusCode: 503);
                }

                // Chọn endpoint đầu tiên
                var targetEndpoint = endpoints.First();
                
                // Build target URL
                var targetUrl = $"{targetEndpoint.TrimEnd('/')}/{path.TrimStart('/')}";
                
                if (!string.IsNullOrEmpty(Request.QueryString.Value))
                {
                    targetUrl += Request.QueryString.Value;
                }

                _logger.LogInformation("Proxying to: {TargetUrl}", targetUrl);

                // Tạo request
                var request = new HttpRequestMessage(new HttpMethod(Request.Method), targetUrl);

                // Copy headers (trừ Host)
                foreach (var header in Request.Headers)
                {
                    if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                        continue;
                        
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }

                // Copy body nếu có (POST/PUT/PATCH)
                if (Request.ContentLength > 0 && 
                    (Request.Method == "POST" || Request.Method == "PUT" || Request.Method == "PATCH"))
                {
                    var bodyStream = new MemoryStream();
                    await Request.Body.CopyToAsync(bodyStream);
                    bodyStream.Position = 0;
                    request.Content = new StreamContent(bodyStream);
                    
                    if (!string.IsNullOrEmpty(Request.ContentType))
                    {
                        request.Content.Headers.TryAddWithoutValidation("Content-Type", Request.ContentType);
                    }
                }

                // Gửi request
                var response = await _httpClient.SendAsync(request);

                // Return response content
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Successfully proxied to {TargetUrl}, status: {StatusCode}", 
                    targetUrl, response.StatusCode);

                return new ContentResult
                {
                    Content = content,
                    ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json",
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying request to service '{ServiceName}', path: '{Path}'", serviceName, path);
                return Problem("Internal gateway error", statusCode: 502);
            }
        }
    }
}
