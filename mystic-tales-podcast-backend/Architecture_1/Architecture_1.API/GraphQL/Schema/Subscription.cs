
using Architecture_1.API.GraphQL.Features.Chat.Types;
using Architecture_1.GraphQL.Schema.QueryGroups;
using Architecture_1.GraphQL.Schema.SubscriptionGroups;

namespace Architecture_1.GraphQL.Schema
{
    public class Book
    {
        public string Title { get; set; } = default!;
        public string Author { get; set; } = default!;
    }
    public class Subscription
    {
        // public ChattingSubscription _chattingSubscription { get; }
        // public Subscription(
        //     ChattingSubscription chattingSubscription
        //     )
        // {
        //     _chattingSubscription = chattingSubscription;
        // }

        [Subscribe]
        // The topic argument must be in the format "{argument}"
        // Using string interpolation and nameof is a good way to reference the argument name properly
        [Topic($"{{{nameof(author)}}}")]
        public Book BookPublished(string author, [EventMessage] Book book)
        => book;




    }
}


