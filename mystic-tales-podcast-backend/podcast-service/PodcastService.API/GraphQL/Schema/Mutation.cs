using PodcastService.GraphQL.Schema.MutationGroups;

namespace PodcastService.GraphQL.Schema
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
