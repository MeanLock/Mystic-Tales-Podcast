using SurveyTalkService.GraphQL.Common.Types;

namespace SurveyTalkService.API.GraphQL.Features.Json.Queries
{
    public class NhapJson_1_Query
    {
        public ComplexJsonData GetComplexJsonAbc()
        {
            return new ComplexJsonData
            {
                Title = "Sample ABC Data",
                Description = "This is a sample JSON object for AbcQueries",
                Items = new List<ComplexJsonItem>
                {
                new ComplexJsonItem  { Id = 1, Name = "Item 1", Values = new List<int> { 10, 20, 30 } },
                new ComplexJsonItem  { Id = 2, Name = "Item 2", Values = new List < int > { 40, 50, 60 } },
                new ComplexJsonItem  { Id = 3, Name = "Item 3", Values = new List < int > { 70, 80, 90 } }
            },
                Metadata = new ComplexJsonMetadata { CreatedAt = DateTime.UtcNow, Source = "Abc Service" }
            };
        }
    }
}
