using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateAccountManual
{
    public class LoginAccountManualParameterDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
