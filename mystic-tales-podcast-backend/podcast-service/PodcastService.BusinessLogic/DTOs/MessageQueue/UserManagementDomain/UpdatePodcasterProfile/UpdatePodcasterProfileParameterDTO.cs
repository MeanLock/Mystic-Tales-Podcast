using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PodcastService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.UpdatePodcasterProfile
{
    public class UpdatePodcasterProfileParameterDTO
    {
        public int AccountId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public string BuddyAudioFileKey { get; set; }
    }
}
