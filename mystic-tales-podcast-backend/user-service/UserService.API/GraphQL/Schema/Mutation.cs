using UserService.GraphQL.Schema.MutationGroups;

namespace UserService.GraphQL.Schema
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
