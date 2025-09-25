


using SurveyTalkService.API.GraphQL.Features.Json.Queries;

namespace SurveyTalkService.GraphQL.Schema.QueryGroups
{
    public class JsonQuery
    {
        public NhapJson_1_Query _nhapJson_1_Queries { get; }
        public NhapJson_2_Query _nhapJson_2_Queries { get; }
        public JsonQuery(
            NhapJson_1_Query nhapJson_1_Query,
            NhapJson_2_Query nhapJson_2_Query
            )
        {
            _nhapJson_1_Queries = nhapJson_1_Query;
            _nhapJson_2_Queries = nhapJson_2_Query;

        }

    }
}
