using SubscriptionService.GraphQL.Schema.QueryGroups;

namespace SubscriptionService.API.GraphQL.Features.Book.Queries
{
    [ExtendObjectType(typeof(DbQuery))]
    public class BookQuery
    {
        public Types.Book GetBook() =>
        new Types.Book
        {
            Id = 1,
            Title = "GraphQL in Action",
            Author = "John Doe",
            PublishedDate = DateTime.Now
        };

        public Types.Book GetBook_NewType() =>
        new Types.Book
        {
            Id = 1,
            Title = "GraphQL in Action",
            Author = "John Doe",
            PublishedDate = DateTime.Now
        };


    }

}
