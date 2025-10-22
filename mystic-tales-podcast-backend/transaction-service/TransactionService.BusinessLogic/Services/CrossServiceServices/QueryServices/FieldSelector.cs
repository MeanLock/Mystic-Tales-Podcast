using TransactionService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using TransactionService.DataAccess.Data;
using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TransactionService.BusinessLogic.Models.CrossService;
using System.Security.Cryptography;
using System.Text;
using TransactionService.Common.AppConfigurations.SystemService.interfaces;
using System.Reflection;
using Newtonsoft.Json;

namespace TransactionService.BusinessLogic.Services.CrossServiceServices.QueryServices
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
