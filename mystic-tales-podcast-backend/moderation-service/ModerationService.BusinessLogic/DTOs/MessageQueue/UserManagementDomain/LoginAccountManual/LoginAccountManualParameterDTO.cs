using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.LoginAccountManual
{
    public class LoginAccountManualParameterDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
