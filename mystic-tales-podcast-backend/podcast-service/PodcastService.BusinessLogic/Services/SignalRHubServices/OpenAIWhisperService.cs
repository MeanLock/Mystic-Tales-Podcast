using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PodcastService.Infrastructure.Configurations.OpenAI.interfaces;

namespace PodcastService.BusinessLogic.Services.SignalRHubServices
{
    public class OpenAIWhisperService
    {
        private readonly ClientWebSocket _webSocket = new();
        private readonly IOpenAIConfig _openAIConfig;
        public OpenAIWhisperService(IOpenAIConfig openAIConfig)
        {
            _openAIConfig = openAIConfig;
        }

        public async Task<string> SendAudioAsync(byte[] audioData)
        {

            using var httpClient = new HttpClient();

            // Thêm header Authorization
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _openAIConfig.ApiKey);


            // Tạo nội dung multipart/form-data
            using var content = new MultipartFormDataContent();
            Console.WriteLine($"[OpenAI Whisper] Sending audio data of length {audioData.Length} bytes");
            var audioContent = new ByteArrayContent(audioData);
            audioContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/webm"); // hoặc "audio/wav" nếu đúng định dạng
            content.Add(audioContent, "file", "audio.webm");
            // thêm form data language
            content.Add(new StringContent("vi"), "language"); // Thêm ngôn ngữ nếu cần

            // Thêm các tham số bắt buộc
            content.Add(new StringContent("whisper-1"), "model");

            // Gọi API OpenAI
            var response = await httpClient.PostAsync("https://api.openai.com/v1/audio/transcriptions", content);

            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[OpenAI Whisper] Response: {responseString}");

            if (response.IsSuccessStatusCode)
            {
                // Parse kết quả và gửi về callback
                using var doc = JsonDocument.Parse(responseString);
                if (doc.RootElement.TryGetProperty("text", out var textProp))
                {
                    Console.WriteLine($"[OpenAI Whisper] Transcription: {textProp.GetString()}");
                    return textProp.GetString();
                }
            }
            else
            {
                Console.WriteLine($"[OpenAI Whisper] Error: {response.StatusCode} - {responseString}");

            }
            return "";
        }

        // public async Task<string> SendAudioAsync(byte[] audioData)
        // {
        //     string tempWebmPath = Path.GetTempFileName() + ".webm";
        //     string tempWavPath = Path.GetTempFileName() + ".wav";

        //     try
        //     {
        //         // 1. Ghi file .webm tạm
        //         await File.WriteAllBytesAsync(tempWebmPath, audioData);

        //         // 2. Convert sang WAV (16kHz, mono, PCM 16-bit)
        //         var ffmpeg = new Process
        //         {
        //             StartInfo = new ProcessStartInfo
        //             {
        //                 FileName = "ffmpeg",
        //                 Arguments = $"-y -i \"{tempWebmPath}\" -ar 16000 -ac 1 -c:a pcm_s16le \"{tempWavPath}\"",
        //                 RedirectStandardError = true,
        //                 RedirectStandardOutput = true,
        //                 UseShellExecute = false,
        //                 CreateNoWindow = true
        //             }
        //         };

        //         ffmpeg.Start();
        //         string ffmpegError = await ffmpeg.StandardError.ReadToEndAsync();
        //         ffmpeg.WaitForExit();

        //         if (ffmpeg.ExitCode != 0)
        //         {
        //             Console.WriteLine($"[FFmpeg Error] {ffmpegError}");
        //             return "";
        //         }

        //         // 3. Đọc file WAV đã convert
        //         var wavBytes = await File.ReadAllBytesAsync(tempWavPath);

        //         using var httpClient = new HttpClient();
        //         httpClient.DefaultRequestHeaders.Authorization =
        //             new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _openAIConfig.ApiKey);

        //         using var content = new MultipartFormDataContent();
        //         Console.WriteLine($"[OpenAI Whisper] Sending WAV data of length {wavBytes.Length} bytes");

        //         var audioContent = new ByteArrayContent(wavBytes);
        //         audioContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");
        //         content.Add(audioContent, "file", "audio.wav");

        //         content.Add(new StringContent("vi"), "language"); // Tiếng Việt
        //         content.Add(new StringContent("gpt-4o-transcribe"), "model");

        //         var response = await httpClient.PostAsync("https://api.openai.com/v1/audio/transcriptions", content);

        //         var responseString = await response.Content.ReadAsStringAsync();
        //         Console.WriteLine($"[OpenAI Whisper] Response: {responseString}");

        //         if (response.IsSuccessStatusCode)
        //         {
        //             using var doc = JsonDocument.Parse(responseString);
        //             if (doc.RootElement.TryGetProperty("text", out var textProp))
        //             {
        //                 return textProp.GetString();
        //             }
        //         }
        //         else
        //         {
        //             Console.WriteLine($"[OpenAI Whisper] Error: {response.StatusCode} - {responseString}");
        //         }

        //         return "";
        //     }
        //     finally
        //     {
        //         // 4. Xóa file tạm
        //         if (File.Exists(tempWebmPath)) File.Delete(tempWebmPath);
        //         if (File.Exists(tempWavPath)) File.Delete(tempWavPath);
        //     }
        // }

        public async Task DisconnectAsync()
        {
            if (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.CloseReceived)
            {
                try
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed by client", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error when closing WebSocket: {ex.Message}");
                }
            }

            _webSocket.Dispose();
        }

        private string ExtractTextFromWhisperResponse(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("type", out var typeProp) && typeProp.GetString() == "transcription")
            {
                if (root.TryGetProperty("text", out var textProp))
                {
                    return textProp.GetString();
                }
            }
            return null;
        }

        private string ExtractSessionIdFromCreated(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("type", out var typeProp) && typeProp.GetString() == "transcription_session.created")
            {
                if (root.TryGetProperty("session", out var sessionProp) &&
                    sessionProp.TryGetProperty("id", out var idProp))
                {
                    return idProp.GetString();
                }
            }
            return null;
        }
    }

}

