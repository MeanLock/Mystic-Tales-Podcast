using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.VerifyPodcaster
{
    public class VerifyPodcasterParameterDTO
    {
        public required int AccountId { get; set; }
        public required bool IsVerified { get; set; }
    }
}
