using TransactionService.GraphQL.Schema.MutationGroups;

namespace TransactionService.GraphQL.Schema
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
