using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolvePodcasterEpisodesReportNoEffectTerminatePodcasterForce
{
    public class ResolvePodcasterEpisodesReportNoEffectTerminatePodcasterForceParameterDTO
    {
        public List<Guid>? DmcaDismissedEpisodeIds { get; set; }
    }
}
