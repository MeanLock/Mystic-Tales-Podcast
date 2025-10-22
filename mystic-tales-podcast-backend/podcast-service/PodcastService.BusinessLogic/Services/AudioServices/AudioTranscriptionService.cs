using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.Infrastructure.Models.Audio.Transcription;
using PodcastService.Infrastructure.Services.Audio.Transcription;

namespace PodcastService.BusinessLogic.Services.AudioServices
{
    public class AudioTranscriptionService
    {
        // CONFIG
        public readonly IAppConfig _appConfig;

        // HTTP CLIENT
        private readonly IHttpClientFactory _httpClientFactory;

        // Services
        private readonly AudioTranscriptionApiService _audioTranscriptionApiService;

        public AudioTranscriptionService(
            IAppConfig appConfig,
            IHttpClientFactory httpClientFactory,
            AudioTranscriptionApiService audioTranscriptionApiService
            )
        {
            _appConfig = appConfig;
            _httpClientFactory = httpClientFactory;
            _audioTranscriptionApiService = audioTranscriptionApiService;
        }

        /// <summary>
        /// Gửi audio stream tới endpoint transcription, trả về JSON string kết quả.
        /// Service trung gian gọi về Infrastructure layer.
        /// </summary>
        public async Task<string> TranscribeAudioAsync(Stream audioStream, string fileName = "audio.wav")
        {
            try
            {
                var apiResultJson = await _audioTranscriptionApiService.TranscribeAudioAsync(audioStream, fileName, "parallel", 3);
                return apiResultJson.Transcript;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while transcribing audio in BusinessLogic: {ex.Message}");
                throw new Exception("Error while transcribing audio in BusinessLogic", ex);
            }
        }

        /// <summary>
        /// Transcribe từ file path - service trung gian
        /// </summary>
        // public async Task<string> TranscribeAudioFromFileAsync(string filePath)
        // {
        //     try
        //     {
        //         var apiResultJson = await _audioTranscriptionService.TranscribeAudioFromFileAsync(filePath);
        //         return apiResultJson;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Error while transcribing audio from file in BusinessLogic: {ex.Message}");
        //         throw new Exception("Error while transcribing audio from file in BusinessLogic", ex);
        //     }
        // }

        // /// <summary>
        // /// Transcribe audio và parse thành DTO object (nếu cần)
        // /// </summary>
        // public async Task<AudioTranscriptionResultDTO> TranscribeAudioToDTOAsync(Stream audioStream, string fileName = "audio.wav")
        // {
        //     try
        //     {
        //         var apiResultJson = await _audioTranscriptionService.TranscribeAudioAsync(audioStream, fileName);
        //         var result = JsonConvert.DeserializeObject<AudioTranscriptionResultDTO>(apiResultJson, new JsonSerializerSettings
        //         {
        //             ContractResolver = new DefaultContractResolver() // PascalCase
        //         });
        //         return result;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Error while transcribing audio to DTO: {ex.Message}");
        //         throw new Exception("Error while transcribing audio to DTO", ex);
        //     }
        // }
    }
}
