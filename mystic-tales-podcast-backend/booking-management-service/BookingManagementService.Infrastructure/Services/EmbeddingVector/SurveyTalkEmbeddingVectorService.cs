using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using BookingManagementService.Common.AppConfigurations.App.interfaces;

namespace BookingManagementService.Infrastructure.Services.EmbeddingVector
{
    public class SurveyTalkEmbeddingVectorService
    {
        // CONFIG
        public readonly IAppConfig _appConfig;

        // HTTP CLIENT
        private readonly IHttpClientFactory _httpClientFactory;

        public SurveyTalkEmbeddingVectorService(
            IAppConfig appConfig,
            IHttpClientFactory httpClientFactory
            )
        {
            _appConfig = appConfig;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Accepts JSON string, returns JSON string. No DTO dependency.
        /// </summary>
        public async Task<string> GetVectorEncodedFilterTagsAsync(string summarizedFilterTagsJson)
        {
            try
            {
                var content = new StringContent(summarizedFilterTagsJson, Encoding.UTF8, "application/json");
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsync(
                    _appConfig.EMBEDDING_VECTOR_API_URL + "/encode/filter-tags",
                     content
                );
                response.EnsureSuccessStatusCode();
                var apiResultJson = await response.Content.ReadAsStringAsync();
                return apiResultJson;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while getting vector encoded filter tags: {ex.Message}");
                throw new Exception("Error while getting vector encoded filter tags", ex);
            }
        }

        /// <summary>
        /// Accepts JSON string, returns JSON string. No DTO dependency.
        /// </summary>
        public async Task<string> GetSimilarCandidateIdsByEmbeddingVectorFilterTagsWithTargetAccuracyAsync(string filterTagSimilarityComparisonRequestJson)
        {
            try
            {
                var content = new StringContent(filterTagSimilarityComparisonRequestJson, Encoding.UTF8, "application/json");
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsync(
                    _appConfig.EMBEDDING_VECTOR_API_URL + "/compare/filter-tag-similarity-by-target-accuracy",
                     content
                );
                response.EnsureSuccessStatusCode();
                var apiResultJson = await response.Content.ReadAsStringAsync();
                return apiResultJson;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while getting vector encoded filter tags: {ex.Message}");
                throw new Exception("Error while getting vector encoded filter tags", ex);
            }
        }


        /// <summary>
        /// Accepts JSON string, returns JSON string. No DTO dependency.
        /// </summary>
        public async Task<string> GetSimilarCandidateIdsByEmbeddingVectorFilterTagsWithCandidateAccuracyAsync(string filterTagSimilarityComparisonRequestJson)
        {
            try
            {
                var content = new StringContent(filterTagSimilarityComparisonRequestJson, Encoding.UTF8, "application/json");
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsync(
                    _appConfig.EMBEDDING_VECTOR_API_URL + "/compare/filter-tag-similarity-by-candidate-accuracy",
                     content
                );
                response.EnsureSuccessStatusCode();
                var apiResultJson = await response.Content.ReadAsStringAsync();
                return apiResultJson;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while getting vector encoded filter tags: {ex.Message}");
                throw new Exception("Error while getting vector encoded filter tags", ex);
            }
        }


        
    }
}
