using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using ModerationService.Infrastructure.Configurations.OpenAI.interfaces;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ModerationService.Infrastructure.Services.OpenAI._4oMini
{
    public class OpenAI4oMiniService
    {
        // LOGGER
        private readonly ILogger<OpenAI4oMiniService> _logger;

        // CONFIG
        private readonly IOpenAIConfig _openAIConfig;

        // SERVICES



        public OpenAI4oMiniService(
            IOpenAIConfig openAIConfig,
            ILogger<OpenAI4oMiniService> logger
            )
        {
            _openAIConfig = openAIConfig;

            _logger = logger;
        }

        /// <summary>
        /// Gọi OpenAI function_call, trả về arguments (string) từ response.
        /// </summary>
        public async Task<JToken> CallOpenAIChatCompletionFunctionAsync(JObject body)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAIConfig.ApiKey);
                httpClient.BaseAddress = new Uri(_openAIConfig.BaseUrl);
                var content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("/v1/chat/completions", content);
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJson = JObject.Parse(responseString);
                var functionCall = responseJson["choices"]?[0]?["message"]?["function_call"];
                return functionCall?["arguments"];
            }
        }

    }
}
