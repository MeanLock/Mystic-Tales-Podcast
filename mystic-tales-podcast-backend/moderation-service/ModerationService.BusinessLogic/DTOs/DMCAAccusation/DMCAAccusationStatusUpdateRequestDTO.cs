using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.DMCAAccusation
{
    public class DMCAAccusationStatusUpdateRequestDTO
    {
        [Required]
        public List<IFormFile> AttachmentFiles { get; set; } = new List<IFormFile>();
    }
}
