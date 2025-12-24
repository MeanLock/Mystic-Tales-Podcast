using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SendResetPasswordLink
{
    public class SendResetPasswordLinkParameterDTO
    {
        public required string Email { get; set; }
    }
}
