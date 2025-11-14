using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.UpdateAccountPassword
{
    public class UpdateAccountPasswordParameterDTO
    {
        public int AccountId { get; set; }
        public required string OldPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}
