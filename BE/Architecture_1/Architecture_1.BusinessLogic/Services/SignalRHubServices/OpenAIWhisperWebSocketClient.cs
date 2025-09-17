using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Architecture_1.Infrastructure.Configurations.OpenAI.interfaces;
using Newtonsoft.Json.Linq;

namespace Architecture_1.BusinessLogic.Services.SignalRHubServices
{
    public class OpenAIWhisperWebSocketClient
    {
        private readonly ClientWebSocket _webSocket = new();
        private readonly IOpenAIConfig _openAIConfig;

        private string sessionId { get; set; }

        public OpenAIWhisperWebSocketClient(IOpenAIConfig openAIConfig)
        {
            _openAIConfig = openAIConfig;
        }

        public async Task ConnectSessionAsync()
        {
            _webSocket.Options.SetRequestHeader("Authorization", $"Bearer {_openAIConfig.ApiKey}");
            _webSocket.Options.SetRequestHeader("openai-beta", "realtime=v1");

            await _webSocket.ConnectAsync(new Uri("wss://api.openai.com/v1/realtime?intent=transcription"), CancellationToken.None);

            var startMessage = new
            {
                type = "transcription_session.update", // <--- sửa tại đây
                input_audio_format = "pcm16",
                input_audio_transcription = new
                {
                    model = "gpt-4o-transcribe", // nên cập nhật theo tài liệu mới
                    prompt = "",
                    language = "vi"
                },
                turn_detection = new
                {
                    type = "server_vad",
                    threshold = 0.5,
                    prefix_padding_ms = 300,
                    silence_duration_ms = 500
                },
                input_audio_noise_reduction = new
                {
                    type = "near_field"
                },
                include = new[] { "item.input_audio_transcription.logprobs" }
            };

            string json = JsonSerializer.Serialize(startMessage);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            Console.WriteLine("Connecting to OpenAI WebSocket...");
            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine("Đã gửi update message tới OpenAI");
        }

        public async Task ConnectAsync(Func<string, Task> onTranscriptionReceived)
        {
            try
            {
                await ConnectSessionAsync();
                Console.WriteLine("Đã gửi start message tới OpenAI");
                _ = Task.Run(async () =>
                {
                    var buffer = new byte[4096];
                    while (_webSocket.State == WebSocketState.Open)
                    {
                        Console.WriteLine("Waiting for data from OpenAI...");
                        var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"[OpenAI WS] Received: {json}");

                        var doc = JObject.Parse(json).ToObject<dynamic>();
                        if (doc.type == "transcription_session.created")
                        {
                            sessionId = ExtractSessionIdFromCreated(json);
                            Console.WriteLine($"[OpenAI WS] Session ID: {this.sessionId}");
                        }

                        // Nếu là lỗi, log ra luôn
                        Console.WriteLine($"[OpenAI WS] Type: {doc.type}");
                        if (doc.type == "error")
                        {
                            Console.WriteLine($"[OpenAI WS] Error: {json}");
                            await ConnectSessionAsync(); // Tự động kết nối lại nếu có lỗi
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to OpenAI WebSocket: {ex.Message}");
                throw;
            }
        }

        public async Task SendAudioAsync(byte[] audioData, string connectionId)
        {

            Console.WriteLine("5");
            Console.WriteLine(_webSocket.State);
            Console.WriteLine(WebSocketState.Open);
            Console.WriteLine("6");
            // Encode audio chunk thành base64
            string base64Audio = Convert.ToBase64String(audioData);

            // Tạo JSON message đúng chuẩn OpenAI
            Console.WriteLine("sessionId: " + sessionId);
            Console.WriteLine("7");
            var audioMessage = new
            {
                type = "input_audio_buffer.append",
                audio = base64Audio,
                id = sessionId
                // session = sessionId // Sử dụng sessionId đã lấy từ phản hồi trước đó
            };
            string json = JsonSerializer.Serialize(audioMessage);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            // Gửi JSON message dạng text
            await _webSocket.SendAsync(
                new ArraySegment<byte>(jsonBytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
            Console.WriteLine("Sent audio chunk to OpenAI (as base64 JSON)");

        }

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

