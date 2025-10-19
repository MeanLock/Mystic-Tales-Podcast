using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateShowFollowedRollback
{
    public class CreateShowFollowedRollbackParameterDTO
    {
        public required int AccountId { get; set; }
        public required Guid PodcastShowId { get; set; }
    }
}
