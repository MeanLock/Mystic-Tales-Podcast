using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodcastService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreatePodcasterProfile
{
    public class CreatePodcasterProfileParameterDTO
    {
        public int AccountId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CommitmentDocumentFileKey { get; set; }
    }
}
