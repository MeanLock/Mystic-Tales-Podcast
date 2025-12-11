using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.UpdatePodcasterProfile
{
    public class UpdateAccountViolationLevelParameterDTO
    {
        public int AccountId { get; set; }
        public required int ViolationLevel { get; set; }
    }
}
