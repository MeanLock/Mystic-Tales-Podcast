using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.Snippet
{
    public class PodcastShowSnippetResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string MainImageFileKey { get; set; }
        public DateOnly ReleaseDate { get; set; }
        public bool IsReleased { get; set; }
    }
}
