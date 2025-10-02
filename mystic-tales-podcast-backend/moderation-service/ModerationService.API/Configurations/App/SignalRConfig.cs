using ScriptPalaverBE.Services.chat_services;

namespace ModerationService.API.Configurations.App
{
    public static class SignalRConfig
    {
        public static void AddSignalRConfig(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints => { endpoints.MapHub<SpeechToTextHub>("/speech-to-text-transcription"); });
        }

    }
}
