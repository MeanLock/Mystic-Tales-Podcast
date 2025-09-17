using Architecture_1.API.GraphQL.Features.Chat.Types;
using Architecture_1.GraphQL.Schema;
using HotChocolate.Subscriptions;


namespace Architecture_1.API.GraphQL.Features.Chat.Subscriptions
{
    public class ChatRoomUpdatePayload
    {
        public int AccountId { get; set; }
        public List<ChatRoom1v1> ChatRooms { get; set; }
    }

    [ExtendObjectType(typeof(Subscription))]
    public class ChatRoomSubscription
    {

        [Subscribe]
        // [Topic(nameof(UserTopic))]
        [Topic($"ChatRoom_User_{{{nameof(accountId)}}}")]
        public List<ChatRoom1v1> OnChatRoomListUpdate(
            int accountId,
            [EventMessage] List<ChatRoom1v1> chatRooms
            // [EventMessage] ChatRoomUpdatePayload payload
            )
        {
            // Console.WriteLine($"\n\nOnChatRoomListUpdate: {accountId}\n\n");
            return chatRooms;
        }
        private static string UserTopic(int accountId) => $"ChatRoom_User_{accountId}";


        //-------------------------------------------------------------------------------------
        // [Subscribe]
        // [Topic(nameof(UserTopic))]
        // public async ValueTask<List<ChatRoom1v1>> OnChatRoomListUpdate(
        //     int accountId, 
        //     [EventMessage] List<ChatRoom1v1> chatRooms)
        // {
        //     return chatRooms;
        // }

        //-------------------------------------------------------------------------------------

        // [Subscribe]
        // public ValueTask<ISourceStream<List<ChatRoom1v1>>> OnChatRoomListUpdate(
        //     int accountId,
        //     [Service] ITopicEventReceiver receiver)
        // {
        //     return receiver.SubscribeAsync<List<ChatRoom1v1>>($"User_{accountId}");
        // }


        // [Topic] // Trả dữ liệu về FE
        // public List<ChatRoom1v1> OnChatRoomListUpdate([EventMessage] List<ChatRoom1v1> chatRooms)
        // {
        //     return chatRooms;
        // }

        //--------------------------------------------------------------------------------------
        // [Subscribe(With = nameof(SubscribeToChatRooms))]
        // public List<ChatRoom1v1> OnChatRoomListUpdate(
        //     [EventMessage] List<ChatRoom1v1> chatRooms) => chatRooms;

        // public ValueTask<ISourceStream<List<ChatRoom1v1>>> SubscribeToChatRooms(
        //     int accountId,
        //     [Service] ITopicEventReceiver receiver)
        // {
        //     string topic = $"User_{accountId}";
        //     return receiver.SubscribeAsync<List<ChatRoom1v1>>(topic);
        // }





    }

    // public class ChatRoomSubscription : ObjectTypeExtension
    // {
    //     protected override void Configure(IObjectTypeDescriptor descriptor)
    //     {
    //         descriptor.Name("Subscription"); // tên type chính (giống với .AddSubscriptionType<Subscription>() ở cấu hình)

    //         descriptor
    //             .Field("onChatRoomListUpdate")
    //             .Argument("accountId", a => a.Type<NonNullType<IntType>>())
    //             .Type<ListType<ObjectType<ChatRoom1v1>>>()
    //             .Resolve(ctx =>
    //             {
    //                 var chatRooms = ctx.Parent<List<ChatRoom1v1>>();
    //                 return chatRooms;
    //             })
    //             .Subscribe(async ctx =>
    //             {
    //                 var accountId = ctx.ArgumentValue<int>("accountId");
    //                 var receiver = ctx.Service<ITopicEventReceiver>();
    //                 var topic = $"User_{accountId}";
    //                 return await receiver.SubscribeAsync<List<ChatRoom1v1>>(topic);
    //             });
    //     }
    // }

}
