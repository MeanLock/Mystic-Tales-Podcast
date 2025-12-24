using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteEpisodeSavedRollback
{
    public class DeleteEpisodeSavedRollbackParameterDTO
    {
        public List<int> AffectedAccountIds { get; set; } = new List<int>();
        public required Guid PodcastEpisodeId { get; set; }
    }
}
