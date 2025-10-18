using ModerationService.GraphQL.Schema.MutationGroups;

namespace ModerationService.GraphQL.Schema
{
    public class Mutation
    {

        public DbMutation _dbMutations { get; }

        public Mutation(
            DbMutation dbMutations
            )
        {
            _dbMutations = dbMutations;
        }
    }
}
