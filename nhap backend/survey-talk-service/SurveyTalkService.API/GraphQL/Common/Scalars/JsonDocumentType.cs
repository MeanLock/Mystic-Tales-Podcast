

using System.Text.Json;
using HotChocolate.Language;
using HotChocolate.Types;
using System.IO;


namespace SurveyTalkService.GraphQL.Common.Scalars
{
    //** Scalar type là kiểu dữ liệu cơ bản, không phải là kiểu dữ liệu do người dùng tự định nghĩa trong HotChocolate, danh sách bao gồm: Int, Float, String, Boolean, ID, Byte, Short, Long, Decimal, DateTime, Guid, Url, UUID
    //** ScalarType <T, TNode> là một abstract class, T là kiểu dữ liệu của runtime value, TNode là kiểu dữ liệu của syntax node. Lớp này sẽ được implenment mỗi khi muốn tạo một scalar type mới
    public class JsonDocumentScalarType : ScalarType<JsonDocument, StringValueNode>
    {
        public JsonDocumentScalarType() : base("JSON_JSONDOCUMENT") { }

        public override IValueNode ParseResult(object resultValue)
        {
            return ParseValue(resultValue);
        }

        protected override JsonDocument ParseLiteral(StringValueNode valueSyntax)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            writer.WriteStringValue(valueSyntax.Value);
            writer.Flush();
            stream.Position = 0;
            return JsonDocument.Parse(stream);
        }

        protected override StringValueNode ParseValue(JsonDocument runtimeValue)
        {
            var json = runtimeValue.RootElement.GetRawText();
            return new StringValueNode(json);
        }

        public override bool TrySerialize(object runtimeValue, out object resultValue)
        {
            if (runtimeValue is JsonDocument jsonDocument)
            {
                resultValue = jsonDocument.RootElement.GetRawText();
                return true;
            }

            resultValue = null;
            return false;
        }

        public override bool TryDeserialize(object resultValue, out object runtimeValue)
        {
            if (resultValue is string json)
            {
                runtimeValue = JsonDocument.Parse(json);
                return true;
            }

            runtimeValue = null;
            return false;
        }
    }

}
