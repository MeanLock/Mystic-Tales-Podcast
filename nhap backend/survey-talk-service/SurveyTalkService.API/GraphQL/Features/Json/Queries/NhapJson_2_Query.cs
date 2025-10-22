using SurveyTalkService.GraphQL.Schema.QueryGroups;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace SurveyTalkService.API.GraphQL.Features.Json.Queries
{
    

    public record JsonResult_Record(string Key, JsonDocument Value);

    public record JsonResult
    {
        public string Status { get; set; }
        //public object Data { get; set; }  // Dữ liệu có thể là danh sách hoặc bất kỳ kiểu dữ liệu nào
        //public object Metadata { get; set; }
    }

    [ExtendObjectType<JsonResult>]
    public class JsonResultExtension
    {
        public static List<JsonResult_Record> GetData() => new List<JsonResult_Record>
        {
            new JsonResult_Record("something",  JsonDocument.Parse("Data_888")),
            new JsonResult_Record("something",  JsonDocument.Parse("Data_999"))
        };
        public static List<JsonResult_Record> GetMetadata() => new List<JsonResult_Record>
        {
            new JsonResult_Record("something",  JsonDocument.Parse("MetaDate_888")),
            new JsonResult_Record("something",  JsonDocument.Parse("MetaDate_999"))
        };
    }

    public class NhapJson_2_Query
    {
        public JsonDocument GetComplexJson_JsonDocument()
        {
            // Tạo dữ liệu và parse nó thành JsonDocument
            var data = new
            {
                Status = "Success",
                Data = new[]
                {
                new { Key = "A", Values = new[] { "Alpha", "Beta", "Gamma" } },
                new { Key = "B", Values = new[] { "Delta", "Epsilon", "Zeta" } },
                new { Key = "C", Values = new[] { "Eta", "Theta", "Iota" } }
            },
                Metadata = new
                {
                    ProcessedBy = "Xyz Service",
                    Timestamp = DateTime.UtcNow
                }
            };

            // Serialize object to JSON string
            string jsonString = JsonSerializer.Serialize(data); 
            
            // Parse JSON string to JsonDocument
            JsonDocument jsonDocument = JsonDocument.Parse(jsonString); 
            
            // Return JsonDocument
            return jsonDocument;
        }

        public JToken GetComplexJson_JToken()
        {
            // Tạo dữ liệu và parse nó thành JToken
            var data = new
            {
                Status = "Success",
                Data = new[]
                {
                new { Key = "A", Values = new[] { "Alpha", "Beta", "Gamma" } },
                new { Key = "B", Values = new[] { "Delta", "Epsilon", "Zeta" } },
                new { Key = "C", Values = new[] { "Eta", "Theta", "Iota" } }
            },
                Metadata = new
                {
                    ProcessedBy = "Xyz Service",
                    Timestamp = DateTime.UtcNow
                }
            };

            // Serialize object to JSON string
            string jsonString = JsonSerializer.Serialize(data); 
            
            // Parse JSON string to JToken
            JToken jToken = JToken.FromObject(data); 
            
            // Return JToken
            return jToken;
        }
    }






}
