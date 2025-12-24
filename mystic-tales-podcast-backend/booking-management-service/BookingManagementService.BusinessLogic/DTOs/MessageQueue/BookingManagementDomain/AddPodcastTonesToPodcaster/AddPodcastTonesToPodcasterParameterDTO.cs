using Microsoft.Identity.Client;
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
        [Required]
        public int AccountId { get; set; }
        [Required]
        public bool IsBuddy { get; set; }
        public List<Guid> PodcastToneIds { get; set; }
    }
}
