using Architecture_1.API.GraphQL.Features.Chat.Types;
using Architecture_1.GraphQL.Schema;

namespace Architecture_1.API.GraphQL.Features.Chat.Subscriptions
{
    [ExtendObjectType(typeof(Subscription))]
    public class ChatSubscription
    {
        public ChatSubscription() { }

        [Subscribe]
        // [Topic(nameof(UserTopic))] 
        [Topic($"Chat_User_{{{nameof(accountId)}}}")]
        public ChatMessage OnMessageSent(int accountId, [EventMessage] ChatMessage message)
        {
            Console.WriteLine($"\n\nOnMessageSent: {accountId}\n\n");
            return message;
        }

        private static string UserTopic(int accountId) => $"Chat_User_{accountId}";


    }

}
