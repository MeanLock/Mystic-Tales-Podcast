using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodcastService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.NewResetPassword
{
    public class NewResetPasswordParameterDTO
    {
        public required string ResetPasswordToken { get; set; }
        public required string Email { get; set; }
        public required string NewPassword { get; set; }
    }
}
