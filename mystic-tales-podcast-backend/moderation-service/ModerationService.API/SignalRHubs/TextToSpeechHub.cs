using ModerationService.BusinessLogic.Services.SignalRHubServices;
using ModerationService.DataAccess.Data;
using Microsoft.AspNetCore.SignalR;
using ModerationService.Infrastructure.Configurations.OpenAI.interfaces;

namespace ScriptPalaverBE.Services.chat_services
{
    public class SpeechToTextHub : Hub
    {
        private static readonly Dictionary<string, OpenAIWhisperWebSocketClient> _webSocketClients = new();

        private readonly ILogger<SpeechToTextHub> _logger;
        private readonly IOpenAIConfig _openAIConfig;

        private readonly OpenAIWhisperService _openAIWhisperService;

        public SpeechToTextHub(
            ILogger<SpeechToTextHub> logger,
            IOpenAIConfig openAIConfig,
            OpenAIWhisperService openAIWhisperService
        )
        {
            _logger = logger;
            _openAIConfig = openAIConfig;
            _openAIWhisperService = openAIWhisperService;
        }

        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;


            Console.WriteLine($"Client connected: {connectionId}");

            await base.OnConnectedAsync();
        }

        public async Task SendAudioChunkBase64(string base64Audio)
        {
            byte[] audioChunk = Convert.FromBase64String(base64Audio);
            Console.WriteLine($"Received audio chunk of length {audioChunk.Length}");

            // ...xử lý như cũ...
            string connectionId = Context.ConnectionId;
            string transcription = await _openAIWhisperService.SendAudioAsync(audioChunk);
            await Clients.Client(connectionId).SendAsync("ReceiveTranscription", transcription);
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

            if (exception != null)
            {
                _logger.LogError(exception, $"Client disconnected with error: {connectionId}");
            }
            else
            {
                _logger.LogInformation($"Client disconnected: {connectionId}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }

}
