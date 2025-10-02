namespace BookingManagementService.BusinessLogic.Services.MessagingServices.interfaces
{
    public interface IHandlerRegistryService
    {
        void RegisterAllHandlers();
        void UnregisterHandler(string messageType);
        Dictionary<string, Func<string, string, Task>> GetAllHandlers();
        Dictionary<string, List<string>> GetTopicMessageTypes();
    }
}
