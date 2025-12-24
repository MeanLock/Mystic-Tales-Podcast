using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.LoginAccountManual
{
    public class LoginAccountManualParameterDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string DeviceInfoToken { get; set; }
    }
}
