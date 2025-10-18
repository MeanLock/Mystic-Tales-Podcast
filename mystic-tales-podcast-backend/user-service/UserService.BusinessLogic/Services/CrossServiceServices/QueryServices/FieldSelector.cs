using System.Reflection;
using Newtonsoft.Json;

namespace UserService.BusinessLogic.Services.CrossServiceServices.QueryServices
{
    // Services/FieldSelector.cs
    public class FieldSelector
    {
        public object SelectFields(object source, string[] fields)
        {
            if (!fields.Any()) return source;

            var sourceType = source.GetType();
            var result = new Dictionary<string, object>();

            foreach (var field in fields)
            {
                var property = sourceType.GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    var value = property.GetValue(source);
                    result[field] = value ?? "null";
                }
            }

            return result;
        }

        public T SelectFields<T>(T source, string[] fields) where T : class
        {
            if (!fields.Any()) return source;

            var selected = SelectFields((object)source, fields);
            var json = JsonConvert.SerializeObject(selected);
            return JsonConvert.DeserializeObject<T>(json) ?? source;
        }
    }
}
