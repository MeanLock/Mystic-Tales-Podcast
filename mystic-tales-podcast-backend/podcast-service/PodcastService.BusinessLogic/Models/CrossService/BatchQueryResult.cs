using PodcastService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using PodcastService.DataAccess.Data;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;

namespace PodcastService.BusinessLogic.Models.CrossService
{
    public class BatchQueryResult
    {
        public JObject Results { get; set; } = new JObject();
        public long ExecutionTimeMs { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool IsSuccess => !Errors.Any();
    }
}
