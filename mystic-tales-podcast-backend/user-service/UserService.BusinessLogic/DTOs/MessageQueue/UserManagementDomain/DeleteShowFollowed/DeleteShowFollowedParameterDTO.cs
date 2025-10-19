using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteShowFollowed
{
    public class DeleteShowFollowedParameterDTO
    {
        public int? AccountId { get; set; }
        public required Guid PodcastShowId { get; set; }
    }
}
