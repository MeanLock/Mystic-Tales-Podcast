
using Architecture_1.API.GraphQL.Features.Category.Queries;
using Architecture_1.API.GraphQL.Features.Chat.Queries;
using Architecture_1.API.GraphQL.Features.Product.Queries;

namespace Architecture_1.GraphQL.Schema.QueryGroups
{
    public class DbQuery
    {

        public ProductQuery _productQueries { get; } 
        public CategoryQuery _categoryQueries { get; }
        public ChatQuery _chatQueries { get; } = default!;

        public DbQuery(
            ProductQuery productQuery,
            CategoryQuery categoryQuery,
            ChatQuery chatQuery
            )
        {
            _productQueries = productQuery;
            _categoryQueries = categoryQuery;
            _chatQueries = chatQuery;

        }

    }
}


