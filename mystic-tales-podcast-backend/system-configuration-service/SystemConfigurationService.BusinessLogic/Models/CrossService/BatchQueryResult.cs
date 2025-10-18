using SystemConfigurationService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SystemConfigurationService.DataAccess.Data;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;

namespace SystemConfigurationService.BusinessLogic.Models.CrossService
{
        public class BatchQueryResult
    {
        public JObject Results { get; set; } = new JObject();
        public long ExecutionTimeMs { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool IsSuccess => !Errors.Any();
    }
}
