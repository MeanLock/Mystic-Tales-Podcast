using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.VerifyAccount
{
    public class VerifyAccountParameterDTO
    {
        public string VerifyCode { get; set; }
        public string Email { get; set; }
    }
}
