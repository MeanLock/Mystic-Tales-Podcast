using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using BookingManagementService.BusinessLogic.DTOs.FilterTag;
using BookingManagementService.Common.AppConfigurations.App.interfaces;
using BookingManagementService.Infrastructure.Services.EmbeddingVector;

namespace BookingManagementService.BusinessLogic.Services.EmbeddingVectorServices
{
    public class SurveyEmbeddingVectorService
    {
        // CONFIG
        public readonly IAppConfig _appConfig;

        // HTTP CLIENT
        private readonly IHttpClientFactory _httpClientFactory;

        // Services
        private readonly SurveyTalkEmbeddingVectorService _surveyTalkEmbeddingVectorService;

        public SurveyEmbeddingVectorService(
            IAppConfig appConfig,
            IHttpClientFactory httpClientFactory,

            SurveyTalkEmbeddingVectorService surveyTalkEmbeddingVectorService
            )
        {
            _appConfig = appConfig;
            _httpClientFactory = httpClientFactory;

            _surveyTalkEmbeddingVectorService = surveyTalkEmbeddingVectorService;
        }

        public async Task<List<EmbeddingVectorFilterTagDTO>> GetVectorEncodedFilterTagsAsync(List<SummarizedFilterTagDTO> summarizedFilterTagsDTO)
        {
            try
            {
                var json = JsonConvert.SerializeObject(summarizedFilterTagsDTO, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver() // PascalCase
                });
                var apiResultJson = await _surveyTalkEmbeddingVectorService.GetVectorEncodedFilterTagsAsync(json);
                var apiResult = JArray.Parse(apiResultJson).ToObject<List<EmbeddingVectorFilterTagDTO>>();
                return apiResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while getting vector encoded filter tags: {ex.Message}");
                throw new Exception("Error while getting vector encoded filter tags", ex);
            }
        }


