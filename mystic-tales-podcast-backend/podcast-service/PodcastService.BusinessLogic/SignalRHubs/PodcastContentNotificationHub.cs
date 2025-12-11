using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace PodcastService.BusinessLogic.SignalRHubs
{
    public class PodcastContentNotificationHub : Hub
    {

        private readonly ILogger<PodcastContentNotificationHub> _logger;

        public PodcastContentNotificationHub(ILogger<PodcastContentNotificationHub> logger)
        {
            _logger = logger;
        }
        public override async Task OnConnectedAsync()
        {
            // Lấy userId từ claim "id"
            var userId = Context.User?.FindFirst("id")?.Value;
            Console.WriteLine($"\n\n\nUserId from claims: {userId}\n\n\n");

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"User {userId} connected with ConnectionId: {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst("id")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);

                Console.WriteLine($"User {userId} disconnected. ConnectionId: {Context.ConnectionId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}