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
        public int AccountId { get; set; }
        public string AccountEmail { get; set; }
        public string AccountPhone { get; set; }
        public string StatementPerjury { get; set; }
        public string Jurisdiction { get; set; }
        public string Signature { get; set; }
        public DateTime FiledDate { get; set; }
        public List<string> CounterNoticeAttachFileKeys { get; set; }
    }
}