        // So sánh vector cosine và chuyển đổi sang accuracy như Python
        private static float CalcCosineSimilarity(float[]? v1, float[]? v2, float maxScore)
        {
            if (v1 == null || v2 == null)
                return maxScore;
            double dot = 0, norm1 = 0, norm2 = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                dot += v1[i] * v2[i];
                norm1 += v1[i] * v1[i];
                norm2 += v2[i] * v2[i];
            }
            if (norm1 == 0 || norm2 == 0) return maxScore;
            return (float)(dot / (Math.Sqrt(norm1) * Math.Sqrt(norm2)));
        }

        private static float ConvertSimilarityToAccuracy(float similarity, float minScore, float maxScore)
        {
            if (similarity <= minScore)
                return 0f;
            else if (similarity >= maxScore)
                return 1f;
            else
                return (float)Math.Round((similarity - minScore) / (maxScore - minScore), 2);
        }

        //////////////////////////////////////////////////////////////////
        public async Task<List<FilterTagSimilarityComparisonResultDTO>> GetSimilarCandidateIdsByEmbeddingVectorFilterTagsWithTargetAccuracyAsync(FilterTagSimilarityComparisonRequestDTO filterTagSimilarityComparisonRequestDTO)
        {
            try
            {
                var json = JsonConvert.SerializeObject(filterTagSimilarityComparisonRequestDTO, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver() // PascalCase
                });
                var apiResultJson = await _surveyTalkEmbeddingVectorService.GetSimilarCandidateIdsByEmbeddingVectorFilterTagsWithTargetAccuracyAsync(json);
                var apiResult = JArray.Parse(apiResultJson).ToObject<List<FilterTagSimilarityComparisonResultDTO>>();
                return apiResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while getting vector encoded filter tags: {ex.Message}");
                throw new Exception("Error while getting vector encoded filter tags", ex);
            }
        }


        public async Task<List<FilterTagSimilarityComparisonResultDTO>> GetSimilarCandidateIdsByEmbeddingVectorFilterTagsWithCandidateAccuracyAsync(FilterTagSimilarityComparisonRequestDTO filterTagSimilarityComparisonRequestDTO)
        {
            try
            {
                var json = JsonConvert.SerializeObject(filterTagSimilarityComparisonRequestDTO, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver() // PascalCase
                });
                var apiResultJson = await _surveyTalkEmbeddingVectorService.GetSimilarCandidateIdsByEmbeddingVectorFilterTagsWithCandidateAccuracyAsync(json);
                var apiResult = JArray.Parse(apiResultJson).ToObject<List<FilterTagSimilarityComparisonResultDTO>>();
                return apiResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while getting vector encoded filter tags: {ex.Message}");
                throw new Exception("Error while getting vector encoded filter tags", ex);
            }
        }


        // Hàm tương đương filter_candidates_by_tag_similarity (dùng TargetTagFilterAccuracyRate)
        public List<FilterTagSimilarityComparisonResultDTO> FilterCandidatesByTagSimilarityByTargetAccuracy(FilterTagSimilarityComparisonRequestDTO req)
        {
            float minScore = req.MinScore;
            float maxScore = req.MaxScore;
            double? tagFilterAccuracyRate = req.TargetTagFilterAccuracyRate;
            if (tagFilterAccuracyRate == null) return new();
            var targetTagDict = req.TargetEmbeddingVectorFilterTags.ToDictionary(x => x.FilterTagId, x => x.EmbeddingVector);
            var results = new List<FilterTagSimilarityComparisonResultDTO>();
            foreach (var candidate in req.CandidateEmbeddingVectorFilterTags)
            {
                var accSimilarities = new List<float>();
                foreach (var tag in candidate.EmbeddingVectorFilterTags)
                {
                    targetTagDict.TryGetValue(tag.FilterTagId, out var targetVec);
                    var sim = CalcCosineSimilarity(targetVec, tag.EmbeddingVector, maxScore);
                    accSimilarities.Add(sim);
                }
                if (accSimilarities.Count == 0) continue;
                float avgSim = accSimilarities.Average();
                float avgAccuracy = ConvertSimilarityToAccuracy(avgSim, minScore, maxScore);
                if (avgAccuracy >= (float)(tagFilterAccuracyRate.Value / 100.0))
                {
                    results.Add(new FilterTagSimilarityComparisonResultDTO
                    {
                        CandidateId = candidate.CandidateId,
                        SimilarityScore = avgSim
                    });
                }
            }
            return results;
        }

        // Hàm tương đương filter_candidates_by_tag_similarity_by_candidate_accuracy (dùng CandidateTagFilterAccuracyRate)
        public List<FilterTagSimilarityComparisonResultDTO> FilterCandidatesByTagSimilarityByCandidateAccuracy(FilterTagSimilarityComparisonRequestDTO req)
        {
            float minScore = req.MinScore;
            float maxScore = req.MaxScore;
            var targetTagDict = req.TargetEmbeddingVectorFilterTags.ToDictionary(x => x.FilterTagId, x => x.EmbeddingVector);
            var results = new List<FilterTagSimilarityComparisonResultDTO>();
            foreach (var candidate in req.CandidateEmbeddingVectorFilterTags)
            {
                double? candidateAccuracyRate = candidate.CandidateTagFilterAccuracyRate;
                if (candidateAccuracyRate == null) continue;
                var accSimilarities = new List<float>();
                foreach (var tag in candidate.EmbeddingVectorFilterTags)
                {
                    targetTagDict.TryGetValue(tag.FilterTagId, out var targetVec);
                    var sim = CalcCosineSimilarity(targetVec, tag.EmbeddingVector, maxScore);
                    accSimilarities.Add(sim);
                }
                if (accSimilarities.Count == 0) continue;
                float avgSim = accSimilarities.Average();
                float avgAccuracy = ConvertSimilarityToAccuracy(avgSim, minScore, maxScore);
                if (avgAccuracy >= (float)(candidateAccuracyRate.Value / 100.0))
                {
                    results.Add(new FilterTagSimilarityComparisonResultDTO
                    {
                        CandidateId = candidate.CandidateId,
                        SimilarityScore = avgSim
                    });
                }
            }
            return results;
        }
    }
}
