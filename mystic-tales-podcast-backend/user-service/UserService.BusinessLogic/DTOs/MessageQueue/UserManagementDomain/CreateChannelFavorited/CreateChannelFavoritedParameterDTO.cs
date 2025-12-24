using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateChannelFavorited
{
    public class CreateChannelFavoritedParameterDTO
    {
        public required int AccountId { get; set; }
        public required Guid PodcastChannelId { get; set; }
    }
}
