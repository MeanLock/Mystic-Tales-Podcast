using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.Infrastructure.Models.Audio.Transcription;

namespace PodcastService.Infrastructure.Services.Audio.Transcription
{
    public class AudioTranscriptionApiService
    {
        // CONFIG
        public readonly IAppConfig _appConfig;

        // HTTP CLIENT
        private readonly IHttpClientFactory _httpClientFactory;

        public AudioTranscriptionApiService(
            IAppConfig appConfig,
            IHttpClientFactory httpClientFactory
            )
        {
            _appConfig = appConfig;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Gửi audio stream tới endpoint transcription, trả về JSON string kết quả.
        /// </summary>
        public async Task<AudioTranscriptionApiResult> TranscribeAudioAsync(Stream audioStream, string fileName = "audio.wav", string? method ="auto", int? maxParallel = 4)
        {
            try
            {
                // Reset stream position nếu có thể
                if (audioStream.CanSeek)
                    audioStream.Position = 0;

                var client = _httpClientFactory.CreateClient();
                
                // Tạo multipart form data
                using var formData = new MultipartFormDataContent();

                // settimeout vô hạn
                client.Timeout = TimeSpan.FromHours(1);
                
                // Tạo stream content cho audio file
                var streamContent = new StreamContent(audioStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");
                
                // Thêm vào form với tên "AudioFile"
                formData.Add(streamContent, "AudioFile", fileName);

                var response = await client.PostAsync(
                    _appConfig.AUDIO_TRANSCRIPTION_API_URL + "/transcribe" +
                    $"?method={method}&max_parallel={maxParallel}",
                    formData
                );
                
                response.EnsureSuccessStatusCode();
                var apiResultJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AudioTranscriptionApiResult>(apiResultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while transcribing audio: {ex.Message}");
                throw new Exception("Error while transcribing audio", ex);
            }
        }

        /// <summary>
        /// Overload để transcribe từ file path
        /// </summary>
        // public async Task<AudioTranscriptionApiResult> TranscribeAudioFromFileAsync(string filePath)
        // {
        //     try
        //     {
        //         using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        //         var fileName = Path.GetFileName(filePath);
        //         return await TranscribeAudioAsync(fileStream, fileName);
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Error while transcribing audio from file: {ex.Message}");
        //         throw new Exception("Error while transcribing audio from file", ex);
        //     }
        // }
    }
}