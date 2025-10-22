using Microsoft.AspNetCore.Mvc;
using PodcastService.API.Filters.ExceptionFilters;
using PodcastService.BusinessLogic.Helpers.FileHelpers;
using PodcastService.BusinessLogic.Models.CrossService;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using PodcastService.Infrastructure.Services.Audio.AcoustID;
using PodcastService.Infrastructure.Models.Audio.AcoustID;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace PodcastService.API.Controllers.QueryControllers
{
    [Route("api/nhap")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class NhapController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        private readonly FileIOHelper _fileIOHelper;
        private readonly AcoustIDAudioFingerprintGenerator _acoustIDAudioFingerprintGenerator;
        private readonly AcoustIDAudioFingerprintComparator _acoustIDAudioFingerprintComparator;

        public NhapController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient,
            FileIOHelper fileIOHelper, AcoustIDAudioFingerprintGenerator acoustIDAudioFingerprintGenerator,
            AcoustIDAudioFingerprintComparator acoustIDAudioFingerprintComparator)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _fileIOHelper = fileIOHelper;
            _acoustIDAudioFingerprintGenerator = acoustIDAudioFingerprintGenerator;
            _acoustIDAudioFingerprintComparator = acoustIDAudioFingerprintComparator;
        }

        [HttpGet("test-get-restricted-terms")]
        public async Task<IActionResult> TestGetRestrictedTerms()
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastRestrictedTerms",
                            QueryType = "findall",
                            EntityType = "PodcastRestrictedTerm",
                                Parameters = JObject.FromObject(new
                                {

                                }),
                            Fields = new[] { "Id", "Term" }
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("SystemConfigurationService", batchRequest);

            // return Ok((JArray)result.Results["podcastRestrictedTerms"]);
            List<string> terms = ((JArray)result.Results["podcastRestrictedTerms"]).Select(terms => terms["Term"].ToString()).ToList();
            return Ok(terms);
        }

        [HttpPost("generate-audio-fingerprint")]
        public async Task<IActionResult> GenerateAudioFingerprint([FromBody] string filePath)
        {
            try
            {
                using var fileStream = await _fileIOHelper.GetFileStreamAsync(filePath);
                if (fileStream == null)
                {
                    return BadRequest("File not found or could not be accessed.");
                }

                // Service đã xử lý tất cả các loại stream, chỉ cần gọi trực tiếp
                var fingerprint = await _acoustIDAudioFingerprintGenerator.GenerateFingerprintAsync(fileStream);

                return Ok(new
                {
                    fileUrl = await _fileIOHelper.GeneratePresignedUrlAsync(filePath),
                    fingerprint = fingerprint.FingerprintData
                });
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Console.WriteLine($"Error generating fingerprint: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, "Internal server error while generating fingerprint.");
            }
        }

        [HttpPost("compare-audio-fingerprints")]
        public IActionResult CompareTargetToCandidates([FromBody] AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparison comparison)
        {
            if (string.IsNullOrEmpty(comparison?.Target?.AudioFingerPrint))
            {
                return BadRequest("Target fingerprint is required.");
            }

            if (comparison.Candidates == null || !comparison.Candidates.Any())
            {
                return BadRequest("At least one candidate fingerprint is required for comparison.");
            }

            try
            {
                var result = _acoustIDAudioFingerprintComparator.CompareTargetToCandidates(comparison);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error comparing fingerprints: {ex.Message}");
                return StatusCode(500, "Internal server error while comparing fingerprints.");
            }
        }
    }
}
