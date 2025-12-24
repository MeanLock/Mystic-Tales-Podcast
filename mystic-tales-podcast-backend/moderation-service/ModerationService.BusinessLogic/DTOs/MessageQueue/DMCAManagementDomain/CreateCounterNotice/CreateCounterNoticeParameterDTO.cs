using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CreateCounterNotice
{
    public class CreateCounterNoticeParameterDTO
    {
        public int DMCAAccusationId { get; set; }
        public List<string> CounterNoticeAttachFileKeys { get; set; }
    }
}
