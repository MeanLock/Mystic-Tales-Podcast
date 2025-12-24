using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.Snippet
{
    public class DMCAPodcastShowSnippetResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string MainImageFileKey { get; set; }
        public string PodcasterName { get; set; }
    }
}
