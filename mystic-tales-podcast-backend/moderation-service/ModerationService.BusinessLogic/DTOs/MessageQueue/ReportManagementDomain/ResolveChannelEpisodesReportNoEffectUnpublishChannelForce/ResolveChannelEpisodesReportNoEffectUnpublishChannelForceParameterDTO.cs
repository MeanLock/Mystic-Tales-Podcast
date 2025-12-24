using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveChannelEpisodesReportNoEffectUnpublishChannelForce
{
    public class ResolveChannelEpisodesReportNoEffectUnpublishChannelForceParameterDTO
    {
        public List<Guid>? DmcaDismissedEpisodeIds { get; set; }
    }
}
