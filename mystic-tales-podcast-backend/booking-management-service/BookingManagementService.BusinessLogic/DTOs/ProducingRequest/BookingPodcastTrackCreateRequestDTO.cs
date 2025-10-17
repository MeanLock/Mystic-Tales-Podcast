using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.ProducingRequest
{
    public class BookingPodcastTrackCreateRequestDTO
    {
        public List<IFormFile> AudioFiles { get; set; } = new List<IFormFile>();
    }
}
