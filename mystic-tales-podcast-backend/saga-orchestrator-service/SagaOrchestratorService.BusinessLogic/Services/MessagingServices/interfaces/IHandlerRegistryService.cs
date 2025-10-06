namespace SagaOrchestratorService.BusinessLogic.Services.MessagingServices.interfaces
{
    public interface IHandlerRegistryService
    {
        void RegisterAllHandlers();
        void UnregisterHandler(string messageName);
        Dictionary<string, Func<string, string, Task>> GetAllHandlers();
        Dictionary<string, List<string>> GetTopicMessageNames();
        //void AddRuntimeHandler(string messageType, string topic, Func<string, string, Task> handler);
        //Task RegisterYamlHandlersAsync(string? yamlPath = null);
    }
}
