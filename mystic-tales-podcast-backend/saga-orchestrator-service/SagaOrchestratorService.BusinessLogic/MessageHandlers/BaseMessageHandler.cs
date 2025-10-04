using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.Json;

namespace SagaOrchestratorService.BusinessLogic.MessageHandlers
{
    public abstract class BaseMessageHandler
    {
        protected readonly ILogger _logger;

        protected BaseMessageHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected T? DeserializeMessage<T>(string messageJson) where T : class
        {
            try
            {
                Console.WriteLine($"Deserializing message: {messageJson}");
                return JsonConvert.DeserializeObject<T>(messageJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize message: {MessageJson}", messageJson);
                return null;
            }
        }

        protected string SerializeToJson<T>(T data) where T : class
        {
            try
            {
                return JsonConvert.SerializeObject(data, Formatting.Indented);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to serialize data: {Data}", data);
                return "{}";
            }
        }

        protected string? ExtractResponseDataFromRequest(Dictionary<string, object> requestData)
        {
            if (requestData.TryGetValue("ResponseData", out var responseDataObj))
            {
                try
                {
                    return JsonConvert.SerializeObject(responseDataObj, Formatting.Indented);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to serialize ResponseData from RequestData");
                    return null;
                }
            }
            return null;
        }

        protected string? ExtractErrorMessageFromRequest(Dictionary<string, object> requestData)
        {
            if (requestData.TryGetValue("ErrorMessage", out var errorMessageObj))
            {
                return errorMessageObj?.ToString();
            }
            return null;
        }
    }
}
