using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemConfigurationService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.UpdateUser
{
    public class UpdateUserParameterDTO
    {
        public int AccountId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateOnly Dob { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string MainImageFileKey { get; set; }
    }
}
