using BookingManagementService.BusinessLogic.Services.SignalRHubServices;
using BookingManagementService.DataAccess.Data;
using Microsoft.AspNetCore.SignalR;
using BookingManagementService.Infrastructure.Configurations.OpenAI.interfaces;

namespace ScriptPalaverBE.Services.chat_services
{
    public class TextToSpeechHub_nhap : Hub
    {
        private static readonly Dictionary<string, OpenAIWhisperWebSocketClient> _webSocketClients = new();

        private readonly ILogger<SpeechToTextHub> _logger;
        private readonly IOpenAIConfig _openAIConfig;

        public TextToSpeechHub_nhap(
            ILogger<SpeechToTextHub> logger,
            IOpenAIConfig openAIConfig
        )
        {
            _logger = logger;
            _openAIConfig = openAIConfig;
        }

        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;

            var client = new OpenAIWhisperWebSocketClient(
                _openAIConfig
            );

            await client.ConnectAsync(async (transcription) =>
            {
                await Clients.Client(connectionId).SendAsync("ReceiveTranscription", transcription);
            });

            _webSocketClients[connectionId] = client;

            Console.WriteLine($"Client connected: {connectionId}");

            await base.OnConnectedAsync();
        }

        public async Task SendAudioChunk(byte[] audioChunk)
        {
            Console.WriteLine($"Received audio chunk of length {audioChunk.Length}");

            try
            {
                byte[] byteChunk = audioChunk.Select(i => (byte)i).ToArray();
                string connectionId = Context.ConnectionId;
                Console.WriteLine($"Received audio chunk from {connectionId}, size: {audioChunk.Length} bytes");

                if (_webSocketClients.TryGetValue(connectionId, out var client))
                {
                    Console.WriteLine($"Sending ....");
                    await client.SendAudioAsync(audioChunk, connectionId);
                    Console.WriteLine($"Sent successfully");
                }
                else
                {
                    Console.WriteLine($"No client found for connection {connectionId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SendAudioChunk: {ex.Message}");
            }
        }

        public async Task SendAudioChunkBase64(string base64Audio)
        {
            byte[] audioChunk = Convert.FromBase64String(base64Audio);
            Console.WriteLine($"Received audio chunk of length {audioChunk.Length}");

            // ...xử lý như cũ...
            string connectionId = Context.ConnectionId;
            if (_webSocketClients.TryGetValue(connectionId, out var client))
            {
                Console.WriteLine($"Sending ....");
                await client.SendAudioAsync(audioChunk, connectionId);
                Console.WriteLine($"Sent successfully");
            }
        }

        public async Task SendAudioChunkList(List<byte> audioChunk)
        {
            Console.WriteLine($"Received audio chunk list of length {audioChunk.Count}");
        }
        public async Task SendAudioChunk_demo(string abc)
        // public async Task SendAudioChunk(string alo)
        {
            string connectionId = Context.ConnectionId;

            await Clients.Client(connectionId).SendAsync("ReceiveTranscription", abc);
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string connectionId = Context.ConnectionId;
            foreach (var c in _webSocketClients)
            {
                Console.WriteLine($"Client {c.Key} is connected");
            }
            if (_webSocketClients.TryGetValue(connectionId, out var client))
            {
                await client.DisconnectAsync(); // Phải có trong client
                _webSocketClients.Remove(connectionId);
            }

            Console.WriteLine($"Client disconnected: {connectionId}");

            await base.OnDisconnectedAsync(exception);
        }
    }

}
