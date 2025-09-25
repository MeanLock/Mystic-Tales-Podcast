using Architecture_1.GraphQL.Schema.MutationGroups;

namespace Architecture_1.GraphQL.Schema
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
