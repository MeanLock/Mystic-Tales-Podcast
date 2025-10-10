using Newtonsoft.Json;

namespace UserService.BusinessLogic.Helpers.JsonHelpers
{
    public class NullableDateTimeJsonConverter : JsonConverter<DateTime?>
    {
        public override DateTime? ReadJson(JsonReader reader, Type objectType, DateTime? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            
            if (reader.TokenType == JsonToken.Null || reader.Value == null)
                return null;

            // ✅ Handle Date token type (when JSON has valid date)
            if (reader.TokenType == JsonToken.Date)
            {
                return (DateTime)reader.Value;
            }

            // ✅ Handle String token type
            if (reader.TokenType == JsonToken.String)
            {
                var stringValue = reader.Value.ToString();
                if (string.IsNullOrEmpty(stringValue) || stringValue.ToLower() == "null")
                    return null;

                if (DateTime.TryParse(stringValue, out DateTime result))
                    return result;
            }

            // ✅ Fallback: try to convert any other type
            try
            {
                return Convert.ToDateTime(reader.Value);
            }
            catch
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, DateTime? value, JsonSerializer serializer)
        {
            if (value.HasValue)
                writer.WriteValue(value.Value);
            else
                writer.WriteNull();
        }
    }
}
