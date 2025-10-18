
using SurveyTalkService.GraphQL.Schema.QueryGroups;

namespace SurveyTalkService.GraphQL.Schema
{

    public class Query
    {
        //*** Chỉ các thuột tính trong lớp có return value và type của thuộc tính đó thuộc một trong số các Scalar Type được hỗ trợ bởi HotChocolate hoặc Custom Scalar Type (vd: JsonScalarType) => thì HotChocolate mới có thể quét qua để tạo type trong schema 
        public DbQuery _dbQueries { get; }
        public JsonQuery _jsonQueries { get; }

        public Query(
            DbQuery dbQuery,
            JsonQuery jsonQuery
            )
        {
            _dbQueries = dbQuery;
            _jsonQueries = jsonQuery;

        }
        

        



    }
}


