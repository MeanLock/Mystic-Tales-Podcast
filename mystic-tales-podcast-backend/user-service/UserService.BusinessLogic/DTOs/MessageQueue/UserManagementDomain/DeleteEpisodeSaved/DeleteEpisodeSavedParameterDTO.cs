using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteEpisodeSaved
{
    public class DeleteEpisodeSavedParameterDTO
    {
        public int? AccountId { get; set; }
        public required Guid PodcastEpisodeId { get; set; }
    }
}
