
using Architecture_1.API.GraphQL.Features.Category.Mutations;
using Architecture_1.API.GraphQL.Features.Chat.Mutations;
using Architecture_1.API.GraphQL.Features.Product.Mutations;

namespace Architecture_1.GraphQL.Schema.MutationGroups
{
    public class DbMutation
    {
        public ProductMutation _productMutations { get; }
        public CategoryMutation _categoryMutations { get; }
        public ChatMutation _chatMutations { get; }

        public DbMutation(ProductMutation productMutations, CategoryMutation categoryMutations, ChatMutation chatMutations)
        {
            _chatMutations = chatMutations;
            _productMutations = productMutations;
            _categoryMutations = categoryMutations;
        }

    }
}

