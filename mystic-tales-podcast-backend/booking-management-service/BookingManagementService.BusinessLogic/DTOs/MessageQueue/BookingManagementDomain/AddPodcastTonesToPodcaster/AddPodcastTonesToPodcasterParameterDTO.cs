using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AddPodcastTonesToPodcaster
{
    public class AddPodcastTonesToPodcasterParameterDTO
    {
        public int AccountId { get; set; }
        [Required]
        public List<Guid> PodcastToneIds { get; set; }
    }
}
